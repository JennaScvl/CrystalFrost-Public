#if !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR_OSX
// Copyright (c) 2007-2016 CSJ2K contributors.
// Licensed under the BSD 3-Clause License.
using CSJ2K.Util;
using System.Drawing;
using System.Drawing.Imaging;

namespace CrystalFrost.Assets.Textures.CSJ2K
{
    /// <summary>
    /// Represents an image as a <see cref="System.Drawing.Bitmap"/>.
    /// </summary>
    internal class BitmapImage : ImageBase<Image>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImage"/> class.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytes">The raw byte data of the image.</param>
        internal BitmapImage(int width, int height, byte[] bytes)
            : base(width, height, bytes)
        {
        }

        /// <summary>
        /// Gets the image object as a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <returns>The image object.</returns>
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