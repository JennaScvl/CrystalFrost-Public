// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.

using CSJ2K.j2k.image;
using CSJ2K.Util;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
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

        public static void Register()
        {
            ImageFactory.Register(Instance);
        }

        public IImage Create(int width, int height, byte[] bytes)
        {
#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR_OSX
            return new BitmapImage(width, height, bytes);
#else
			throw new System.NotImplementedException();	
#endif
        }

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
