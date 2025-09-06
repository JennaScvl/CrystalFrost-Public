using OpenMetaverse.Assets;
using System;
using System.Threading.Tasks;

namespace CrystalFrost.Assets.Textures
{
    /// <summary>
    /// Defines an interface for decoding texture assets.
    /// </summary>
    public interface ITextureDecoder
    {
        /// <summary>
        /// Decodes the specified texture asset.
        /// </summary>
        /// <param name="texture">The texture asset to decode.</param>
        /// <returns>A task that represents the asynchronous decode operation. The task result contains the decoded texture.</returns>
        Task<DecodedTexture> Decode(AssetTexture texture);
    }

    /// <summary>
    /// The exception that is thrown when a texture fails to decode.
    /// </summary>
    public class TextureDecodeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDecodeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TextureDecodeException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureDecodeException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public TextureDecodeException(string message, Exception inner) : base(message, inner) { }
    }
}
