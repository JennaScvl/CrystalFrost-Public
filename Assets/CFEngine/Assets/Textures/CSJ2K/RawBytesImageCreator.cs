using CSJ2K.j2k.image;
using CSJ2K.Util;
using System;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
    public class RawBytesImage : IImage
    {
        public byte[] Data { get; }
        public int Height { get; }
        public int Width { get; }    

        public RawBytesImage(int width, int height, byte[] bytes)
        {
            Data = bytes;
            Width = width;
            Height = height;
        }

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
