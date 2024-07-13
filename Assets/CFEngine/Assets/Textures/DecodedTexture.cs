using OpenMetaverse;

namespace CrystalFrost.Assets.Textures
{
    public class DecodedTexture
    {
        public UUID UUID { get; set; }
        public byte[] Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Bytes per pixel
        /// </summary>
        public int Components { get; set; }
    }
}
