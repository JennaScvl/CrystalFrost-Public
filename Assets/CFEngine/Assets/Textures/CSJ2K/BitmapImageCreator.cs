// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

using CSJ2K.j2k.image;
using CSJ2K.Util;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
    /// <summary>
    /// A factory for creating <see cref="BitmapImage"/> instances.
    /// </summary>
    public class BitmapImageCreator : IImageCreator
    {
        private static readonly IImageCreator Instance = new BitmapImageCreator();

        public bool IsDefault
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Registers this image creator with the image factory.
        /// </summary>
        public static void Register()
        {
            ImageFactory.Register(Instance);
        }

        /// <summary>
        /// Creates a new <see cref="BitmapImage"/> instance.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytes">The raw byte data of the image.</param>
        /// <returns>A new <see cref="BitmapImage"/> instance.</returns>
        public IImage Create(int width, int height, byte[] bytes)
        {
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR_OSX
            return new BitmapImage(width, height, bytes);
#else
			throw new System.NotImplementedException();	
#endif
        }

        /// <summary>
        /// Converts an image object to a portable image source.
        /// </summary>
        /// <param name="imageObject">The image object to convert.</param>
        /// <returns>A new <see cref="BitmapImageSource"/> instance.</returns>
        public BlkImgDataSrc ToPortableImageSource(object imageObject)
        {
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR_OSX
            return BitmapImageSource.Create(imageObject);
#else
			throw new System.NotImplementedException();
#endif
        }
    }
}
