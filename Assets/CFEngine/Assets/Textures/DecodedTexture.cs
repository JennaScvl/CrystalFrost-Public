using OpenMetaverse;

namespace CrystalFrost.Assets.Textures
{
    /// <summary>
    /// Represents a decoded texture, including its pixel data and metadata.
    /// </summary>
    public class DecodedTexture
    {
        /// <summary>
        /// Gets or sets the UUID of the texture.
        /// </summary>
        public UUID UUID { get; set; }
        /// <summary>
        /// Gets or sets the raw pixel data of the texture.
        /// </summary>
        public byte[] Data { get; set; }
        /// <summary>
        /// Gets or sets the width of the texture.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Gets or sets the height of the texture.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Bytes per pixel
        /// </summary>
        public int Components { get; set; }
    }
}
