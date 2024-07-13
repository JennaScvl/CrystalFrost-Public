﻿using OpenMetaverse;
using OpenMetaverse.Rendering;
using System;
using System.Collections.Generic;

namespace CrystalFrost.Lib
{
    public interface ITransformTexCoords
    {
        void TransformTexCoords(List<Vertex> vertices, Vector3 center, Primitive.TextureEntryFace teFace, Vector3 primScale);
    }

    public class TransformTexCoordsForUnity :ITransformTexCoords
    {

        /// <summary>
        /// Based on OpenMetaverse.Rendering.MeshmerizeR.TransformTexCoords.
        /// Modified to rotate texture by 1/2pi (90 degrees) because Unity has a different texture rotation system.
        /// </summary>
        public void TransformTexCoords(List<Vertex> vertices, Vector3 center, Primitive.TextureEntryFace teFace, Vector3 primScale)
        {
            // compute trig stuff up front

            // Modified:
            float cosineAngle = (float)Math.Sin(teFace.Rotation + 1.570796316f);
            float sinAngle = (float)Math.Cos(teFace.Rotation + 1.570796316f);
            
            // Unmodified:
            //float cosineAngle = (float)Math.Sin(teFace.Rotation);
            //float sinAngle = (float)Math.Cos(teFace.Rotation);

            for (int ii = 0; ii < vertices.Count; ii++)
            {
                // tex coord comes to us as a number between zero and one
                // transform about the center of the texture
                Vertex vert = vertices[ii];

                // aply planar tranforms to the UV first if applicable
                if (teFace.TexMapType == MappingType.Planar)
                {
                    Vector3 binormal;
                    float d = Vector3.Dot(vert.Normal, Vector3.UnitX);
                    if (d >= 0.5f || d <= -0.5f)
                    {
                        binormal = Vector3.UnitY;
                        if (vert.Normal.X < 0f) binormal *= -1;
                    }
                    else
                    {
                        binormal = Vector3.UnitX;
                        if (vert.Normal.Y > 0f) binormal *= -1;
                    }
                    Vector3 tangent = binormal % vert.Normal;
                    Vector3 scaledPos = vert.Position * primScale;
                    vert.TexCoord.X = 1f + (Vector3.Dot(binormal, scaledPos) * 2f - 0.5f);
                    vert.TexCoord.Y = -(Vector3.Dot(tangent, scaledPos) * 2f - 0.5f);
                }

                float repeatU = teFace.RepeatU;
                float repeatV = teFace.RepeatV;
                float tX = vert.TexCoord.X - 0.5f;
                float tY = vert.TexCoord.Y - 0.5f;

                vert.TexCoord.X = (tX * cosineAngle + tY * sinAngle) * repeatU + teFace.OffsetU + 0.5f;
                vert.TexCoord.Y = (-tX * sinAngle + tY * cosineAngle) * repeatV + teFace.OffsetV + 0.5f;
                vertices[ii] = vert;
            }
        }
    }
}