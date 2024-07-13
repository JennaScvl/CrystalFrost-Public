using Microsoft.Extensions.Logging;
using System.IO;

namespace CrystalFrost.Lib
{
    public interface ITgaReader
    {
        /// <summary>
        /// Reads TGA Data. Properties are set when reading.
        /// </summary>
        /// <param name="b">a byte array containing TGA data.</param>
        void Read(byte[] tgaData);

        /// <summary>
        /// The width of the read image in pixels.
        /// </summary>
        int Width { get; }
        
        /// <summary>
        /// the height of the read image in pixels.
        /// </summary>
        int Height { get; }
        
        /// <summary>
        /// The bits per pixel of the image read.
        /// 32 (4 bytes per pixel) or
        /// 23 (3 bytes per pixel)
        /// </summary>
        int BitsPerPixel { get; }

        /// <summary>
        /// The image converted to bitmap format.
        /// Either 32bpp RGBA, or 24bpp RGB.
        /// Will be null if the code could not convert.
        /// </summary>
        byte[] Bitmap { get; }
    }

    /// <inheritdoc/>
    public class TgaReader : ITgaReader
    {
        private readonly ILogger<TgaReader> _log;

        // based on code from aaro4130 on the Unity forums

        public int Width { get; private set; } = 0;

        public int Height { get; private set; } = 0;
        
        public int BitsPerPixel { get; private set; } = 0;

        public byte[] Bitmap { get; private set; } = null;

        public TgaReader(ILogger<TgaReader> log)
        {
            _log = log;
        }

        public void Read(byte[] tgaData)
        {
            using var m = new MemoryStream(tgaData);
            using var r = new BinaryReader(m);

            // Skip some header info we don't care about.
            // Even if we did care, we have to move the stream seek point to the beginning,
            // as the previous method in the workflow left it at the end.
            r.BaseStream.Seek(12, SeekOrigin.Begin);

            Width = r.ReadInt16();
            Height = r.ReadInt16();
            BitsPerPixel = r.ReadByte();

            // Skip a byte of header information we don't care about.
            r.BaseStream.Seek(1, SeekOrigin.Current);

            var pixels = Width * Height;

            if (BitsPerPixel == 32)
            {
                Bitmap = new byte[pixels * 4];
                for (int i = 0; i < pixels; i++)
                {
                    // convert BGRA to RGBA
                    Bitmap[(i * 4) + 2] = r.ReadByte();
                    Bitmap[(i * 4) + 1] = r.ReadByte();
                    Bitmap[(i * 4)    ] = r.ReadByte();
                    Bitmap[(i * 4) + 3] = r.ReadByte();
                }
            }
            else if (BitsPerPixel == 24)
            {
                Bitmap = new byte[pixels * 3];
                for (int i = 0; i < pixels; i++)
                {
                    // convert  BGR to RGB
                    Bitmap[(i * 3) + 2] = r.ReadByte();
                    Bitmap[(i * 3) + 1] = r.ReadByte();
                    Bitmap[(i * 3)] = r.ReadByte();
                }
            }
            else
            {
                Bitmap = null;
                _log.UnsupportedBitDepth(BitsPerPixel);
            }
        }
    }
}