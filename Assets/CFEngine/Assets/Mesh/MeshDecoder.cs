using OpenMetaverse.Rendering;
using OpenMetaverse;
using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using CrystalFrost.Extensions;

namespace CrystalFrost.Assets.Mesh
{
    public interface IMeshDecoder
    {
        void Decode(MeshRequest request);
    }

    public class MeshDecoder : IMeshDecoder
    {
        private readonly IDecodedMeshQueue _readyMeshQueue;

        public MeshDecoder(IDecodedMeshQueue readyMeshQueue)
        {
            _readyMeshQueue = readyMeshQueue;
        }

        public void Decode(MeshRequest request)
        {
			/*
#if RenderHighestDetail
            TranscodeFacetedMeshAtDetailLevel(request, DetailLevel.Highest);
#endif
#if !RenderHighDetail
            TranscodeFacetedMeshAtDetailLevel(request, DetailLevel.High);
#endif
#if !RenderMediumDetail
            TranscodeFacetedMeshAtDetailLevel(request, DetailLevel.Medium);
#endif
			*/
			TranscodeFacetedMeshAtDetailLevel(request, DetailLevel.Highest);
		}

		private void TranscodeFacetedMeshAtDetailLevel(MeshRequest request, DetailLevel detailLevel)
        {
            var prim = request.Primitive;
            var assetMesh = request.AssetMesh;

			if (!RiggedMesh.TryDecodeFromAsset(prim, assetMesh, detailLevel, out RiggedMesh fmesh))
			{
				Debug.LogWarning($"Unable to decode {detailLevel} detail mesh UUID: {request.UUID}");
				return;
			}

			//if (!FacetedMesh.TryDecodeFromAsset(prim, assetMesh, detailLevel, out FacetedMesh fmesh))
            //{
            //    Debug.LogWarning($"Unable to decode {detailLevel} detail mesh");
            //    return;
            //
			
            request.DecodedMesh = new();
			request.DecodedMesh.assetId = request.UUID;
			request.DecodedMesh.joints = fmesh.Joints;
			request.DecodedMesh.bindShapeMatrix = fmesh.BindShapeMatrix.ToMatrix4x4();
			request.DecodedMesh.pelvisOffsetMatrix = fmesh.PelvisOffsetMatrix.ToMatrix4x4();
			request.DecodedMesh.isSkinned = fmesh.Joints != null && fmesh.Joints.Length > 0;

            for (var j = 0; j < fmesh.Faces.Count; j++)
            {
                Primitive.TextureEntryFace textureEntryFace = prim.Textures.GetFace((uint)j);
				RawMeshData rmd = null;

				// rmd = fmesh.Faces[j].ToRawMeshData()
				if (fmesh.IsSkinned)
				{
					rmd = fmesh.Faces[j].ToRawMeshData(fmesh.JointInfluences[j], fmesh.Joints);
				}
				else
				{
					rmd = fmesh.Faces[j].ToRawMeshData();
				}


				float cosineAngle = (float)Math.Cos(textureEntryFace.Rotation * Mathf.Deg2Rad);
                float sinAngle = (float)Math.Sin(textureEntryFace.Rotation * Mathf.Deg2Rad);

                for (var i = 0; i < rmd.uvs.Length; i++)
                {
                    float repeatU = textureEntryFace.RepeatU;
                    float repeatV = textureEntryFace.RepeatV;
                    float tX = rmd.uvs[i].x - 0.5f;
                    float tY = rmd.uvs[i].y - 0.5f;

                    if (textureEntryFace.TexMapType == MappingType.Planar)
                    {
                        Vector3 binormal;
                        float d = Vector3.Dot(rmd.normals[i], Vector3.right);
                        if (d >= 0.5f || d <= -0.5f)
                        {
                            binormal = Vector3.forward;
                            if (rmd.normals[i].x < 0f) binormal *= -1;
                        }
                        else
                        {
                            binormal = Vector3.right;
                            if (rmd.normals[i].z > 0f) binormal *= -1;
                        }
                        Vector3 tangent = Vector3.Cross(binormal, rmd.normals[i]);//binormal % rmd.normals[i];
                        var primScale = prim.Scale.ToUnity();
                        var scaledPos = Vector3.Scale(rmd.vertices[i], primScale);

                        rmd.uvs[i].x = 1f + (Vector3.Dot(binormal, scaledPos) * 2f - 0.5f);
                        rmd.uvs[i].y = -(Vector3.Dot(tangent, scaledPos) * 2f - 0.5f);
                    }

                    rmd.uvs[i].x = (tX * cosineAngle + tY * sinAngle) * repeatU + textureEntryFace.OffsetU + 0.5f;
                    rmd.uvs[i].y = (-tX * sinAngle + tY * cosineAngle) * repeatV + (1f - textureEntryFace.OffsetV) + 0.5f;
                }
                request.DecodedMesh.meshData.Add(rmd);
            }
            _readyMeshQueue.Enqueue(request);
        }
    }
}
