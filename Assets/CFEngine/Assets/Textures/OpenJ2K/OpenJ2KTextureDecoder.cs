using CrystalFrost.Lib;
using CrystalFrost.Timing;
using Microsoft.Extensions.Logging;
using OpenMetaverse.Assets;
using System;
using System.Threading.Tasks;

namespace CrystalFrost.Assets.Textures.OpenJ2K
{
    public class OpenJ2KTextureDecoder : ITextureDecoder
    {
        private readonly ILogger<OpenJ2KTextureDecoder> _log;
        private readonly ITgaReader _tgaReader;

        public OpenJ2KTextureDecoder(
            ILogger<OpenJ2KTextureDecoder> log,
            ITgaReader tgaReader)
        {
            _log = log;
            _tgaReader = tgaReader;
        }

        public Task<DecodedTexture> Decode(AssetTexture texture)
        {
            return Perf.Measure("OpenJ2KTextureDecoder.Decode",
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
                // 'using' keyword cause dispose automatically when the variable goes out of scope.
                using var reader = new OpenJpegDotNet.IO.Reader(texture.AssetData);
                if (!reader.ReadHeader())
                {
                    _log.LogWarning("Failed to read header for texture " + texture.AssetID);
                }

                // gee. it sure would be nice to not
                // convert twice. Could we go straight from Jpeg2000 to bitmap?
                using var image = reader.Decode();
                if (image.NumberOfComponents != 3 && image.NumberOfComponents != 4)
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
                using var raw = image.ToTarga();
                _tgaReader.Read(raw.Bytes);

                if (_tgaReader.Bitmap == null)
                {
                    // throwing is bad, we should change this to not throw somehow,
                    // and instead communicate failure to the caller.
                    _log.LogError("Could not convert TGA Data.");
                    throw new TextureDecodeException("Could not convert TGA Data.");
                }

                return new DecodedTexture()
                {
                    UUID = texture.AssetID,
                    Data = _tgaReader.Bitmap,
                    Width = raw.Width,
                    Height = raw.Height,
                    Components = _tgaReader.BitsPerPixel / 8
                };
            }
            catch (Exception ex)
            {
                _log.LogError("Texture decode error. " + ex.Message);
                throw new TextureDecodeException("There was a problem decoding a texture.", ex);
            }
        }
    }
}
