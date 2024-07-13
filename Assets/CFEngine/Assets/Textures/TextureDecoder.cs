using OpenMetaverse.Assets;
using System;
using System.Threading.Tasks;

namespace CrystalFrost.Assets.Textures
{

    public interface ITextureDecoder
    {
        Task<DecodedTexture> Decode(AssetTexture texture);
    }

    public class TextureDecodeException : Exception
    {
        public TextureDecodeException(string message) : base(message) { }
        public TextureDecodeException(string message, Exception inner) : base(message, inner) { }
    }
}
