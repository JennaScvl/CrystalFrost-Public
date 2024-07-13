using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SkeletonLoad : MonoBehaviour
{
	private const string fileName = "avatar_skeleton.xml";
	public GameObject bonePrefab;
	public GameObject collisionPrefab;

	public Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
	public Dictionary<string, int> boneIndices = new Dictionary<string, int>();
	public Dictionary<string, Transform> collisionVolumes = new Dictionary<string, Transform>();

	public Dictionary<string, string[]> meshExpandedWeights = new Dictionary<string, string[]>();

	public SkinnedMeshRenderer[] renderers;

	public Transform[] boneTransforms;
	public TextAsset xmlFile;

	public class LLMBone
	{
		public Transform transform;
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;
		public Vector3 end;
		public Vector3 pivot;
		public string group;
		public string aliases;
		public bool connected;
		public string support;
		public LLMBone parent;

		public LLMBone(Transform transform, Vector3 position, Vector3 rotation, Vector3 scale, Vector3 end, Vector3 pivot, string group, string aliases, bool connected, string support)
		{
			Quaternion boneRotOffset = Quaternion.Euler(0f, 90f, 0f);
			this.transform = transform;
			//this.position = position; //use transform position for now because it inherits child positions properly
			//this might need to change to a purely mathematical version in the future
			//but we could always just spawn he skeleton like this at load and then
			//instantiate it any time it's needed by a mesh.
			this.position = transform.position;
			this.rotation = rotation;
			this.scale = scale;
			//this.end = boneRotOffset * end;
			this.end = end;
			this.pivot = pivot;
			this.aliases = aliases;
			this.connected = connected;
			this.support = support;
		}

	}

	public Dictionary<string, LLMBone> llmBones = new Dictionary<string, LLMBone>();

	private void Awake()
	{
		if (bonePrefab == null)
		{
			bonePrefab = new GameObject();
		}

		if (collisionPrefab == null)
		{
			collisionPrefab = new GameObject();
		}

		if (gameObject.transform.Find("mPelvis") != null)
		{
			Destroy(gameObject.transform.Find("mPelvis").gameObject);
		}

		if (bonePrefab == null)
		{
			bonePrefab = new GameObject();
		}
		if (collisionPrefab == null)
		{
			collisionPrefab = new GameObject();
		}

		meshExpandedWeights.Add("avatar_upper_body", new string[]
		{
			"mSpine2",
			"mTorso",
			"mSpine4",
			"mChest",
			"mNeck",
			"mChest",
			"mCollarLeft",
			"mShoulderLeft",
			"mElbowLeft",
			"mWristLeft",
			"mChest",
			"mCollarRight",
			"mShoulderRight",
			"mElbowRight",
			"mWristRight",
			""
		});

		meshExpandedWeights.Add("avatar_lower_body", new string[]
		{
			"mPelvis",
			"mHipRight",
			"mKneeRight",
			"mAnkleRight",
			"mPelvis",
			"mHipLeft",
			"mKneeLeft",
			"mAnkleLeft",
			""
		});

		meshExpandedWeights.Add("avatar_head", new string[]
		{
			"mNeck",
			"mHead",
			""
		});

		var xmlDoc = new XmlDocument();

		if (xmlFile != null)
		{
			xmlDoc.LoadXml(xmlFile.text);
			Debug.Log("XML file loaded successfully");
		}
		else
		{
			Debug.LogError("XML file not assigned in the inspector");
		}

		var rootNode = xmlDoc.DocumentElement;

		var numBones = int.Parse(rootNode.Attributes["num_bones"].Value);

		var boneList = rootNode.GetElementsByTagName("bone");
		var collisionList = rootNode.GetElementsByTagName("collision_volume");
		int count = 0;
		LLMBone parent;
		
		foreach (XmlNode boneNode in boneList)
		{
			var boneName = boneNode.Attributes["name"].Value;
			var bonePosition = ParseVector3(boneNode.Attributes["pos"].Value);
			var boneRotation = ParseVector3Rot(boneNode.Attributes["rot"].Value);
			var boneScale = ParseVector3(boneNode.Attributes["scale"].Value);
			var bonePivot = ParseVector3(boneNode.Attributes["pivot"].Value);

			var boneGameObject = Instantiate<GameObject>(bonePrefab);
			boneGameObject.name = boneName;
			boneGameObject.transform.parent = transform;
			boneGameObject.transform.localPosition = bonePosition;
			boneGameObject.transform.localEulerAngles = boneRotation;
			boneGameObject.transform.localScale = boneScale;
			bones.Add(boneName, boneGameObject.transform);
		}

		boneTransforms = new Transform[bones.Count];

		string parentName = "mPelvis";

		foreach (XmlNode boneNode in boneList)
		{
			parentName = "mPelvis";
			var boneName = boneNode.Attributes["name"].Value;
			//if (boneName == "mPelvis") continue;
			var bonePosition = ParseVector3(boneNode.Attributes["pos"].Value);
			var boneRotation = ParseVector3Rot(boneNode.Attributes["rot"].Value);
			var boneScale = ParseVector3(boneNode.Attributes["scale"].Value);
			if (boneName != "mPelvis")
			{
				bones[boneName].parent = bones[boneNode.ParentNode.Attributes["name"].Value];
				parentName = boneNode.ParentNode.Attributes["name"].Value;
			}

			bones[boneName].localPosition = bonePosition;
			bones[boneName].localEulerAngles = boneRotation;
			bones[boneName].localScale = boneScale;

			boneIndices.Add(boneName, count);
			boneTransforms[count] = bones[boneName];

			count++;

			//Debug.Log($"{boneName} parent = {parentName}");
			llmBones.Add(boneName, new LLMBone
			(
				bones[boneName],
				bonePosition,
				boneRotation,
				boneScale,
				ParseVector3(boneNode.Attributes["end"].Value),
				ParseVector3(boneNode.Attributes["pivot"].Value),
				boneNode.Attributes["group"].Value,
				boneNode.Attributes["aliases"].Value,
				(boneNode.Attributes["connected"].Value != "false"),
				boneNode.Attributes["support"].Value
			));
			if (boneName != "mPelvis")
			{
				llmBones[boneName].parent = llmBones[parentName];
			}

		}

		/*
		foreach (SkinnedMeshRenderer r in renderers)
		{
			//r.rootBone = bones["mPelvis"];
			AvatarLadLoad av = r.GetComponent<AvatarLadLoad>();
			av.skeleton = this;
		}
		*/

		foreach (XmlNode boneNode in boneList)
		{
			Vector3 bonePos = ParseVector3(boneNode.Attributes["end"].Value);
			Vector3 boneEnd = ParseVector3(boneNode.Attributes["end"].Value);
			Debug.DrawLine(bonePos, boneEnd, Color.red, 100000f);
		}

		foreach (XmlNode collision in collisionList)
		{
			var boneName = collision.ParentNode.Attributes["name"].Value;
			var collisionName = collision.Attributes["name"].Value;
			// if (boneName == "mPelvis") continue;
			var bonePosition = ParseVector3(collision.Attributes["pos"].Value);
			var boneRotation = ParseVector3Rot(collision.Attributes["rot"].Value);
			var boneScale = ParseVector3(collision.Attributes["scale"].Value);
			var collisionGameObject = Instantiate<GameObject>(collisionPrefab);
			collisionVolumes.Add(collisionName, collisionGameObject.transform);
			//var bonePivot = ParseVector3(collision.Attributes["pivot"].Value);
			collisionGameObject.name = collisionName;
			collisionGameObject.transform.parent = bones[boneName];
			collisionGameObject.transform.localPosition = bonePosition;
			collisionGameObject.transform.eulerAngles = boneRotation;
			collisionGameObject.transform.localScale = boneScale;
		}
		// Debug.LogError("Collision List    : " + string.Join(", ", collisionList.Cast<XmlNode>().Select(x => x.Attributes["name"].Value).ToArray()));
		// Debug.LogError("Collision Volumes : " + string.Join(", ", collisionVolumes.Keys.ToArray()));

	}

	bool showBento = false;
	private void Update()
	{
		//Draw bone from position to parent position
		/*foreach (KeyValuePair<string, Transform> kvp in bones)
		{
			Transform childTransform = kvp.Value;
			Transform parentTransform = childTransform.parent;

			// If the child has a parent, draw a line between them
			if (parentTransform != null && kvp.Key != "mPelvis")
			{
				Debug.DrawLine(childTransform.position, parentTransform.position, Color.red, 0.05f);
			}
		}*/
		if (Input.GetKeyDown(KeyCode.B))
		{
			showBento = !showBento;
		}
		//Draw bone from position to end
		foreach (KeyValuePair<string, LLMBone> kvp in llmBones)
		{
			if (kvp.Value.support == "base" || showBento)
				Debug.DrawLine(kvp.Value.transform.position, kvp.Value.transform.position + (kvp.Value.transform.rotation * kvp.Value.end), GetColorFromStringHash(kvp.Key), 0.025f);
		}

	}

	public static Color GetColorFromStringHash(string str)
	{
		// Generate a hash code from the input string
		int hash = str.GetHashCode();

		// Convert the hash code to a float between 0 and 1
		float hue = Mathf.Abs(hash % 360) / 360f;

		// Set the saturation and value to fixed values
		float saturation = 0.8f;
		float value = 0.9f;

		// Convert the HSV values to RGB values
		Color color = Color.HSVToRGB(hue, saturation, value);

		return color;
	}

	private static Vector3 ParseVector3(string value)
	{
		var elements = value.Split(' ');
		var x = float.Parse(elements[0]);
		var y = float.Parse(elements[1]);
		var z = float.Parse(elements[2]);
		return new Vector3(x, z, y);
	}

	private static Vector3 ParseVector3Rot(string value)
	{
		var elements = value.Split(' ');
		var x = float.Parse(elements[0]);
		var y = float.Parse(elements[1]);
		var z = float.Parse(elements[2]);
		return new Vector3(y, x, z);
	}
}