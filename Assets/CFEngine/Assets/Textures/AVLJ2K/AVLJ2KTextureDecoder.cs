using CrystalFrost.Lib;
using CrystalFrost.Timing;
using Microsoft.Extensions.Logging;
using OpenMetaverse.Assets;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CrystalFrost.Assets.Textures.AVLJ2K
{
	/// <summary>
	/// A texture decoder that uses the AVLJ2K library to decode JPEG2000 textures.
	/// </summary>
	public class AVLJ2KTextureDecoder : ITextureDecoder
	{
		[DllImport("avl_j2k")]
		private static extern bool AVL_j2k_decode(System.IntPtr raw_j2k_bytes, int size, System.IntPtr pixel_buffer_out);

		[DllImport("avl_j2k")]
		private static extern int AVL_j2k_width(System.IntPtr raw_j2k_bytes, int size);

		[DllImport("avl_j2k")]
		private static extern int AVL_j2k_height(System.IntPtr raw_j2k_bytes, int size);

		[DllImport("avl_j2k")]
		private static extern int AVL_j2k_channels(System.IntPtr raw_j2k_bytes, int size);


		private readonly ILogger<AVLJ2KTextureDecoder> _log;
		private readonly ITgaReader _tgaReader;

		/// <summary>
		/// Initializes a new instance of the <see cref="AVLJ2KTextureDecoder"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		/// <param name="tgaReader">The TGA reader for converting decoded textures.</param>
		public AVLJ2KTextureDecoder(
			ILogger<AVLJ2KTextureDecoder> log,
			ITgaReader tgaReader)
		{
			_log = log;
			_tgaReader = tgaReader;
		}

		/// <summary>
		/// Decodes a JPEG2000 texture asset.
		/// </summary>
		/// <param name="texture">The texture asset to decode.</param>
		/// <returns>A task that represents the asynchronous decode operation. The task result contains the decoded texture.</returns>
		public Task<DecodedTexture> Decode(AssetTexture texture)
		{
			return Perf.Measure("AVLJ2KTextureDecoder.Decode",
				() => Task.FromResult(DecodeOpenJ2K(texture)));
		}

		private DecodedTexture DecodeOpenJ2K(AssetTexture texture)
		{
			if (texture.AssetData == null || texture.AssetData.Length == 0)
			{
				_log.LogWarning("Texture has no data " + texture.AssetID);
			}


			try
			{
				GCHandle pinnedArray = GCHandle.Alloc(texture.AssetData, GCHandleType.Pinned);
				IntPtr pointer = pinnedArray.AddrOfPinnedObject();
				var width = AVL_j2k_width(pointer, texture.AssetData.Length);
				var height = AVL_j2k_height(pointer, texture.AssetData.Length);
				var channels = AVL_j2k_channels(pointer, texture.AssetData.Length);

				if (width <= 0 || height <= 0 || channels <= 0)
				{
					_log.LogError($"Failed to decode texture {texture.AssetID}");
					pinnedArray.Free();
					return new DecodedTexture()
					{
						UUID = texture.AssetID,
						Data = new byte[] { 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127,
											127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127,
											127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127,
											127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127},
						Width = 1,
						Height = 1,
						Components = 3
					};
				}

				// allocate a buffer for the decoded texture
				var decodedTexture = new byte[width * height * channels];
				GCHandle pinnedArray2 = GCHandle.Alloc(decodedTexture, GCHandleType.Pinned);
				IntPtr pointer2 = pinnedArray2.AddrOfPinnedObject();
				if (!AVL_j2k_decode(pointer, texture.AssetData.Length, pointer2))
				{
					_log.LogError($"Failed to decode texture {texture.AssetID}");
					pinnedArray.Free();
					pinnedArray2.Free();
					throw new TextureDecodeException("Failed to decode texture");
				}
				pinnedArray2.Free();
				pinnedArray.Free();
				// _log.LogInformation($"Decoding texture {texture.AssetID} {width}x{height} {channels} channels");


				var decoded = new DecodedTexture();
				//decoded.RawData = texture.AssetData;
				decoded.Data = decodedTexture;
				decoded.Width = width;
				decoded.Height = height;
				decoded.Components = channels;
				decoded.UUID = texture.AssetID;

				if (channels != 3 && channels != 4)
				{
					// TODO, Fallback texture should come from a unfied place
					// so that it doesn't result in the creation of a new TextureTD, or Material.
					// and all objects using the fallback can use a single instance of a shared 
					// texture & material.
					return new DecodedTexture()
					{
						UUID = texture.AssetID,
						Data = new byte[] { 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127,
											127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127,
											127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127,
											127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127},
						Width = 1,
						Height = 1,
						Components = 3
					};
				}

				return decoded;
			}
			catch (Exception ex)
			{
				_log.LogError("Texture decode error: " + ex.Message);
				throw new TextureDecodeException("There was a problem decoding a texture.", ex);
			}
		}
	}
}
