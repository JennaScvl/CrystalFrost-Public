using System.Runtime.InteropServices;

namespace CrystalFrost.Assets.Textures
{
    public static class ColorConverter
    {
        [StructLayout(LayoutKind.Sequential, Size = 4)]
        private struct BRGAColor
        {
            public byte B;
            public byte R;
            public byte G;
            public byte A;
        }

        [StructLayout(LayoutKind.Sequential, Size = 3)]
        private struct RGBColor
        {
            public byte R;
            public byte G;
            public byte B;
        }

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        private struct RGBAColor
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        }

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        private struct ABGRColor
        {
            public byte A;
            public byte B;
            public byte G;
            public byte R;
        }

        /// <summary>
        /// Converts a BRGA byte array to RGB byte arry
        /// </summary>
        /// <param name="brgaBytes"></param>
        /// <returns></returns>
        public static byte[] BrgaToRgb(byte[] brgaBytes)
        {
            var srcSpan = MemoryMarshal.Cast<byte, BRGAColor>(brgaBytes);
            var result = new byte[srcSpan.Length * 3];
            var destSpan = MemoryMarshal.Cast<byte, RGBColor>(result);
            for (var i = 0; i < srcSpan.Length; i++)
            {
                destSpan[i].R = srcSpan[i].R;
                destSpan[i].G = srcSpan[i].G;
                destSpan[i].B = srcSpan[i].B;
            }
            return result;
        }

        /// <summary>
        /// Converts an ABGR byte array to RGBA byte array
        /// </summary>
        /// <param name="abgrBytes"></param>
        /// <returns></returns>
        public static byte[] AbgrToRgba(byte[] abgrBytes)
        {
            var srcSpan = MemoryMarshal.Cast<byte, ABGRColor>(abgrBytes);
            var result = new byte[srcSpan.Length * 4];
            var destSpan = MemoryMarshal.Cast<byte, RGBAColor>(result);
            for (var i = 0; i < srcSpan.Length; i++)
            {
                destSpan[i].R = srcSpan[i].R;
                destSpan[i].G = srcSpan[i].G;
                destSpan[i].B = srcSpan[i].B;
                destSpan[i].A = srcSpan[i].A;
            }
            return result;
        }
        
    }
}
