using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMetaverse.Assets;
using System.Threading.Tasks;
using CrystalFrost.Lib;
using System.IO;
using CrystalFrost.Client.Credentials;
using CrystalFrost.Config;
using CrystalFrost.Assets.Textures;

namespace CrystalFrost.Assets.Animation
{
	/// <summary>
	/// Defines an interface for a background worker that manages animation caching.
	/// </summary>
	public interface IAnimationCacheWorker : IDisposable { }

	/// <summary>
	/// A background worker that manages loading and saving animation assets to a local cache.
	/// </summary>
	public class AnimationCacheWorker : BackgroundWorker, IAnimationCacheWorker
	{
		private readonly AnimationConfig _animationConfig;
		private readonly IDownloadedAnimationQueue _downloadedAnimationQueue;
		private readonly IAnimationRequestQueue _animationRequestQueue;
		private readonly IAnimationDownloadRequestQueue _downloadRequestQueue;
		private readonly IDownloadedAnimationCacheQueue _downloadedCacheQueue;
		private readonly IAesEncryptor _encryptor;
		private bool _isCachingAllowed;
		private string _cachePath;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationCacheWorker"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="runningIndicator">The provider for shutdown signals.</param>
		/// <param name="aesAnimationEncryptor">The encryptor for animation data.</param>
		/// <param name="downloaded">The queue for downloaded animations.</param>
		/// <param name="downloadRequestQueue">The queue for animation download requests.</param>
		/// <param name="animationRequestQueue">The queue for animation requests.</param>
		/// <param name="downloadedCache">The queue for downloaded animations to be cached.</param>
		/// <param name="animationConfig">The animation configuration.</param>
		public AnimationCacheWorker(
			ILogger<IAnimationCacheWorker> log,
			IProvideShutdownSignal runningIndicator,
			IAesEncryptor aesAnimationEncryptor,
			IDownloadedAnimationQueue downloaded,
			IAnimationDownloadRequestQueue downloadRequestQueue,
			IAnimationRequestQueue animationRequestQueue,
			IDownloadedAnimationCacheQueue downloadedCache,
			IOptions<AnimationConfig> animationConfig)
			: base("AnimationCache", 1, log, runningIndicator)
		{
			_animationConfig = animationConfig.Value;
			_encryptor = aesAnimationEncryptor;
			_cachePath = _animationConfig.GetCachePath();
			_isCachingAllowed = _animationConfig.isCachingAllowed;
			if (!Directory.Exists(_cachePath))
			{
				Directory.CreateDirectory(_cachePath);
			}

			_downloadedAnimationQueue = downloaded;
			_animationRequestQueue = animationRequestQueue;
			_downloadRequestQueue = downloadRequestQueue;
			_downloadedCacheQueue = downloadedCache;

			_animationRequestQueue.ItemEnqueued += WorkItemEnqueued;
			_downloadedCacheQueue.ItemEnqueued += WorkItemEnqueued;
			_animationRequestQueue.ItemDequeued += WorkItemEnqueued;
			_downloadedCacheQueue.ItemDequeued += WorkItemEnqueued;
		}

		private void WorkItemEnqueued(AnimationRequest obj)
		{
			CheckForWork();
		}

		protected override Task<bool> DoWork()
		{
			if (!_isCachingAllowed)
			{
				return Task.Run(() =>
				{
					// Just pass through the mesh requests through queues
					bool resultLoad = DoWorkImplPassThroughLoad(); // Request Queue -> Download Request Queue (skip cache)
					bool resultSave = DoWorkImplPassThroughSave(); // Downloaded Cache Queue -> Downloaded Texture Queue (skip cache)
					return resultLoad || resultSave;
				});
			}
			else
			{
				return Task.Run(() =>
				{
					bool resultLoad = DoWorkImplLoadCache(); // Request Queue -> (cache check) (A - cache exists) -> Downloaded Texture Queue (skip download) (B - cache miss) -> Download Request Queue
					bool resultSave = DoWorkImplSaveCache(); // Downloaded Cache Queue -> (cache save) -> Downloaded Texture Queue
					return resultLoad || resultSave;
				});
			}
		}

		private bool DoWorkImplPassThroughLoad()
		{
			if (_animationRequestQueue.Count == 0) return false;
			if (!_animationRequestQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			_downloadRequestQueue.Enqueue(request);
			return _animationRequestQueue.Count > 0;
		}

		private bool DoWorkImplPassThroughSave()
		{
			if (_downloadedCacheQueue.Count == 0) return false;
			if (!_downloadedCacheQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			_downloadedAnimationQueue.Enqueue(request);
			return _downloadedCacheQueue.Count > 0;
		}



		private bool DoWorkImplLoadCache()
		{
			if (_animationRequestQueue.Count == 0) return false;
			if (!_animationRequestQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			var cachePath = Path.Combine(_cachePath, request.UUID.ToString() + ".asset");
			//_log.LogError($"Checking cache for {request.UUID} at {cachePath}");

			if (!File.Exists(cachePath)) // Animation is not cached, pass it to download queue
			{
				_downloadRequestQueue.Enqueue(request);
			}
			else // load cached Animation
			{
				using (var stream = File.OpenRead(cachePath))
				{
					var encryptedData = new byte[stream.Length];
					stream.Read(encryptedData, 0, encryptedData.Length);
					var decryptedData = _encryptor.Decrypt(encryptedData);
					request.AssetAnimation = new AssetAnimation(request.UUID, decryptedData);
					_downloadedAnimationQueue.Enqueue(request);
				}
			}
			return _animationRequestQueue.Count > 0;
		}

		private bool DoWorkImplSaveCache()
		{
			if (_downloadedCacheQueue.Count == 0) return false;
			if (!_downloadedCacheQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			var cachePath = Path.Combine(_cachePath, request.UUID.ToString() + ".asset");
			if (!File.Exists(cachePath))
			{
				using (var stream = File.Create(cachePath))
				{
					var encryptedData = _encryptor.Encrypt(request.AssetAnimation.AssetData);
					stream.Write(encryptedData, 0, encryptedData.Length);
				}
			}

			_downloadedAnimationQueue.Enqueue(request);
			return _downloadedCacheQueue.Count > 0;
		}


		protected override bool OutputIsBacklogged()
		{
			//return _downloaded.Count > _AnimationConfig.MaxDownloadedAnimationes;
			return false; // Temporary measure for now
		}

		protected override void ShuttingDown()
		{
			base.ShuttingDown();
		}

	}
}

