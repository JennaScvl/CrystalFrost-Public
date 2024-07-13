using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;

public struct LindenMesh
{
	public Vector3 position;
	public Vector3 rotationAngles;
	public Vector3 scale;
	public List<Vector3> vertices;
	public List<Vector3> normals;
	public List<Vector3> binormals;
	public List<Vector2> uvs;
	public List<Vector2> detailUV;
	public List<float> weights;
	public List<Dictionary<string, List<LindenVertex>>> morphs;
}

public struct LindenVertex
{
	public int vertexIndex;
	public Vector3 coord;
	public Vector3 normal;
	public Vector3 binormal;
	public Vector2 texCoord;
}

public static class LindenMeshLoader
{
	public static Vector3 ReadVector3(BinaryReader reader)
	{
		Vector3 v = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		return new Vector3(v.x,v.z,v.y);
	}
	public static Vector3 ReadVector2(BinaryReader reader)
	{
		return (new Vector2(reader.ReadSingle(), reader.ReadSingle()));
	}
	/*public static LindenMesh Load(string filePath)
	{
		LindenMesh mesh = new LindenMesh();

		using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
		{
			// Read header
			char[] header = reader.ReadChars(24);
			bool hasWeights = reader.ReadByte() != 0;
			bool hasDetailUV = reader.ReadByte() != 0;
			mesh.position = ReadVector3(reader);
			mesh.rotationAngles = ReadVector3(reader);
			reader.ReadByte(); // Ignore rotationOrder
			mesh.scale = ReadVector3(reader);

			// Read vertices
			int numVertices = reader.ReadUInt16();
			mesh.vertices = new List<Vector3>(numVertices);
			mesh.normals = new List<Vector3>(numVertices);
			mesh.binormals = new List<Vector3>(numVertices);
			mesh.uvs = new List<Vector2>(numVertices);

			for (int i = 0; i < numVertices; i++)
			{
				mesh.vertices.Add(ReadVector3(reader));
				mesh.normals.Add(ReadVector3(reader));
				mesh.binormals.Add(ReadVector3(reader));
				mesh.uvs.Add(ReadVector2(reader));
			}

			// Read detail UV
			if (hasDetailUV)
			{
				mesh.detailUV = new List<Vector2>(numVertices);
				for (int i = 0; i < numVertices; i++)
				{
					mesh.detailUV.Add(ReadVector2(reader));
				}
			}

			// Read weights
			if (hasWeights)
			{
				mesh.weights = new List<float>(numVertices);
				int numSkinJoints = reader.ReadUInt16();
				for (int i = 0; i < numVertices; i++)
				{
					mesh.weights.Add(reader.ReadSingle());
				}
			}

			// Read faces
			int numFaces = reader.ReadUInt16();
			List<int[]> faceData = new List<int[]>(numFaces);

			for (int i = 0; i < numFaces; i++)
			{
				int[] face = new int[3];
				for (int j = 0; j < 3; j++)
				{
					face[j] = reader.ReadInt16();
				}
				faceData.Add(face);
			}

			// Read morphs
			mesh.morphs = new List<Dictionary<string, List<LindenVertex>>>();
			while (true)
			{
				char[] morphName = reader.ReadChars(64);
				if (new string(morphName).TrimEnd('\0') == "End Morphs")
				{
					break;*/
}
