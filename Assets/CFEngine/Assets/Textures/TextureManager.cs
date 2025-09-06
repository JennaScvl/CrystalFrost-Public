using CrystalFrost.Assets.Mesh;
using CrystalFrost.Assets.Textures;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;
using System.Collections.Generic;

namespace CrystalFrost.Assets
{
	/// <summary>
	/// Defines an interface for a manager that handles texture assets.
	/// </summary>
	public interface ITextureManager
	{
		/// <summary>
		/// Requests that the texture manager download
		/// and decode the desired image asset.
		/// </summary>
		/// <param name="uuid"></param>
		void RequestImage(UUID uuid);
		/// <summary>
		/// Gets the queue of decoded textures that are ready for use.
		/// </summary>
		IReadyTextureQueue ReadyTextures { get; }
	}

	/// <summary>
	/// Manages the entire lifecycle of texture assets, from requesting and downloading to decoding and caching.
	/// </summary>
	public class TextureManager : ITextureManager, IDisposable
	{
		private readonly ILogger<TextureManager> _log;
		private readonly ITextureRequestQueue _textureRequests;

		public IReadyTextureQueue ReadyTextures { get; }

		private readonly ITextureDecodeWorker _decodeWorker;
		private readonly ITextureDownloadWorker _downloadWorker;
		private readonly ITextureCacheWorker _textureCache;


		private readonly List<UUID> _pending = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="TextureManager"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="readyTextureQueue">The queue for ready textures.</param>
		/// <param name="textureRequests">The queue for texture requests.</param>
		/// <param name="decodeWorker">The worker for decoding textures.</param>
		/// <param name="downloadWorker">The worker for downloading textures.</param>
		/// <param name="textureCache">The worker for caching textures.</param>
		public TextureManager(
			ILogger<TextureManager> log,
			IReadyTextureQueue readyTextureQueue,
			ITextureRequestQueue textureRequests,
			ITextureDecodeWorker decodeWorker,
			ITextureDownloadWorker downloadWorker,
			ITextureCacheWorker textureCache)
		{
			_log = log;
			_textureRequests = textureRequests;
			_textureCache = textureCache;
			ReadyTextures = readyTextureQueue;

			// we don't really use these directly, but we
			// want them to exist so that they will subscribed to queue
			// events and do things when things are enqueud.
			_decodeWorker = decodeWorker;
			_downloadWorker = downloadWorker;

			ReadyTextures.ItemDequeued += ReadyTextures_ItemDequeued;
		}

		private void ReadyTextures_ItemDequeued(DecodedTexture decoded)
		{
			lock (_pending)
			{
				_pending.Remove(decoded.UUID);
			}
			_log.PendingTextureRemoved(decoded.UUID);
		}

		public void RequestImage(UUID uuid)
		{
			_log.TextureRequested(uuid);
			_textureRequests.Enqueue(uuid);
		}

		public void Dispose()
		{
			_decodeWorker.Dispose();
			_downloadWorker.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
