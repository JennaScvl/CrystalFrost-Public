using System.Collections.Generic;
using CrystalFrost.Lib;
using NUnit.Framework;
using OpenMetaverse;
using OpenMetaverse.Rendering;

namespace CrystalFrostEngine.Tests
{
    public class TransformTexCoordsForUnity_Test
    {
        [Test]
        public void TransformTexCoords_Rotates90DegreesCorrectly()
        {
            // Arrange
            var transformer = new TransformTexCoordsForUnity();
            var vertices = new List<Vertex>
            {
                new Vertex
                {
                    TexCoord = new Vector2(1, 1)
                }
            };
            var teFace = new Primitive.TextureEntryFace(null)
            {
                Rotation = 0, // The code adds 90 degrees, so this will be a 90 degree rotation
                RepeatU = 1,
                RepeatV = 1,
                OffsetU = 0,
                OffsetV = 0
            };

            // Act
            transformer.TransformTexCoords(vertices, Vector3.Zero, teFace, Vector3.One);

            // Assert
            // A 90 degree rotation should transform (1,1) to (0,1)
            // after the translation and scaling logic.
            Assert.AreEqual(0, vertices[0].TexCoord.X, 0.0001);
            Assert.AreEqual(1, vertices[0].TexCoord.Y, 0.0001);
        }
    }
}
