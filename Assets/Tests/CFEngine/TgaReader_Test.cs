using System.IO;
using CrystalFrost.Lib;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace CrystalFrostEngine.Tests
{
    public class TgaReader_Test
    {
        [Test]
        public void TgaReader_ReadsBottomLeftOriginCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TgaReader>>();
            var tgaReader = new TgaReader(mockLogger.Object);
            var tgaData = File.ReadAllBytes("Assets/Tests/CFEngine/test.tga");

            // Act
            tgaReader.Read(tgaData);

            // Assert
            Assert.AreEqual(2, tgaReader.Width);
            Assert.AreEqual(2, tgaReader.Height);
            Assert.AreEqual(24, tgaReader.BitsPerPixel);

            // The image is:
            // Top-left: Blue (0, 0, 255)
            // Top-right: White (255, 255, 255)
            // Bottom-left: Red (255, 0, 0)
            // Bottom-right: Green (0, 255, 0)

            // The raw data is stored with a bottom-left origin.
            // Our reader should flip it to be top-left.

            // The test TGA is stored bottom-up, so the first pixels in the file are the bottom row
            // The corrected (flipped) buffer should have the top row first.

            var bitmap = tgaReader.Bitmap;

            // Top-left pixel (Blue)
            Assert.AreEqual(0, bitmap[0]);
            Assert.AreEqual(0, bitmap[1]);
            Assert.AreEqual(255, bitmap[2]);

            // Top-right pixel (White)
            Assert.AreEqual(255, bitmap[3]);
            Assert.AreEqual(255, bitmap[4]);
            Assert.AreEqual(255, bitmap[5]);

            // Bottom-left pixel (Red)
            Assert.AreEqual(255, bitmap[6]);
            Assert.AreEqual(0, bitmap[7]);
            Assert.AreEqual(0, bitmap[8]);

            // Bottom-right pixel (Green)
            Assert.AreEqual(0, bitmap[9]);
            Assert.AreEqual(255, bitmap[10]);
            Assert.AreEqual(0, bitmap[11]);
        }
    }
}
