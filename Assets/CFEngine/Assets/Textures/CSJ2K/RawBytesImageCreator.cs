using CSJ2K.j2k.image;
using CSJ2K.Util;
using System;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
    /// <summary>
    /// Represents an image as a raw byte array.
    /// </summary>
    public class RawBytesImage : IImage
    {
        /// <summary>
        /// Gets the raw byte data of the image.
        /// </summary>
        public byte[] Data { get; }
        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height { get; }
        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width { get; }    

        /// <summary>
        /// Initializes a new instance of the <see cref="RawBytesImage"/> class.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytes">The raw byte data of the image.</param>
        public RawBytesImage(int width, int height, byte[] bytes)
        {
            Data = bytes;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Casts the image to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <returns>The casted image.</returns>
        public T As<T>()
        {
            if (typeof(T) == typeof(RawBytesImage))
            {
                return (T)(object)this;
            }
            throw new TextureDecodeException($"Cannot cast RawBytesImage to {typeof(T)}");
        }
    }

    internal class RawBytesImageCreator : IImageCreator
    {
        private static readonly IImageCreator Instance = new RawBytesImageCreator();

        public bool IsDefault => false;

        public static void Register()
        {
            ImageFactory.Register(Instance);
        }

        public IImage Create(int width, int height, byte[] bytes)
        {
            return new RawBytesImage(width, height, bytes);
        }

        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
            throw new NotImplementedException();
        }
    }
}
