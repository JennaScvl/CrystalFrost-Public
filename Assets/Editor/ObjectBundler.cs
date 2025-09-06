using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// An editor window for bundling selected GameObjects into a prefab, including their meshes, materials, and textures.
/// </summary>
public class ObjectBundler : EditorWindow
{
	string folderPath = "Assets/SavedAssets";

	/// <summary>
	/// Shows the Object Bundler window.
	/// </summary>
	[MenuItem("Tools/Object Bundler")]
	public static void ShowWindow()
	{
		GetWindow<ObjectBundler>("Object Bundler");
	}

	private void OnGUI()
	{
		GUILayout.Label("Specify the folder to save assets", EditorStyles.boldLabel);
		folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

		if (GUILayout.Button("Bundle Selected Objects"))
		{
			BundleObjects();
		}
	}

	string SanitizeAssetName(string name)
	{
		return name.TrimStart().Replace(":", "-");
	}

	Material CreateNewMaterial(Material original)
	{
		Material newMaterial = new Material(original.shader);
		newMaterial.CopyPropertiesFromMaterial(original);
		return newMaterial;
	}

	Texture2D CreateNewTexture(Texture2D original)
	{
		Texture2D newTexture = new Texture2D(original.width, original.height);
		newTexture.SetPixels(original.GetPixels());
		newTexture.Apply();
		return newTexture;
	}

	void EnsureDirectoryExists(string assetPath)
	{
		string directory = Path.GetDirectoryName(assetPath);
		if (!AssetDatabase.IsValidFolder(directory))
		{
			string parentDir = Path.GetDirectoryName(directory);
			string newFolder = Path.GetFileName(directory);
			EnsureDirectoryExists(parentDir);
			AssetDatabase.CreateFolder(parentDir, newFolder);
		}
	}

	void BundleObjects()
	{
		if (!AssetDatabase.IsValidFolder(folderPath))
		{
			Debug.LogError("The specified folder does not exist. Please create it first.");
			return;
		}

		string meshFolder = $"{folderPath}/meshes";
		string materialFolder = $"{folderPath}/materials";
		string textureFolder = $"{folderPath}/textures";

		EnsureDirectoryExists(meshFolder);
		EnsureDirectoryExists(materialFolder);
		EnsureDirectoryExists(textureFolder);

		GameObject[] selectedObjects = Selection.gameObjects;
		if (selectedObjects.Length == 0) return;

		Vector3 center = Vector3.zero;
		foreach (var obj in selectedObjects)
		{
			center += obj.transform.position;
		}
		center /= selectedObjects.Length;

		GameObject root = new GameObject(SanitizeAssetName("RootObject"));
		root.transform.position = center;

		foreach (var obj in selectedObjects)
		{
			obj.transform.SetParent(root.transform);
		}

		foreach (var obj in selectedObjects)
		{
			MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
			foreach (var renderer in renderers)
			{
				MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
				if (meshFilter)
				{
					Mesh mesh = meshFilter.sharedMesh;
					string meshPath = $"{meshFolder}/{SanitizeAssetName(mesh.name)}.asset";
					EnsureDirectoryExists(meshPath);

					if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) == null)
					{
						try
						{
							AssetDatabase.CreateAsset(mesh, meshPath);
						}
						catch
						{
							// Skip this asset if it already exists
							continue;
						}
					}
					meshFilter.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
				}

				Material[] materials = renderer.sharedMaterials;
				for (int i = 0; i < materials.Length; i++)
				{
					Material material = materials[i];
					string materialPath = $"{materialFolder}/{SanitizeAssetName(material.name)}.asset";
					EnsureDirectoryExists(materialPath);

					if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) == null)
					{
						try
						{
							AssetDatabase.CreateAsset(material, materialPath);
						}
						catch
						{
							// Skip this asset if it already exists
							continue;
						}
					}
					materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

					Texture2D texture = material.mainTexture as Texture2D;
					string texturePath = $"{textureFolder}/{SanitizeAssetName(texture.name)}.asset";
					EnsureDirectoryExists(texturePath);

					if (AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath) == null)
					{
						try
						{
							AssetDatabase.CreateAsset(texture, texturePath);
						}
						catch
						{
							// Skip this asset if it already exists
							continue;
						}
					}
					material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
				}
				renderer.sharedMaterials = materials;
			}
		}

		string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{SanitizeAssetName(root.name)}.prefab");
		EnsureDirectoryExists(prefabPath);
		PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
	}
}
