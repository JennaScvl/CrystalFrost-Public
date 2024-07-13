using System;
using System.Threading.Tasks;

using CrystalFrost.Assets.Textures;
using CrystalFrost.Config;
using CrystalFrost.Lib;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OpenMetaverse.Assets;

namespace CrystalFrost.Assets
{
	public interface ITextureDecodeWorker : IDisposable { }

	public class TextureDecodeWorker : BackgroundWorker, ITextureDecodeWorker
	{
		private readonly ITextureDecoder _decoder;
		private readonly IDownloadedTextureQueue _downloadedTextureQueue;
		private readonly IDecodedTextureCacheQueue _readyTextureQueue;
		private readonly TextureConfig _textureConfig;

		public TextureDecodeWorker(
			ILogger<TextureDecodeWorker> log,
			IProvideShutdownSignal runningIndicator,
			ITextureDecoder decoder,
			IDecodedTextureCacheQueue readyTextureQueue,
			IDownloadedTextureQueue downloadedTextureQueue,
			IOptions<TextureConfig> textureConfig)
			: base("TextureDecode", 0, log, runningIndicator)
		{
			_textureConfig = textureConfig.Value;
			_decoder = decoder;
			_downloadedTextureQueue = downloadedTextureQueue;
			_downloadedTextureQueue.ItemEnqueued += DownloadedTextureQueue_ItemEnqueued;
			_readyTextureQueue = readyTextureQueue;
			_readyTextureQueue.ItemDequeued += ReadyTextureQueue_ItemDequeued;
		}

		private void ReadyTextureQueue_ItemDequeued(DecodedTexture obj)
		{
			CheckForWork();
		}

		private void DownloadedTextureQueue_ItemEnqueued(AssetTexture obj)
		{
			CheckForWork();
		}

		protected override async Task<bool> DoWork()
		{
			if (_downloadedTextureQueue.Count == 0) return false;

			if (!_downloadedTextureQueue.TryDequeue(out var texture)) return true;

			var decoded = await _decoder.Decode(texture);

			_readyTextureQueue.Enqueue(decoded);

			return _downloadedTextureQueue.Count > 0; // there is more work to do.
		}

		protected override bool OutputIsBacklogged()
		{
			// return true if there are enough pending
			// textures to be loaded into the GPU.
			return _readyTextureQueue.Count > _textureConfig.MaxReadyTextures;
		}

		public override void Dispose()
		{
			_downloadedTextureQueue.ItemEnqueued -= DownloadedTextureQueue_ItemEnqueued;
			_readyTextureQueue.ItemDequeued -= ReadyTextureQueue_ItemDequeued;
			base.Dispose();
		}
	}
}
