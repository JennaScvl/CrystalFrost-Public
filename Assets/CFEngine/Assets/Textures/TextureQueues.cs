using CrystalFrost.Assets.Mesh;
using CrystalFrost.Lib;
using OpenMetaverse;
using OpenMetaverse.Assets;

namespace CrystalFrost.Assets.Textures
{
    /// <summary>
    /// A queue for textures that have been downloaded, and need to be decoded.
    /// </summary>
    public interface IDownloadedTextureQueue : IConcurrentQueue<AssetTexture> { }

    public class TextureQueues : AbstractedConcurrentQueue<AssetTexture>, IDownloadedTextureQueue { }


	/// <summary>
	/// A queue for meshes that have been decoded but not cached.
	/// </summary>
	public interface IDecodedTextureCacheQueue : IConcurrentQueue<DecodedTexture> { }

	public class DownloadedTextureCacheQueue : AbstractedConcurrentQueue<DecodedTexture>, IDecodedTextureCacheQueue { }


	/// <summary>
	/// Represents a queue of decoded textures that are ready for
	/// the main thread to pick up and read into Graphics memory.
	/// </summary>
	public interface IReadyTextureQueue : IConcurrentQueue<DecodedTexture> { }

    public class ReadyTextureQueue : AbstractedConcurrentQueue<DecodedTexture>, IReadyTextureQueue { }

    /// <summary>
    /// A queue for textures that have not been downloaded.
    /// </summary>
    public interface ITextureDownloadRequestQueue : IConcurrentQueue<UUID> { }

    public class TextureDownloadRequestQueue : AbstractedConcurrentQueue<UUID> , ITextureDownloadRequestQueue{ }


	/// <summary>
	/// A queue for textures that have been requested (from cache or downloads)
	/// </summary>
	public interface ITextureRequestQueue : IConcurrentQueue<UUID> { }

	public class TextureRequestQueue : AbstractedConcurrentQueue<UUID>, ITextureRequestQueue { }
}
