using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;

namespace CrystalFrost.Assets.Animation
{
	/// <summary>
	/// Defines an interface for a manager that handles animation assets.
	/// </summary>
	public interface IAnimationManager : IDisposable
	{
		/// <summary>
		/// Requests an animation asset.
		/// </summary>
		/// <param name="primitive">The primitive associated with the animation.</param>
		/// <param name="animationId">The UUID of the animation asset.</param>
		public void RequestAnimation(Primitive primitive, UUID animationId);
	}

	/// <summary>
	/// Manages the entire lifecycle of animation assets, from requesting and downloading to decoding and caching.
	/// </summary>
	public class AnimationManager : IAnimationManager
	{
		private readonly ILogger<AnimationManager> _log;
		private readonly IAnimationRequestQueue _requestQueue;
		private readonly IAnimationDownloadWorker _downloadWorker;
		private readonly IAnimationDecodeWorker _decodeWorker;
		private readonly IAnimationCacheWorker _animationCache;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnimationManager"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="requestQueue">The queue for animation requests.</param>
		/// <param name="downloadWorker">The worker for downloading animations.</param>
		/// <param name="decodeWorker">The worker for decoding animations.</param>
		/// <param name="animationCache">The worker for caching animations.</param>
		public AnimationManager(ILogger<AnimationManager> log,
			IAnimationRequestQueue requestQueue,
			IAnimationDownloadWorker downloadWorker,
			IAnimationDecodeWorker decodeWorker,
			IAnimationCacheWorker animationCache)
		{
			this._log = log;
			this._requestQueue = requestQueue;
			this._downloadWorker = downloadWorker;
			this._decodeWorker = decodeWorker;
			this._animationCache = animationCache;
		}

		/// <summary>
		/// Enqueues a request for an animation asset.
		/// </summary>
		/// <param name="primitive">The primitive associated with the animation.</param>
		/// <param name="animationId">The UUID of the animation asset.</param>
		public void RequestAnimation(Primitive primitive, UUID animationId)
		{
			//_log.LogInformation($"Request AnimationId: {animationId}");
			AnimationRequest request = new AnimationRequest
			{
				Primitive = primitive,
				UUID = animationId
			};
			this._requestQueue.Enqueue(request);

		}

		void IDisposable.Dispose()
		{
			_animationCache.Dispose();
			_decodeWorker.Dispose();
			_downloadWorker.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}

