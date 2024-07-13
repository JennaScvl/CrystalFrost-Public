#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR_OSX
// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.
using CSJ2K.Util;
using System.Drawing;
using System.Drawing.Imaging;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
    internal class BitmapImage : ImageBase<Image>
    {
        internal BitmapImage(int width, int height, byte[] bytes)
            : base(width, height, bytes)
        {
        }

        protected override object GetImageObject()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            var dstdata = bitmap.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat);

            var ptr = dstdata.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(Bytes, 0, ptr, Bytes.Length);
            bitmap.UnlockBits(dstdata);

            return bitmap;
        }
    }
}
#endif