using CrystalFrost.Lib;
using Microsoft.Extensions.Logging;
using OpenMetaverse.Assets;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrystalFrost.Config;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace CrystalFrost.Assets.Textures
{
	/// <summary>
	/// Defines an interface for a background worker that downloads texture assets.
	/// </summary>
	public interface ITextureDownloadWorker : IDisposable { }

	/// <summary>
	/// A background worker that downloads texture assets from the grid.
	/// </summary>
	public class TextureDownloadWorker : BackgroundWorker, ITextureDownloadWorker
	{
		private readonly TextureConfig _textureConfig;
		private readonly GridClient _client;
		private readonly IDownloadedTextureQueue _downloaded;
		private readonly ITextureDownloadRequestQueue _requests;
		private readonly List<UUID> _pendingDownloads = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="TextureDownloadWorker"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="runningIndicator">The provider for shutdown signals.</param>
		/// <param name="client">The grid client.</param>
		/// <param name="downloadedTextureQueue">The queue for downloaded textures.</param>
		/// <param name="downloadRequestQueue">The queue for texture download requests.</param>
		/// <param name="textureConfig">The texture configuration.</param>
		public TextureDownloadWorker(
			ILogger<TextureDownloadWorker> log,
			IProvideShutdownSignal runningIndicator,
			GridClient client,
			IDownloadedTextureQueue downloadedTextureQueue,
			ITextureDownloadRequestQueue downloadRequestQueue,
			IOptions<TextureConfig> textureConfig)
			: base("TextureDownload", 0, log, runningIndicator)
		{
			_textureConfig = textureConfig.Value;
			_client = client;
			_downloaded = downloadedTextureQueue;
			_downloaded.ItemDequeued += Downloaded_ItemDequeued;
			_requests = downloadRequestQueue;
			_requests.ItemEnqueued += Requests_ItemEnqueued;
		}

		public override void Dispose()
		{
			_downloaded.ItemDequeued -= Downloaded_ItemDequeued;
			_requests.ItemEnqueued -= Requests_ItemEnqueued;
			base.Dispose();
		}

		private void Requests_ItemEnqueued(UUID obj)
		{
			CheckForWork();
		}

		private void Downloaded_ItemDequeued(AssetTexture obj)
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

			//if (request == UUID.Zero) return true;

			if (request == UUID.Zero)
			{
				_log.LogError("Texture request UUID is zero");
				return true;
			}

			_pendingDownloads.Add(request);
			_client.Assets.RequestImage(request, TextureDownloaded);

			return _requests.Count > 0;
		}

		protected override bool OutputIsBacklogged()
		{
			return _downloaded.Count > _textureConfig.MaxDownloadedTextures;
		}

		private void TextureDownloaded(TextureRequestState state, AssetTexture assetTexture)
		{
			_log.LogDebug("Texture Downloaded " + assetTexture.AssetID);
			if (state == TextureRequestState.Finished)
			{
				_pendingDownloads.Remove(assetTexture.AssetID);
				_downloaded.Enqueue(assetTexture);
				CheckForWork();
				return;
			}

			// are there other statuses we care about? Probably.
			// problem for the future
		}

		protected override void ShuttingDown()
		{
			CancelPendingDownloads();
			base.ShuttingDown();
		}

		private void CancelPendingDownloads()
		{
			foreach (var uuid in _pendingDownloads)
			{
				_client.Assets.RequestImageCancel(uuid);
			}
			_pendingDownloads.Clear();
		}
	}
}
