using CSJ2K;
using OpenMetaverse.Assets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Net.Http;
using CrystalFrost.Timing;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
    /// <summary>
    /// A texture decoder that uses the CSJ2K library to decode JPEG2000 textures.
    /// </summary>
    public class CSJ2KTextureDecoder : ITextureDecoder
    {
        private readonly ILogger<CSJ2KTextureDecoder> _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="CSJ2KTextureDecoder"/> class.
        /// </summary>
        /// <param name="log">The logger for recording messages.</param>
        public CSJ2KTextureDecoder(
            ILogger<CSJ2KTextureDecoder> log)
        {
            _log = log;
        }

        /// <summary>
        /// Decodes a JPEG2000 texture asset.
        /// </summary>
        /// <param name="texture">The texture asset to decode.</param>
        /// <returns>A task that represents the asynchronous decode operation. The task result contains the decoded texture.</returns>
        public Task<DecodedTexture> Decode(AssetTexture texture)
        {
            return Perf.Measure("CSJ2KTextureDecoder.Decode", () => DecodeImpl(texture));
        }

        /// <summary>
        /// Decodes a JPEG2000 texture asset.
        /// </summary>
        /// <param name="texture">The texture asset to decode.</param>
        /// <returns>A task that represents the asynchronous decode operation. The task result contains the decoded texture.</returns>
        public Task<DecodedTexture> DecodeImpl(AssetTexture texture)
        {
            RawBytesImageCreator.Register();

            var result = new DecodedTexture
            {
                UUID = texture.AssetID
            };

            var pi = J2kImage.FromBytes(texture.AssetData);
            if (pi == null)
            {
                _log.LogDebug($"pi is null");
                throw new TextureDecodeException("pi is null");
            }

            if (pi.NumberOfComponents < 3 || pi.NumberOfComponents > 4)
            {
                throw new TextureDecodeException($"Invalid number of components in texture: {pi.NumberOfComponents}");
            }
            
            var raw = pi.As<RawBytesImage>();
            // PortableImage.As<T> calls PortableImage.ToBytes() which converts to 4 component.
            // with the fourth component being a fully opaque Alpha channe
            // https://github.com/cureos/csj2k/blob/7666c06b90ff9d9425c1290390a670b04eb9085e/CSJ2K/Util/PortableImage.cs#L81

            result.Width = raw.Width;
            result.Height = raw.Height;
            result.Components = pi.NumberOfComponents;

            switch (pi.NumberOfComponents)
            {
                //case 1:
                    // TODO convert to 3 component greyscale?
                case 3:
                    result.Data = ColorConverter.BrgaToRgb(raw.Data);
                    break;
                case 4:
                    result.Data = ColorConverter.AbgrToRgba(raw.Data);
                    break;
                default:
                    _log.LogDebug($"Invalid number of components in texture: {pi.NumberOfComponents}");
                    throw new TextureDecodeException($"Invalid number of components in texture: {pi.NumberOfComponents}");
            }

            return Task.FromResult(result);
        }
    }
}
