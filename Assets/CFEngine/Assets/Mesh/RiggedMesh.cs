using System;
using System.Collections.Generic;
using CrystalFrost.Extensions;
using OpenMetaverse.Assets;
using OpenMetaverse.StructuredData;
using UnityEngine;

// The common elements shared between rendering plugins are defined here

namespace OpenMetaverse.Rendering
{
	#region Mesh Classes

	public class JointInfo
	{
		public string Name;
		public float[] InverseBindMatrix;
		public float[] AltInverseBindMatrix;
		// public int Parent;

		public Matrix4x4 InverseBindMatrixUnity
		{
			get
			{
				// return MeshUtils.FloatArrayToMatrix(InverseBindMatrix);
				return InverseBindMatrix.ToMatrix4x4();
			}
		}

		public Matrix4x4 AltInverseBindMatrixUnity
		{
			get
			{
				// return MeshUtils.FloatArrayToMatrix(AltInverseBindMatrix);
				return AltInverseBindMatrix.ToMatrix4x4();
			}
		}

		public JointInfo()
		{
		}
	}

	public struct JointInfluence
	{
		public byte JointIndex { get; }
		public float WeightValue { get; }

		public JointInfluence(byte jointIndex, float weightValue)
		{
			JointIndex = jointIndex;
			WeightValue = weightValue;
		}
	}

	/// <summary>
	/// Contains all mesh faces that belong to a prim
	/// </summary>
	public class RiggedMesh : Mesh
	{
		/// <summary>List of primitive faces</summary>

		public List<List<JointInfluence[]>> JointInfluences = null;
		public List<Face> Faces = null;
		public float[] BindShapeMatrix = null;
		public float[] PelvisOffsetMatrix = null;
		public JointInfo[] Joints = null;
		public bool IsSkinned = false;

		public static bool IsRiggedMesh(Primitive prim, AssetMesh meshAsset, DetailLevel LOD)
		{
			try
			{
				if (!meshAsset.Decode())
				{
					return false;
				}

				OSDMap MeshData = meshAsset.MeshData;


				if (MeshData.ContainsKey("skin"))
				{
					return true;
				}


			}
			catch (Exception ex)
			{
				Logger.Log("Failed to decode mesh asset: " + ex.Message, Helpers.LogLevel.Warning);
				return false;
			}

			return false;
		}

		static List<JointInfluence[]> ParseEntries(byte[] inputData)
		{
			List<JointInfluence[]> entries = new();

			for (int i = 0; i < inputData.Length;)
			{
				List<JointInfluence> currentEntry = new();
				
				while (currentEntry.Count < 4)
				{
					var jointIndex = inputData[i++];
					if (jointIndex == 0xFF)
					{
						break;
					}
					// if (jointIndex > 90) UnityEngine.Debug.LogError("Above 90 !? : " + jointIndex);
					var byte0 = inputData[i++];
					var byte1 = inputData[i++];
					var weightValue = Utils.UInt16ToFloat((ushort)((byte1 << 8) | byte0), 0.0f, 1.0f);
					currentEntry.Add(new JointInfluence(jointIndex, weightValue));
				}
				entries.Add(currentEntry.ToArray());
			}

			return entries;
		}

		private static bool TryDecodeMatrixFromOSD(OSD osd, out float[] mat)
		{
			mat = null;
			if (osd is OSDArray poOsd)
			{
				float[] poFloats = new float[16];
				for (int i = 0; i < 16; i++)
				{
					poFloats[i] = (float)poOsd[i].AsReal();
				}
				mat = poFloats;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Decodes mesh asset into RiggedMesh
		/// </summary>
		/// <param name="prim">Mesh primitive</param>
		/// <param name="meshAsset">Asset retrieved from the asset server</param>
		/// <param name="LOD">Level of detail</param>
		/// <param name="mesh">Resulting decoded RiggedMesh</param>
		/// <returns>True if mesh asset decoding was successful</returns>
		public static bool TryDecodeFromAsset(Primitive prim, AssetMesh meshAsset, DetailLevel LOD, out RiggedMesh mesh)
		{
			mesh = null;

			try
			{
				if (!meshAsset.Decode())
				{
					return false;
				}

				OSDMap MeshData = meshAsset.MeshData;

				mesh = new RiggedMesh
				{
					Faces = new List<Face>(),
					Prim = prim,
					Profile =
					{
						Faces = new List<ProfileFace>(),
						Positions = new List<Vector3>()
					},
					Path = { Points = new List<PathPoint>() }
				};

				OSD facesOSD = null;

				switch (LOD)
				{
					default:
					case DetailLevel.Highest:
						facesOSD = MeshData["high_lod"];
						break;

					case DetailLevel.High:
						facesOSD = MeshData["medium_lod"];
						break;

					case DetailLevel.Medium:
						facesOSD = MeshData["low_lod"];
						break;

					case DetailLevel.Low:
						facesOSD = MeshData["lowest_lod"];
						break;
				}

				if (!(facesOSD is OSDArray decodedMeshOsdArray))
				{
					return false;
				}
				mesh.JointInfluences = new();
				for (int faceNr = 0; faceNr < decodedMeshOsdArray.Count; faceNr++)
				{
					OSD subMeshOsd = decodedMeshOsdArray[faceNr];

					// Decode each individual face
					if (subMeshOsd is OSDMap subMeshMap)
					{
						// As per http://wiki.secondlife.com/wiki/Mesh/Mesh_Asset_Format, some Mesh Level
						// of Detail Blocks (maps) contain just a NoGeometry key to signal there is no
						// geometry for this submesh.
						if (subMeshMap.ContainsKey("NoGeometry") && ((OSDBoolean)subMeshMap["NoGeometry"]))
							continue;

						Face oface = new Face
						{
							ID = faceNr,
							Vertices = new List<Vertex>(),
							Indices = new List<ushort>(),
							TextureFace = prim.Textures.GetFace((uint)faceNr)
						};

						Vector3 posMax;
						Vector3 posMin;

						// If PositionDomain is not specified, the default is from -0.5 to 0.5
						if (subMeshMap.ContainsKey("PositionDomain"))
						{
							posMax = ((OSDMap)subMeshMap["PositionDomain"])["Max"];
							posMin = ((OSDMap)subMeshMap["PositionDomain"])["Min"];
						}
						else
						{
							posMax = new Vector3(0.5f, 0.5f, 0.5f);
							posMin = new Vector3(-0.5f, -0.5f, -0.5f);
						}

						// Vertex positions
						byte[] posBytes = subMeshMap["Position"];

						// Normals
						byte[] norBytes = null;
						if (subMeshMap.ContainsKey("Normal"))
						{
							norBytes = subMeshMap["Normal"];
						}

						// UV texture map
						Vector2 texPosMax = Vector2.Zero;
						Vector2 texPosMin = Vector2.Zero;
						byte[] texBytes = null;
						if (subMeshMap.ContainsKey("TexCoord0"))
						{
							texBytes = subMeshMap["TexCoord0"];
							texPosMax = ((OSDMap)subMeshMap["TexCoord0Domain"])["Max"];
							texPosMin = ((OSDMap)subMeshMap["TexCoord0Domain"])["Min"];
						}

						// Extract the vertex position data
						// If present normals and texture coordinates too
						for (int i = 0; i < posBytes.Length; i += 6)
						{
							ushort uX = Utils.BytesToUInt16(posBytes, i);
							ushort uY = Utils.BytesToUInt16(posBytes, i + 2);
							ushort uZ = Utils.BytesToUInt16(posBytes, i + 4);

							Vertex vx = new Vertex
							{
								Position = new Vector3(
									Utils.UInt16ToFloat(uX, posMin.X, posMax.X),
									Utils.UInt16ToFloat(uY, posMin.Y, posMax.Y),
									Utils.UInt16ToFloat(uZ, posMin.Z, posMax.Z))
							};

							if (norBytes != null && norBytes.Length >= i + 4)
							{
								ushort nX = Utils.BytesToUInt16(norBytes, i);
								ushort nY = Utils.BytesToUInt16(norBytes, i + 2);
								ushort nZ = Utils.BytesToUInt16(norBytes, i + 4);

								vx.Normal = new Vector3(
									Utils.UInt16ToFloat(nX, posMin.X, posMax.X),
									Utils.UInt16ToFloat(nY, posMin.Y, posMax.Y),
									Utils.UInt16ToFloat(nZ, posMin.Z, posMax.Z));
							}

							var vertexIndexOffset = oface.Vertices.Count * 4;

							if (texBytes != null && texBytes.Length >= vertexIndexOffset + 4)
							{
								ushort tX = Utils.BytesToUInt16(texBytes, vertexIndexOffset);
								ushort tY = Utils.BytesToUInt16(texBytes, vertexIndexOffset + 2);

								vx.TexCoord = new Vector2(
									Utils.UInt16ToFloat(tX, texPosMin.X, texPosMax.X),
									Utils.UInt16ToFloat(tY, texPosMin.Y, texPosMax.Y));
							}

							oface.Vertices.Add(vx);
						}

						byte[] triangleBytes = subMeshMap["TriangleList"];
						for (int i = 0; i < triangleBytes.Length; i += 6)
						{
							ushort v1 = (ushort)(Utils.BytesToUInt16(triangleBytes, i));
							oface.Indices.Add(v1);
							ushort v2 = (ushort)(Utils.BytesToUInt16(triangleBytes, i + 2));
							oface.Indices.Add(v2);
							ushort v3 = (ushort)(Utils.BytesToUInt16(triangleBytes, i + 4));
							oface.Indices.Add(v3);
						}

						// get Weights correspond to each face
						if (subMeshMap.ContainsKey("Weights"))
						{
							var wts = ParseEntries(subMeshMap["Weights"]);
							if (wts == null || wts.Count != oface.Vertices.Count)
							{
								Logger.Log("Weights count does not match vertices count", Helpers.LogLevel.Warning);
								throw new Exception("Weights count does not match vertices count");
							}

							mesh.JointInfluences.Add(wts);
						}
						mesh.Faces.Add(oface);
					}
				}


				// Decode the skin information
				if (MeshData.ContainsKey("skin") && MeshData["skin"] is OSDMap skinMap)
				{
					mesh.IsSkinned = true;

					if (!TryDecodeMatrixFromOSD(skinMap["bind_shape_matrix"], out mesh.BindShapeMatrix))
					{
						// Logger.Log("Failed to decode bind_shape_matrix matrix", Helpers.LogLevel.Warning);
					}

					if (!TryDecodeMatrixFromOSD(skinMap["pelvis_offset"], out mesh.PelvisOffsetMatrix))
					{
						// Logger.Log("Failed to decode pelvis_offset matrix", Helpers.LogLevel.Warning);
					}

					if (skinMap["joint_names"] is OSDArray jnOsd && skinMap["inverse_bind_matrix"] is OSDArray ibmOsd)
					{
						var jointCount = jnOsd.Count;

						mesh.Joints = new JointInfo[jointCount];

						for (var i = 0; i < jointCount; i++)
						{
							JointInfo jointInfo = new JointInfo();
							jointInfo.Name = jnOsd[i].AsString();
							if (!TryDecodeMatrixFromOSD(ibmOsd[i], out jointInfo.InverseBindMatrix))
							{
								// Logger.Log("Failed to decode inverse_bind_matrix matrix for joint " + jointInfo.Name, Helpers.LogLevel.Warning);
							}
							mesh.Joints[i] = jointInfo;
						}
					}

					if (skinMap["alt_inverse_bind_matrix"] is OSDArray aibmOsd)
					{
						for (var i = 0; i < aibmOsd.Count; i++)
						{
							if (!TryDecodeMatrixFromOSD(aibmOsd[i], out mesh.Joints[i].AltInverseBindMatrix))
							{
								Logger.Log("Failed to decode alt_inverse_bind_matrix matrix for joint " + mesh.Joints[i].Name, Helpers.LogLevel.Warning);
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				Logger.Log("Failed to decode mesh asset: " + ex, Helpers.LogLevel.Error);
				return false;
			}

			return true;
		}
	}


	#endregion Mesh Classes


}