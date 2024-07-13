using System;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using OpenMetaverse.Assets;
using System.IO;
using CrystalFrost.Client.Credentials;
using System.Collections.Concurrent;
using CrystalFrost.Config;
using CrystalFrost.Lib;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using OpenMetaverse.Packets;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrystalFrost.Assets.Mesh
{
	public interface IMeshCacheWorker : IDisposable { }

	public class MeshCacheWorker : BackgroundWorker, IMeshCacheWorker
	{
		private readonly MeshConfig _meshConfig;
		private readonly IDownloadedMeshQueue _downloadedMeshQueue;
		private readonly IMeshRequestQueue _meshRequestQueue;
		private readonly IMeshDownloadRequestQueue _downloadRequestQueue;
		private readonly IDownloadedMeshCacheQueue _downloadedCacheQueue;
		private readonly IAesEncryptor _encryptor;
		private bool _isCachingAllowed;
		private string _cachePath;

		public MeshCacheWorker(
			ILogger<IMeshCacheWorker> log,
			IProvideShutdownSignal runningIndicator,
			IAesEncryptor aesMeshEncryptor,
			IDownloadedMeshQueue downloaded,
			IMeshDownloadRequestQueue downloadRequestQueue,
			IMeshRequestQueue meshRequestQueue,
			IDownloadedMeshCacheQueue downloadedCache,
			IOptions<MeshConfig> meshConfig)
			: base("MeshCache", 1, log, runningIndicator)
		{
			_meshConfig = meshConfig.Value;
			_encryptor = aesMeshEncryptor;
			_cachePath = _meshConfig.GetCachePath();
			_isCachingAllowed = _meshConfig.isCachingAllowed;
			if (!Directory.Exists(_cachePath))
			{
				Directory.CreateDirectory(_cachePath);
			}

			_downloadedMeshQueue = downloaded;
			_meshRequestQueue = meshRequestQueue;
			_downloadRequestQueue = downloadRequestQueue;
			_downloadedCacheQueue = downloadedCache;

			_meshRequestQueue.ItemEnqueued += WorkItemEnqueued;
			_downloadedCacheQueue.ItemEnqueued += WorkItemEnqueued;
			_meshRequestQueue.ItemDequeued += WorkItemEnqueued;
			_downloadedCacheQueue.ItemDequeued += WorkItemEnqueued;

		}

		private void WorkItemEnqueued(MeshRequest obj)
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
						bool resultSave = DoWorkImplPassThroughSave(); // Downloaded Cache Queue -> Downloaded Mesh Queue (skip cache)
						return resultLoad || resultSave;
					});
			}
			else
			{
				return Task.Run(() =>
				{
					bool resultLoad = DoWorkImplLoadCache(); // Request Queue -> (cache check) (A - cache exists) -> Downloaded Mesh Queue (skip download) (B - cache miss) -> Download Request Queue
					bool resultSave = DoWorkImplSaveCache(); // Downloaded Cache Queue -> (cache save) -> Downloaded Mesh Queue
					return resultLoad || resultSave;
				});
			}
		}

		private bool DoWorkImplPassThroughLoad()
		{
			if (_meshRequestQueue.Count == 0) return false;
			if (!_meshRequestQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			_downloadRequestQueue.Enqueue(request);
			return _meshRequestQueue.Count > 0;
		}

		private bool DoWorkImplPassThroughSave()
		{
			if (_downloadedCacheQueue.Count == 0) return false;
			if (!_downloadedCacheQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			_downloadedMeshQueue.Enqueue(request);
			return _downloadedCacheQueue.Count > 0;
		}

		private bool DoWorkImplLoadCache()
		{
			if (_meshRequestQueue.Count == 0) return false;
			if (!_meshRequestQueue.TryDequeue(out var request)) { return true; }
			if (request == null) return true;
			var cachePath = Path.Combine(_cachePath, request.UUID.ToString() + ".asset");
			if (!File.Exists(cachePath)) // mesh is not cached, pass it to download queue
			{
				_downloadRequestQueue.Enqueue(request);
			}
			else // load cached mesh
			{
				using (var stream = File.OpenRead(cachePath))
				{
					var encryptedData = new byte[stream.Length];
					stream.Read(encryptedData, 0, encryptedData.Length);
					var decryptedData = _encryptor.Decrypt(encryptedData);
					request.AssetMesh = new AssetMesh(request.UUID, decryptedData);
					_downloadedMeshQueue.Enqueue(request);
				}
			}
			return _meshRequestQueue.Count > 0;
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
					var encryptedData = _encryptor.Encrypt(request.AssetMesh.AssetData);
					stream.Write(encryptedData, 0, encryptedData.Length);
				}
			}
			_downloadedMeshQueue.Enqueue(request);
			return _downloadedCacheQueue.Count > 0;
		}


		protected override bool OutputIsBacklogged()
		{
			//return _downloaded.Count > _meshConfig.MaxDownloadedMeshes;
			return false; // Temporary measure for now
		}

		protected override void ShuttingDown()
		{
			base.ShuttingDown();
		}

	}
}

