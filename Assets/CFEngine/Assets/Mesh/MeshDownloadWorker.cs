using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using CrystalFrost.Config;
using CrystalFrost.Lib;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenMetaverse;
using OpenMetaverse.Assets;
using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
	public interface IMeshDownloadWorker : IDisposable { }

	public class MeshDownloadWorker : BackgroundWorker, IMeshDownloadWorker
	{
		private readonly MeshConfig _meshConfig;
		private readonly GridClient _client;
		private readonly IDownloadedMeshCacheQueue _downloaded;
		private readonly IMeshDownloadRequestQueue _requests;
		private readonly List<UUID> _pendingDownloads = new();

		public MeshDownloadWorker(
			ILogger<MeshDownloadWorker> log,
			IProvideShutdownSignal runningIndicator,
			GridClient client,
			IDownloadedMeshCacheQueue downloadedMeshQueue,
			IMeshDownloadRequestQueue downloadRequestQueue,
			IOptions<MeshConfig> meshConfig
			)
			: base("MeshDownload", 0, log, runningIndicator)
		{
			_meshConfig = meshConfig.Value;
			_client = client;
			_downloaded = downloadedMeshQueue;
			_downloaded.ItemDequeued += Downloaded_ItemDequeued;
			_requests = downloadRequestQueue;
			_requests.ItemEnqueued += Request_ItemEnqueud;
		}

		private void Downloaded_ItemDequeued(MeshRequest obj)
		{
			CheckForWork();
		}

		private void Request_ItemEnqueud(MeshRequest obj)
		{
			CheckForWork();
		}

		protected override Task<bool> DoWork()
		{
			return Task.Run(DoWorkImpl);
		}

		private bool DoWorkImpl()
		{
			if (_requests.Count == 0) return false;
			if (!_requests.TryDequeue(out var request)) return true;
			if (request is null) return true;
			if (request.UUID == UUID.Zero)
			{
				_log.LogError("Mesh request UUID is zero");
				return true;
			}
			_pendingDownloads.Add(request.UUID);
			_client.Assets.RequestMesh(request.UUID,
				(bool success, AssetMesh assetMesh) =>
				{
					if (!success)
					{
						_log.LogWarning($"Mesh download failed UUID: {request.UUID}");
						// _requests.Enqueue(request); // Doing this has a huge downside that is if
						// the mesh download fails, it will keep retrying to download the same mesh forever.
					}
					else
					{
						request.AssetMesh = assetMesh;
						_downloaded.Enqueue(request);
						_pendingDownloads.Remove(request.UUID);
					}
				});

			return _requests.Count > 0;
		}

		protected override bool OutputIsBacklogged()
		{
			return _downloaded.Count > _meshConfig.MaxDownloadedMeshes;
		}

		protected override void ShuttingDown()
		{
			CancelPendingDownloads();
			base.ShuttingDown();
		}

		private void CancelPendingDownloads()
		{
			//foreach (var uuid in _pendingDownloads)
			//{
			// LibreMetaverse doesn't seem to have
			// a cancel for mesh data.
			//_client.Assets.RequestImageCancel(uuid);
			//}
			_pendingDownloads.Clear();
		}
	}
}
