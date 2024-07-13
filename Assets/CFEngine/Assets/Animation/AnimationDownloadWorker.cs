using System;
using OpenMetaverse;
using System.Collections.Generic;
using CrystalFrost.Lib;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMetaverse.Assets;
using CrystalFrost.Config;

namespace CrystalFrost.Assets.Animation
{
	public interface IAnimationDownloadWorker : IDisposable { }
	public class AnimationDownloadWorker : BackgroundWorker, IAnimationDownloadWorker
	{
		private readonly AnimationConfig _AnimationConfig;
		private readonly GridClient _client;
		private readonly IDownloadedAnimationCacheQueue _downloaded;
		private readonly IAnimationDownloadRequestQueue _requests;
		private readonly List<UUID> _pendingDownloads = new();

		public AnimationDownloadWorker(
			ILogger<AnimationDownloadWorker> log,
			IProvideShutdownSignal runningIndicator,
			GridClient client,
			IDownloadedAnimationCacheQueue downloadedAnimationQueue,
			IAnimationDownloadRequestQueue downloadRequestQueue,
			IOptions<Config.AnimationConfig> AnimationConfig
			)
			: base("AnimationDownload", 0, log, runningIndicator)
		{
			_AnimationConfig = AnimationConfig.Value;
			_client = client;
			_downloaded = downloadedAnimationQueue;
			_downloaded.ItemDequeued += Downloaded_ItemDequeued;
			_requests = downloadRequestQueue;
			_requests.ItemEnqueued += Request_ItemEnqueud;
		}

		private void Downloaded_ItemDequeued(AnimationRequest obj)
		{
			CheckForWork();
		}

		private void Request_ItemEnqueud(AnimationRequest obj)
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
				_log.LogError("Animation request UUID is zero");
				return true;
			}

			_pendingDownloads.Add(request.UUID);
			GridClient Client = Services.GetService<GridClient>();
			Simulator simulator = Client.Network.CurrentSim;
			Client.Assets.RequestAsset(request.UUID, AssetType.Animation, false, (AssetDownload transfer, Asset asset) => {
				if (asset == null)
				{

					_log.LogWarning($"Animation download failed UUID: {request.UUID}");
					return;
				}
				else
				{
					request.AssetAnimation = new AssetAnimation(asset.AssetID, asset.AssetData);
					_downloaded.Enqueue(request);
					_pendingDownloads.Remove(request.UUID);
				}
			});

			return _requests.Count > 0;
		}

		protected override bool OutputIsBacklogged()
		{
			return _downloaded.Count > _AnimationConfig.MaxDownloadedAnimations;
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
			// a cancel for Animation data.
			//_client.Assets.RequestImageCancel(uuid);
			//}
			_pendingDownloads.Clear();
		}

	}
}

