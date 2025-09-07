using CrystalFrost.Extensions;
using OpenMetaverse.Rendering;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
	/// <summary>
	/// Provides extension methods for working with mesh data.
	/// </summary>
	public static class MeshExtensions
	{
		/// <summary>
		/// Reverses the winding order of the triangles in a mesh.
		/// </summary>
		/// <param name="mesh">The mesh to modify.</param>
		/// <returns>The modified mesh.</returns>
		public static UnityEngine.Mesh ReverseWind(this UnityEngine.Mesh mesh)
		{
			var indices = mesh.triangles;
			var triangleCount = indices.Length / 3;
			for (var i = 0; i < triangleCount; i++)
			{
#pragma warning disable IDE0180 // using tuples to swap values is not more readable or more efficient
				var tmp = indices[i * 3];
#pragma warning restore IDE0180
				indices[i * 3] = indices[i * 3 + 1];
				indices[i * 3 + 1] = tmp;
			}
			mesh.triangles = indices;
			return mesh;
		}

		public static UnityEngine.Mesh FlipNormals(this UnityEngine.Mesh mesh)
		{
			var normals = mesh.normals;
			for (var n = 0; n < normals.Length; n++)
			{
				normals[n] = -normals[n];
			}
			mesh.normals = normals;

			return mesh;
		}

		/// <summary>
		/// Converts a OpenMetaverse.Reandering.Face into RawMeshData.
		/// </summary>
		public static RawMeshData ToRawMeshData(this Face face)
		{
			var indices = face.Indices.ToArray();
			var count = face.Vertices.Count;
			var vertices = new Vector3[count];
			var normals = new Vector3[count];
			var uvs = new Vector2[count];

			// could Parallel.For speed this up?
			for (var i = 0; i < count; i++)
			{
				var vert = face.Vertices[i];
				vertices[i] = vert.Position.ToUnity();
				normals[i] = vert.Normal.ToUnity();
				uvs[i] = vert.TexCoord.ToUnity();
			}

			return new RawMeshData(vertices, uvs, normals, indices);
		}

		public static RawMeshData ToRawMeshData(this Face face, List<JointInfluence[]> jointInfluences, JointInfo[] joints)
		{
			var rmd = face.ToRawMeshData();

			if (jointInfluences == null) return rmd;

			var count = face.Vertices.Count;
			var weights = new BoneWeight[count];

			for (var i = 0; i < count; i++)
			{
				var influences = jointInfluences[i];
				var weight = weights[i];
				weight.weight0 = weight.weight1 = weight.weight2 = weight.weight3 = 0.0f;

				for (int j = 0, k = 0; j < influences.Length; j++)				{
					var influence = influences[j];					var jointIndex = influence.JointIndex;					var weightValue = influence.WeightValue;

					//if (!isRegularBone(jointIndex)) continue;

					if (k == 0)					{						weight.boneIndex0 = jointIndex;						weight.weight0 = weightValue;					}					else if (k == 1)					{						weight.boneIndex1 = jointIndex;						weight.weight1 = weightValue;					}					else if (k == 2)					{						weight.boneIndex2 = jointIndex;						weight.weight2 = weightValue;					}					else if (k == 3)					{						weight.boneIndex3 = jointIndex;						weight.weight3 = weightValue;					}					k++;
				}

				float sum = weight.weight0 + weight.weight1 + weight.weight2 + weight.weight3;
				if (sum > 0.0f)
				{
					weight.weight0 /= sum;
					weight.weight1 /= sum;
					weight.weight2 /= sum;
					weight.weight3 /= sum;
				}

				weights[i] = weight;				
			}

			rmd.boneWeights = weights;
			rmd.isSkinned = true;

			return rmd;
		}

		/// <summary>
		/// Converts a OpenMetaverse.Reandering.Face into RawMeshData.
		/// </summary>
		[Obsolete("Decode FacetedMesh.Face[face] directly.")]
		public static RawMeshData ToRawMeshData(FacetedMesh fmesh, int face)
		{
			return ToRawMeshData(fmesh.Faces[face]);
		}

		public static UnityEngine.Mesh[] ToUnityMeshArray(this List<RawMeshData> meshData)
		{
			// I wonder if linq is comparable in speed?
			// return meshData.Select(rmd => rmd.ToUnityMesh()).ToArray();
			var result = new UnityEngine.Mesh[meshData.Count];
			for (var i = 0; i < result.Length; i++)
			{
				result[i] = meshData[i].ToUnityMesh();
			}
			return result;
		}

		public static UnityEngine.Mesh ToUnityMesh(this RawMeshData rmd)
		{
			var result = new UnityEngine.Mesh
			{
				vertices = rmd.vertices,
				uv = rmd.uvs,
				normals = rmd.normals,
			};

			if (rmd.isSkinned)
			{
				result.boneWeights = rmd.boneWeights;
				// result.bindposes = rmd.bindPoses;
			}

			result.SetIndices(rmd.indices, MeshTopology.Triangles, 0);
			result = result.ReverseWind();
			result.RecalculateBounds();
			return result;
		}
	}
}
