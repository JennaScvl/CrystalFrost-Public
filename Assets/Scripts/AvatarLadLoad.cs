using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using CrystalFrost.Assets.Mesh;
using static AvatarLadLoad;
using System.Linq;
using OpenMetaverse.Rendering;
using OpenMetaverse.ImportExport.Collada14;

public class AvatarLadLoad : MonoBehaviour
{

	public string fileName;
	public bool isLOD;
	public SkeletonLoad skeleton;

	public class Morph
	{
		public string morphName;
		public int numVertices;
		public MorphVertex[] morphVertices;

		public Morph(int num)
		{
			numVertices = num;
			morphVertices = new MorphVertex[num];
		}
	}
	public struct MorphVertex
	{
		public uint vertexIndex;
		public Vector3 coord;
		public Vector3 normal;
		public Vector3 binormal;
		public Vector2 texCoord;
	}
	public class Face
	{
		public UInt16[] face = new UInt16[3];

		public Face(UInt16 v1, UInt16 v2, UInt16 v3)
		{
			face[0] = v1;
			face[1] = v2;
			face[2] = v3;
		}
	}
	public class VertexRemap
	{
		public int remapSource;
		public int remapDestination;
	}
	public class LLMeshData
	{
		public string header;
		public bool hasWeights;
		public bool hasDetailTexCoords;
		public Vector3 position;
		public Vector3 rotationAngles;
		public byte rotationOrder;
		public Vector3 scale;
		public UInt16 numVertices;
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector3[] tangents;
		public Vector2[] texCoords;
		public Vector2[] detailTextureCoords;
		public float[] weights;
		public Face[] faces;
		public int numSkinJoints;
		public string[] skinJoints;
		public List<string> expandedSkinJoints;
		public Dictionary<string, Morph> morphs;
		public int numRemaps;
		public VertexRemap[] vertexRemaps;
	}

	public class LLMesh
	{
		public LLMeshData data;
		public UnityEngine.Mesh unityMesh;
		public Transform[] boneTransforms;

		public LLMesh()
		{
			data = new LLMeshData();
			//unityMesh = new Mesh();

		}
	}

	public LLMesh llMesh;
	public UnityEngine.Mesh unityMesh;


	void Start()
	{
		string _fileName = @$"{Application.dataPath}/character/{fileName}.llm";

		llMesh = new LLMesh();
		unityMesh = new UnityEngine.Mesh();
		unityMesh.name = fileName;
		FileStream stream = new FileStream(_fileName, FileMode.Open);
		BinaryReader reader = new BinaryReader(stream);
		SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();

		// Read header information
		//string header = new string(reader.ReadChars(24));
		llMesh.data.header = new string(reader.ReadChars(24));

		llMesh.data.hasWeights = (reader.ReadByte() != 0);
		//Debug.Log($"hasWeights = {llMesh.data.hasWeights}");

		//llMesh.llm.hasWeights = hasWeights;
		llMesh.data.hasDetailTexCoords = (reader.ReadByte() != 0);
		//Debug.Log($"hasDetailTexCoords = {llMesh.data.hasDetailTexCoords}");
		llMesh.data.position = LindenMeshLoader.ReadVector3(reader);
		llMesh.data.rotationAngles = LindenMeshLoader.ReadVector3(reader);
		llMesh.data.rotationOrder = reader.ReadByte();
		llMesh.data.scale = LindenMeshLoader.ReadVector3(reader);
		llMesh.data.numVertices = reader.ReadUInt16();

		// Read vertex data
		llMesh.data.vertices = new Vector3[llMesh.data.numVertices];
		Vertices = new Vertex[llMesh.data.numVertices];
		llMesh.data.normals = new Vector3[llMesh.data.numVertices];
		llMesh.data.texCoords = new Vector2[llMesh.data.numVertices];
		llMesh.data.tangents = new Vector3[llMesh.data.numVertices];
		llMesh.data.detailTextureCoords = new Vector2[llMesh.data.numVertices];
		byte[] bonesPerVertex = new byte[llMesh.data.numVertices];


		float[] weights = new float[llMesh.data.numVertices];
		int[] faces;
		string[] skinJoints;

		//read vertices
		for (int i = 0; i < llMesh.data.numVertices; i++)
		{
			llMesh.data.vertices[i] = LindenMeshLoader.ReadVector3(reader);
			bonesPerVertex[i] = 1;
		}

		//read normals
		for (int i = 0; i < llMesh.data.numVertices; i++)
		{
			llMesh.data.normals[i] = LindenMeshLoader.ReadVector3(reader);
		}

		//read tangents/binormals
		for (int i = 0; i < llMesh.data.numVertices; i++)
		{
			llMesh.data.tangents[i] = LindenMeshLoader.ReadVector3(reader);
		}

		//read UV map
		for (int i = 0; i < llMesh.data.numVertices; i++)
		{
			llMesh.data.texCoords[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		}

		//read detail texture coords
		if (llMesh.data.hasDetailTexCoords)
		{
			for (int i = 0; i < llMesh.data.numVertices; i++)
			{
				llMesh.data.detailTextureCoords[i] = LindenMeshLoader.ReadVector2(reader);
			}
		}

		//read the unexpanded bone weights
		//BoneWeight1[] boneWeights = new BoneWeight1[llMesh.data.numVertices];
		if (llMesh.data.hasWeights)
		{
			llMesh.data.weights = new float[llMesh.data.numVertices];
			for (int i = 0; i < llMesh.data.numVertices; i++)
			{
				//boneWeights[i].weight = reader.ReadSingle();
				llMesh.data.weights[i] = reader.ReadSingle(); //boneWeights[i].weight;
			}
		}

		//read number of faces
		ushort numFaces = reader.ReadUInt16();

		faces = new int[numFaces * 3];
		List<int> lFaces = new List<int>();
		List<Face> _faces = new List<Face>();
		UInt16 v1, v2, v3;

		int counter = 0;

		//read faces

		for (int i = 0; i < numFaces; i++)
		{
			v1 = reader.ReadUInt16();
			v2 = reader.ReadUInt16();
			v3 = reader.ReadUInt16();
			if (v1 == v2 || v1 == v2 || v2 == v3) continue;
			lFaces.Add(v1);
			lFaces.Add(v2);
			lFaces.Add(v3);
			_faces.Add(new Face(v1,v2,v3));
			counter++;
		}
		llMesh.data.faces = _faces.ToArray();

		//read weights
		if(llMesh.data.hasWeights)
		{
			
			List<Line> bonelines = new List<Line>();

			//Debug.Log($"{fileName} {counter} faces");

			llMesh.data.numSkinJoints = reader.ReadUInt16();
			//Debug.Log($"{fileName} {llMesh.data.numVertices} vertices, {llMesh.data.numSkinJoints} skinJoints");
			llMesh.data.skinJoints = new string[llMesh.data.numSkinJoints];
			//char[] chars;
			string skinJoint = string.Empty;
			string debugout = "SkinJoints\n";

			for (int i = 0; i < llMesh.data.numSkinJoints; i++)
			{
				//chars = reader.ReadChars(64);
				skinJoint = CharsToNullTerminatedString(reader.ReadChars(64));
				llMesh.data.skinJoints[i] = skinJoint;

				debugout += $"\"{llMesh.data.skinJoints[i]}\"\n";

			}
			Debug.Log(debugout);
		}

		//read morphs
		string morphName = string.Empty;
		int count = 0;
		Morph morph;
		llMesh.data.morphs = new Dictionary<string, Morph>();
		int _numvertices;
		string morphnames = string.Empty;
		while (true)//(morphName != "End Morphs")
		{
			morphName = CharsToNullTerminatedString(reader.ReadChars(64));
			if (morphName == "End Morphs") break;
			morphnames += $"{morphName}\n";
			_numvertices = reader.ReadInt32();
			llMesh.data.morphs.Add(morphName, new Morph(_numvertices));
			int i;
			//Debug.Log($"{llMesh.data.morphs[morphName].morphVertices.Length} entries in morphVertices");
			for (i = 0; i < llMesh.data.morphs[morphName].numVertices; i++)
			{
				llMesh.data.morphs[morphName].morphVertices[i] = new MorphVertex();
				llMesh.data.morphs[morphName].morphVertices[i].vertexIndex = reader.ReadUInt32();
				llMesh.data.morphs[morphName].morphVertices[i].coord = LindenMeshLoader.ReadVector3(reader);
				llMesh.data.morphs[morphName].morphVertices[i].normal = LindenMeshLoader.ReadVector3(reader);
				llMesh.data.morphs[morphName].morphVertices[i].binormal = LindenMeshLoader.ReadVector3(reader);
				llMesh.data.morphs[morphName].morphVertices[i].texCoord = LindenMeshLoader.ReadVector2(reader);
			}
			morphs.Add(morphName, 0f);
			morphchanges.Add(morphName, 0f);
			count++;
		}
		Debug.Log(morphnames);

		//Aside from the remaps, that should be about it... just not sure what remaps even are.

		//from here on convert the above read data for Unity.

		//skeleton.llmBones[""].
		if (llMesh.data.hasWeights)
		{
			string parent = "";
			foreach (string s in llMesh.data.skinJoints)
			{
				parent += s + "\n";
			}
			//Debug.Log(parent);

			Debug.Log("Beginning recursive search of skeleton");
			llMesh.data.expandedSkinJoints = new List<string>();
			//ExpandSkinJointList(skeleton.llmBones["mPelvis"].transform);
		
			parent = "";
			foreach(string joint in llMesh.data.expandedSkinJoints)
			{
				parent += joint + "\n";
			}
			if(fileName == "avatar_upper_body") Debug.Log(parent);

		}

		unityMesh.vertices = llMesh.data.vertices;
		unityMesh.normals = llMesh.data.normals;
		unityMesh.uv = llMesh.data.texCoords;
		//unityMesh.SetTriangles(faces, 0);
		unityMesh.SetTriangles(lFaces.ToArray(), 0);
		unityMesh.ReverseWind();

		Matrix4x4[] bindposes = new Matrix4x4[skeleton.meshExpandedWeights[fileName].Length - 1];
		BoneWeight[] boneWeights = new BoneWeight[llMesh.data.numVertices];

		if (llMesh.data.hasWeights)
		{
			//faces = _faces.ToArray();

			List<Line> bonelines = new List<Line>();

			Debug.Log($"{fileName} {counter} faces");
			Transform[] boneTransforms;
			Dictionary<string, int> boneIndices = new Dictionary<string, int>();
			//Matrix4x4[] bindPoses = new Matrix4x4[0];
			if (llMesh.data.hasWeights)
			{
				//if (false)
				//{
					//llMesh.data.numSkinJoints = reader.ReadUInt16();
					Debug.Log($"{fileName} {llMesh.data.numVertices} vertices, {llMesh.data.expandedSkinJoints.Count()} expanded skinJoints");
					//llMesh.data.skinJoints = new string[llMesh.data.numSkinJoints];
					/*char[] chars;// = new char[64];
					string skinJoint = string.Empty;
					count = 0;
					bindPoses = new Matrix4x4[llMesh.data.expandedSkinJoints.Count()];
					boneTransforms = new Transform[llMesh.data.expandedSkinJoints.Count()];
					for (int i = 0; i < llMesh.data.expandedSkinJoints.Count(); i++)
					{
						skinJoint = llMesh.data.expandedSkinJoints[i];
						//llMesh.data.skinJoints[i] = skinJoint;

						if (i == 0) renderer.rootBone = skeleton.llmBones[skinJoint].transform;

						//Debug.Log($"\"{llMesh.data.expandedSkinJoints[i]}\"");

						bonelines.Add(new Line(skinJoint, skeleton.llmBones[skinJoint].position, skeleton.llmBones[skinJoint].position + skeleton.llmBones[skinJoint].end));

						bindPoses[i] = skeleton.llmBones[skinJoint].transform.worldToLocalMatrix * transform.worldToLocalMatrix;
						boneTransforms[i] = skeleton.llmBones[skinJoint].transform;
						boneIndices.TryAdd(skinJoint, i);
						//These probably don't match up since there's more weights than skinJoints;
						//skeleton.Bones is a dictionary that converts names to bone indices
						//however the skeleton isn't completely ready for use either.
						//as as far as I know, I still need to actually go and add the transforms
						//for the bones to an array to for the mesh to know which indice is which bone
						//boneWeights[i].boneIndex = skeleton.boneIndices[skinJoints[i]];
					}
					Debug.Log("Beginning weight bone readout");
					*/
					string boneName = string.Empty;
					int bone;
				/*for (int i = 0; i < llMesh.data.numVertices; i++)
				{
					if (bonelines == null) Debug.Log("bonelines null");
					bone = Mathf.FloorToInt(llMesh.data.weights[i]) - 1;
					boneName = llMesh.data.expandedSkinJoints[bone];
					//Debug.Log(llMesh.data.expandedSkinJoints[Mathf.FloorToInt(llMesh.data.weights[i]) - 1]);
					//string boneName = llMesh.data.expandedSkinJoints[llFloor(llMesh.data.weights[i]];//FindNearestLineName(bonelines, llMesh.data.vertices[i]);
					if (boneName == null) Debug.Log("boneName null");
					boneWeights[i].boneIndex = bone; //boneTransforms[bone];
					//Vertices[i] = new Vertex(boneName, llMesh.data.vertices[i]);
				}
				renderer.bones = boneTransforms;//skeleton.boneTransforms;*/
				//}
				//else
				//{
				//UInt16 numSkinJoints = reader.ReadUInt16();
				/*int numSkinJoints = llMesh.data.expandedSkinJoints.Count();
				Debug.Log($"{fileName} {llMesh.data.numVertices} vertices, {numSkinJoints} skinJoints");
				skinJoints = new string[numSkinJoints];
				char[] chars;// = new char[64];
				string skinJoint = string.Empty;
				count = 0;
				bindPoses = new Matrix4x4[numSkinJoints];
				boneTransforms = new Transform[numSkinJoints];
				for (int i = 0; i < numSkinJoints; i++)
				{
					//chars = reader.ReadChars(64);
					skinJoint = //chars[0].ToString();
					count = 1;
					while (chars[count] != '\0')
					{
						skinJoint += chars[count].ToString();
						count++;
					}
					skinJoints[i] = skinJoint;

					if (i == 0) renderer.rootBone = skeleton.llmBones[skinJoint].transform;

					Debug.Log($"\"{skinJoints[i]}\"");

					bonelines.Add(new Line(skinJoint, skeleton.llmBones[skinJoint].position, skeleton.llmBones[skinJoint].position + skeleton.llmBones[skinJoint].end));

					bindPoses[i] = skeleton.llmBones[skinJoint].transform.worldToLocalMatrix * transform.worldToLocalMatrix;
					boneTransforms[i] = skeleton.llmBones[skinJoint].transform;
					boneIndices.Add(skinJoint, i);
					//These probably don't match up since there's more weights than skinJoints;
					//skeleton.Bones is a dictionary that converts names to bone indices
					//however the skeleton isn't completely ready for use either.
					//as as far as I know, I still need to actually go and add the transforms
					//for the bones to an array to for the mesh to know which indice is which bone
					//boneWeights[i].boneIndex = skeleton.boneIndices[skinJoints[i]];
				}*/
				int boneIndex;
				int boneCount = 0;

				Transform[] meshBones = new Transform[skeleton.meshExpandedWeights[fileName].Length-1];

				for(int i = 0; i < meshBones.Length; i++)
				{
					meshBones[i] = skeleton.bones[skeleton.meshExpandedWeights[fileName][i]];
					bindposes[i] = meshBones[i].worldToLocalMatrix * transform.localToWorldMatrix;
				}

				unityMesh.bindposes = new Matrix4x4[meshBones.Length];
				unityMesh.boneWeights = new BoneWeight[llMesh.data.numVertices];

				for (int i = 0; i < llMesh.data.numVertices; i++)
				{
					int firstBoneIndex = Mathf.FloorToInt(llMesh.data.weights[i]);
					float firstBoneWeight = llMesh.data.weights[i] - firstBoneIndex;

					int secondBoneIndex = firstBoneIndex + 1;
					if (secondBoneIndex >= meshBones.Length -1) // handling case when index is the last in the array
					{
						secondBoneIndex = firstBoneIndex;
					}
					//float secondBoneWeight = 1.0f - firstBoneWeight;

					boneWeights[i].boneIndex0 = firstBoneIndex;
					boneWeights[i].weight0 = firstBoneWeight;

					boneWeights[i].boneIndex1 = secondBoneIndex;
					boneWeights[i].weight1 = 1f - firstBoneWeight;

				}

				unityMesh.boneWeights = boneWeights;
				unityMesh.bindposes = bindposes;
				renderer.bones = meshBones;
				//unityMesh.bones = meshBones;
				//}
			}



			Unity.Collections.NativeArray<BoneWeight> _boneWeights = new Unity.Collections.NativeArray<BoneWeight>(boneWeights, Unity.Collections.Allocator.Temp);
			Unity.Collections.NativeArray<byte> _bonesPerVertex = new Unity.Collections.NativeArray<byte>(bonesPerVertex, Unity.Collections.Allocator.Temp);

			// Create mesh

			//bw.
			// Set LOD group settings if applicable
			if (isLOD)
			{
				MeshFilter meshFilter = GetComponent<MeshFilter>();
				meshFilter.mesh = unityMesh;
				//SkinnedMeshRenderer meshRenderer = GetComponent<SkinnedMeshRenderer>();
				renderer.enabled = false;
			}
			else
			{
				//SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
				renderer.sharedMesh = unityMesh;

				GetComponent<MeshFilter>().mesh = unityMesh;
				renderer.ResetBounds();
			}
			//unityMesh.SetBoneWeights(_bonesPerVertex, _boneWeights); //no idea how to do this
			//unityMesh.bindposes = bindPoses;
			//unityMesh.SetIndices()
			renderer.sharedMesh = unityMesh;
			renderer.sharedMesh.RecalculateBounds();
		}



		unityMesh.RecalculateBounds();
		renderer.updateWhenOffscreen = true;
		reader.Close();
		stream.Close();

		ready = true;
	}

	int ClampInt(int value, int minValue, int maxValue)
	{
		if (value < minValue)
			return minValue;
		else if (value > maxValue)
			return maxValue;
		else
			return value;
	}

	void ExpandSkinJointList(Transform parent)
	{
		string parentName;

		// Loop through each child transform
		//Debug.Log($"Initial Child Count: {parent.childCount}");
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (!llMesh.data.skinJoints.Contains(child.name))
			{
				//Debug.Log($"skinJoints DOES NOT contain bone: {child.name}. Moving on");
			}
			else
			{
				//Debug.Log($"skinJoints contains bone: {child.name}. Adding to list.");
				parentName = skeleton.llmBones[child.name].parent.transform.name;

				if (llMesh.data.expandedSkinJoints.Count == 0 || llMesh.data.expandedSkinJoints.Last() != parentName)
				{
					llMesh.data.expandedSkinJoints.Add(parentName);
					//Debug.Log($"Adding Parent:{parentName}");
				}
				//Debug.Log($"Adding Bone:{child.name}");
				llMesh.data.expandedSkinJoints.Add(child.name);
			}
			// Recursively call the function for each child's children
			ExpandSkinJointList(child);
		}
	}

	string CharsToNullTerminatedString(char[] c)
	{
		string retstring;
		//char[] _morphNameChars = reader.ReadChars(64);
		retstring = c[0].ToString();
		int count = 1;
		while (c[count] != '\0')
		{
			retstring += c[count].ToString();
			count++;
		}
		return retstring;
	}

	public class Line
	{
		public string lineName;
		public Vector3 lineStart;
		public Vector3 lineEnd;

		public Line(string lineName, Vector3 lineStart, Vector3 lineEnd)
		{
			this.lineName = lineName;
			this.lineStart = lineStart;
			this.lineEnd = lineEnd;
		}
	}

	class Vertex
	{
		public Color color;
		public Vector3 position;

		public Vertex(string name, Vector3 position)
		{
			color = SkeletonLoad.GetColorFromStringHash(name);
			this.position = position;
		}
	}

	Vertex[] Vertices;

	int visualizeOriginalVertexPositions = 0;
	private void Update()
	{
		//return;
		float dotSize = 0.001f;
		//Vector3[] vertices = gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.vertices;
		Vector3 vertex;
		//Vector3 skinnedVertexPosition;// = skinnedMeshRenderer.localToWorldMatrix.MultiplyPoint(skinnedMesh.vertices[vertexIndex])
		if (Input.GetKeyDown(KeyCode.Space))
		{
			visualizeOriginalVertexPositions++;// !visualizeOriginalVertexPositions;
			if (visualizeOriginalVertexPositions == 3) visualizeOriginalVertexPositions = 0;
		}
		else
		{
			switch (visualizeOriginalVertexPositions)
			{
				case 1:
					//Debug.Log("original");
					for (int i = 0; i < Vertices.Length; i++)
					{
						if (Vertices[i] == null) break;
						vertex = transform.TransformPoint(Vertices[i].position);
						Debug.DrawLine(vertex - Vector3.up * dotSize, vertex + Vector3.up * dotSize, Vertices[i].color);
						Debug.DrawLine(vertex - Vector3.left * dotSize, vertex + Vector3.left * dotSize, Vertices[i].color);
						Debug.DrawLine(vertex - Vector3.forward * dotSize, vertex + Vector3.forward * dotSize, Vertices[i].color);
					}
					break;
				case 2:
					//Debug.Log("not original");
					SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
					UnityEngine.Mesh bakedMesh = new UnityEngine.Mesh();
					skinnedMeshRenderer.BakeMesh(bakedMesh);

					for (int i = 0; i < bakedMesh.vertices.Length; i++)
					{
						if (bakedMesh.vertices[i] == null) break;
						vertex = skinnedMeshRenderer.transform.TransformPoint(bakedMesh.vertices[i]);
						Debug.DrawLine(vertex - Vector3.up * dotSize, vertex + Vector3.up * dotSize, Vertices[i].color);
						Debug.DrawLine(vertex - Vector3.left * dotSize, vertex + Vector3.left * dotSize, Vertices[i].color);
						Debug.DrawLine(vertex - Vector3.forward * dotSize, vertex + Vector3.forward * dotSize, Vertices[i].color);
					}
					break;
				default:
					break;
			}
		}
	}

	Dictionary<string, float> morphs = new Dictionary<string, float>();
	Dictionary<string, float> morphchanges = new Dictionary<string, float>();
	List<string> morphdeltanames = new List<string>();
	bool ready = false;
	private void OnGUI()
	{
		if (!ready) return;
		// Create a horizontal slider
		if (fileName != "avatar_upper_body") return;
		//float _bigBellyTorso = GUI.HorizontalSlider(new Rect(25, 25, 200, 30), bigBellyTorso, -1f, 1f);
		//float _breastSizeSlider = GUI.HorizontalSlider(new Rect(25, 25, 200, 30), breastSizeSlider, -1f, 1f);
		//float _breastCleavage = GUI.HorizontalSlider(new Rect(25, 25, 200, 30), breastSizeSlider, -1f, 1f);
		int counter = 0;
		bool changed = false;
		foreach(KeyValuePair<string, Morph> kvp in llMesh.data.morphs)
		{
			string name = kvp.Key;
			//float value = kvp.Value;
			morphchanges[name] = GUI.HorizontalSlider(new Rect(25, 5 + (counter * 32), 200, 30), morphchanges[name], 0f, 1f);
			GUI.Label(new Rect(25, 12 + (counter * 32), 200, 30), $"{name}: {morphchanges[name]}");
			counter++;
			if (morphchanges[name] != morphs[name])
			{
				morphdeltanames.Add(name);
				morphs[name] = morphchanges[name];
				changed = true;
			}
		}

		if (changed)
		{
			UnityEngine.Mesh modifiedMesh = Instantiate(unityMesh);
			Vector3[] vertices = new Vector3[modifiedMesh.vertexCount];
			Vector3[] mVertices = modifiedMesh.vertices;
			string o = string.Empty;
			foreach (KeyValuePair<string, float> kvp in morphs)
			{
				//morphs[name] = value;
				string name = kvp.Key;
				float value = kvp.Value;
				if (value > 0.01)
				{
					o += name + ", ";
					vertices = MorphMesh(name, morphchanges[name]);
					for (int i = 0; i < vertices.Length; i++)
					{
						mVertices[i] += vertices[i];
					}
				}
			}
			Debug.Log(o);
			modifiedMesh.vertices = mVertices;
			SkinnedMeshRenderer renderer = gameObject.GetComponent<SkinnedMeshRenderer>();
			renderer.sharedMesh = modifiedMesh;

		}
		morphdeltanames.Clear();
	}

	Vector3[] MorphMesh(string morphName, float val)
	{
		Morph morph = llMesh.data.morphs[morphName];
		Vector3[] vertices = new Vector3[llMesh.data.numVertices];
//		Vector3[] morphVertice;
		for (int i = 0; i < morph.numVertices; i++)
		{
//			morphVertice = vertices[morph.morphVertices[i].vertexIndex] + morph.morphVertices[i].coord;
			vertices[morph.morphVertices[i].vertexIndex] = Vector3.Lerp(Vector3.zero, morph.morphVertices[i].coord, val);
		}
		//mesh.vertices = vertices;
		//renderer.mesh = modifiedMesh;
		//renderer.sharedMesh = modifiedMesh;
		return vertices;
	}

	List<Line> lines = new List<Line>();

	public string FindNearestLineName(List<Line> lines, Vector3 point)
	{
		float shortestDistance = float.MaxValue;
		string nearestLineName = "";

		foreach (Line line in lines)
		{
			Vector3 closestPoint = GetClosestPointOnFiniteLine(line.lineStart, line.lineEnd, point);
			float distance = Vector3.Distance(closestPoint, point);

			if (distance < shortestDistance)
			{
				shortestDistance = distance;
				nearestLineName = line.lineName;
			}
		}

		return nearestLineName;
	}

	Vector3 GetClosestPointOnFiniteLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 line_direction = lineEnd - lineStart;
		float line_length = line_direction.magnitude;
		line_direction.Normalize();
		float project_length = Mathf.Clamp(Vector3.Dot(point - lineStart, line_direction), 0f, line_length);
		return lineStart + line_direction * project_length;
	}
}
