
using System;
using Microsoft.Extensions.Logging;
using OpenMetaverse;
using OpenMetaverse.Assets;
using UnityEngine;
using System.IO;
using CrystalFrost.Client.Credentials;
using CrystalFrost.Config;
using Microsoft.Extensions.Options;
using CrystalFrost.Assets.Mesh;
using CrystalFrost.Lib;
using CrystalFrost.Assets.Textures;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Paddings;
using CommandLine;

namespace CrystalFrost
{
	/// <summary>
	/// Defines an interface for a background worker that manages texture caching.
	/// </summary>
	public interface ITextureCacheWorker : IDisposable { }

	/// <summary>
	/// A background worker that manages loading and saving texture assets to a local cache.
	/// </summary>
	public class TextureCacheWorker : BackgroundWorker, ITextureCacheWorker
	{
		private readonly TextureConfig _textureConfig;
		private readonly IReadyTextureQueue _readyTextureQueue;
		private readonly ITextureRequestQueue _textureRequestQueue;
		private readonly ITextureDownloadRequestQueue _downloadRequestQueue;
		private readonly IDecodedTextureCacheQueue _decodedCacheQueue;
		private readonly IAesEncryptor _encryptor;

		private bool _isCachingAllowed;
		private string _cachePath;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextureCacheWorker"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="runningIndicator">The provider for shutdown signals.</param>
		/// <param name="aesMeshEncryptor">The encryptor for texture data.</param>
		/// <param name="readyTextureQueue">The queue for ready textures.</param>
		/// <param name="downloadRequestQueue">The queue for texture download requests.</param>
		/// <param name="meshRequestQueue">The queue for texture requests.</param>
		/// <param name="decodedCache">The queue for decoded textures to be cached.</param>
		/// <param name="textureConfig">The texture configuration.</param>
		public TextureCacheWorker(
			ILogger<IMeshCacheWorker> log,
			IProvideShutdownSignal runningIndicator,
			IAesEncryptor aesMeshEncryptor,
			IReadyTextureQueue readyTextureQueue,
			ITextureDownloadRequestQueue downloadRequestQueue,
			ITextureRequestQueue meshRequestQueue,
			IDecodedTextureCacheQueue decodedCache,
			IOptions<TextureConfig> textureConfig)
			: base("TextureCache", 1, log, runningIndicator)
		{
			_textureConfig = textureConfig.Value;
			_encryptor = aesMeshEncryptor;
			_cachePath = _textureConfig.getCachePath();
			_isCachingAllowed = _textureConfig.isCachingAllowed;

			if (!Directory.Exists(_cachePath))
			{
				Directory.CreateDirectory(_cachePath);
			}

			_readyTextureQueue = readyTextureQueue;
			_textureRequestQueue = meshRequestQueue;
			_downloadRequestQueue = downloadRequestQueue;
			_decodedCacheQueue = decodedCache;

			_textureRequestQueue.ItemEnqueued += WorkItemEnqueued;
			_decodedCacheQueue.ItemEnqueued += WorkItemEnqueued;
			_textureRequestQueue.ItemDequeued += WorkItemEnqueued;
			_decodedCacheQueue.ItemDequeued += WorkItemEnqueued;
		}


		private void WorkItemEnqueued(DecodedTexture obj)
		{
			CheckForWork();
		}


		private void WorkItemEnqueued(UUID obj)
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
					bool resultLoad = false;
					try
					{
						resultLoad = DoWorkImplLoadCache(); // Request Queue -> (cache check) (A - cache exists) -> Downloaded Texture Queue (skip download) (B - cache miss) -> Download Request Queue
					}
					catch (Exception e)
					{
						_log.LogError("Error in DoWorkImplLoadCache :" + e.ToString());
					}
					bool resultSave = DoWorkImplSaveCache(); // Downloaded Cache Queue -> (cache save) -> Downloaded Texture Queue
					return resultLoad || resultSave;
				});
			}
		}

		private bool DoWorkImplPassThroughLoad()
		{
			if (_textureRequestQueue.Count == 0) return false;
			if (!_textureRequestQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			_downloadRequestQueue.Enqueue(request);
			return _textureRequestQueue.Count > 0;
		}

		private bool DoWorkImplPassThroughSave()
		{
			if (_decodedCacheQueue.Count == 0) return false;
			if (!_decodedCacheQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			//_downloadedTextureQueue.Enqueue(request);
			_readyTextureQueue.Enqueue(request);
			return _decodedCacheQueue.Count > 0;
		}

		private DecodedTexture DeserializeDecodedTexture(byte[] data)
		{
			if (data == null) return null;
			var texture = new DecodedTexture();
			using (var stream = new MemoryStream(data))
			{
				using (var reader = new BinaryReader(stream))
				{
					var uuid = reader.ReadBytes(16);
					texture.UUID = new UUID(uuid, 0);
					texture.Width = reader.ReadInt32();
					texture.Height = reader.ReadInt32();
					texture.Components = reader.ReadInt32();
					var size = texture.Width * texture.Height * texture.Components;
					texture.Data = reader.ReadBytes((int)size);
				}
			}
			return texture;
		}

		private byte[] SerializeDecodedTexture(DecodedTexture texture)
		{
			if (texture == null) return null;
			using (var stream = new MemoryStream())
			{
				using (var writer = new BinaryWriter(stream))
				{
					writer.Write(texture.UUID.GetBytes());
					writer.Write(texture.Width);
					writer.Write(texture.Height);
					writer.Write(texture.Components);
					writer.Write(texture.Data);
				}
				return stream.ToArray();
			}
		}

		private bool DoWorkImplLoadCache()
		{
			if (_textureRequestQueue.Count == 0) return false;
			if (!_textureRequestQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;
			var cachePath = Path.Combine(_cachePath, request.ToString() + ".asset");

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
					// var asset = new AssetTexture(request, decryptedData);
					//_downloadedTextureQueue.Enqueue(asset);
					var texture = DeserializeDecodedTexture(decryptedData);
					_readyTextureQueue.Enqueue(texture);
				}
			}
			return _textureRequestQueue.Count > 0;
		}

		private bool DoWorkImplSaveCache()
		{
			if (_decodedCacheQueue.Count == 0) return false;
			if (!_decodedCacheQueue.TryDequeue(out var request)) return true;
			if (request == null) return true;

			_readyTextureQueue.Enqueue(request);

			// var cachePath = Path.Combine(_cachePath, request.AssetID.ToString() + ".asset");
			var cachePath = Path.Combine(_cachePath, request.UUID.ToString() + ".asset");
			if (!File.Exists(cachePath))
			{
				using (var stream = File.Create(cachePath))
				{
					var encryptedData = _encryptor.Encrypt(SerializeDecodedTexture(request));
					stream.Write(encryptedData, 0, encryptedData.Length);
				}
			}
			// _downloadedTextureQueue.Enqueue(request);
			return _decodedCacheQueue.Count > 0;
		}


		protected override bool OutputIsBacklogged()
		{
			return false; // Temporary measure for now
		}

		protected override void ShuttingDown()
		{
			base.ShuttingDown();
		}

	}
}
