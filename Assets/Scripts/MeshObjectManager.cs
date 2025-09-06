using CrystalFrost.Assets.Mesh;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.VisualScripting;
using UnityEngine;
using static CrystalFrost.CFAssetManager;
using CrystalFrost.Assets.Animation;


public class MeshObjectManager : MonoBehaviour
{
	public SkeletonLoad skeleton;
	private static MeshObjectManager s_Instance;
	public static MeshObjectManager Instance { get { return s_Instance; } }

	public Dictionary<OpenMetaverse.UUID, OpenMetaverse.Primitive> avatarPrimitiveMap;
	public Dictionary<uint, SkeletonManager> avatarPrimiToSkMan;

	void Awake()
	{
		if (s_Instance == null && s_Instance != this)
		{
			Destroy(this);
		}
		else
		{
			s_Instance = this;
		}

		avatarPrimitiveMap = new();
		avatarPrimiToSkMan = new();
	}

	private GameObject CreateAvatarObject(GameObject start)
	{
		var startT = start.transform;
		var possibleParents = new List<GameObject>();
		while (startT != null)
		{
			foreach (Transform child in startT)
			{
				if (child.gameObject.GetComponent<PrimInfo>() != null)
				{
					possibleParents.Add(startT.gameObject);
					break;
				}
			}
			startT = startT.parent;
		}
		if (possibleParents.Count == 0) return null;

		var parent = possibleParents[possibleParents.Count - 1];
		var avatarObject = Instantiate(skeleton.gameObject, parent.transform);
		avatarObject.name = "Avatar";
		avatarObject.AddComponent<SkeletonManager>();
		avatarObject.transform.localPosition = Vector3.zero;
		avatarObject.transform.localRotation = Quaternion.identity;
		avatarObject.transform.localScale = Vector3.one;
		return avatarObject;
	}

	private GameObject FindAvatarForObject(GameObject start)
	{
		if (start == null) return null;
		GameObject avatar = null;
		foreach (Transform child in start.transform)
		{
			if (child.name == "Avatar")
			{
				avatar = child.gameObject;
				break;
			}
		}
		if (avatar != null) return avatar.gameObject;
		return FindAvatarForObject(start.transform.parent?.gameObject);
	}

	private GameObject FindOrCreateAvatarForObject(GameObject start)
	{
		var avatar = FindAvatarForObject(start);
		if (avatar != null) return avatar;
		var avatarObject = CreateAvatarObject(start);
		return avatarObject;
	}

	public void SetupAnimation(AnimationRequest animation)
	{
		var decodedAnim	= animation.DecodedAnimation;
		var prim = animation.Primitive;
		// prim is the Avatar prim, and 'prim.localID' is parentID for its child prims.
		// we mapped skeletonManager with parentID of prim which is Avatar prim Local ID that we get from DecodedAnimation
		if (prim == null) return;

		// NOTE: Ideally here if it is not prresent add it to a list (pool) and check occassionally in a 
		// coroutine if the skeleton got setup or not.
		
		if (avatarPrimiToSkMan.TryGetValue(prim.LocalID, out var skMan))
		{
 			skMan.AddAnimation(decodedAnim);
		}
		else
		{
			// Debug.LogError("No skeleton manager found for prim: " + prim.LocalID);
		}
	}

	public void SetupAnimation(OpenMetaverse.UUID animationID, OpenMetaverse.Primitive prim)
	{
		if (prim == null) return;

		if (avatarPrimiToSkMan.TryGetValue(prim.LocalID, out var skMan))
		{
			skMan.AddAnimation(animationID);
		}
		else
		{
			// Debug.LogError("No skeleton manager found for prim: " + prim.LocalID);
		}
	}

	public void OnAvatarUpdate(OpenMetaverse.AvatarUpdateEventArgs e)
	{
		if (this.avatarPrimitiveMap.ContainsKey(e.Avatar.ID))
			this.avatarPrimitiveMap[e.Avatar.ID] = e.Avatar;
		else
			this.avatarPrimitiveMap.Add(e.Avatar.ID, e.Avatar);
	}

	/// <summary>
	/// Creates renderers for the given request.
	/// </summary>
	/// <param name="item"></param>
	public void SetupMeshObject(MeshRequest item)
	{
		DebugStatsManager.AddStateUpdate(DebugStatsType.DecodedMeshProcess, item.AssetMesh.AssetID.ToString());

		var mesh = new SLMeshData();
		/*
#if RenderHighestDetail
		mesh.meshHighest = item.DecodedMesh.meshData.ToUnityMeshArray();
#endif
#if !RenderHighDetail
        mesh.meshHigh = item.DecodedMesh.meshData.ToUnityMeshArray();
#endif
#if !RenderMediumDetail
        mesh.meshMedium = item.DecodedMesh.meshData.ToUnityMeshArray();
#endif
		*/
		mesh.meshHighest = item.DecodedMesh.meshData.ToUnityMeshArray();

		// What is going on here? We add to the cache? but do we ever read from it?
		ClientManager.assetManager.meshCache.TryAdd(item.UUID, mesh);

		if (item.MeshHolder.IsDestroyed())
		{
			return;
		}

		var group = item.MeshHolder.GetComponent<LODGroup>();

		// This was cauing some usses with skinned meshes
		if (item.DecodedMesh.isSkinned)
		{
			group.enabled = false;
		}

		List<LOD> lods = new();

		/*
#if RenderHighestDetail
		var highest = CreateRenderers(mesh.meshHighest, item, "highest");
		lods.Add(new LOD(0.5f, highest.ToArray()));
#endif
#if !RenderHighDetail
        var high = CreateRenderers(mesh.meshHighest, item, "high"); // is mesh.meshHighest a bug?
		lods.Add(new LOD(0.333f, high.ToArray()));
#endif
#if !RenderMediumDetail
        var medium = CreateRenderers(mesh.meshHighest, item, "medium"); // is mesh.meshHighest a bug?
		lods.Add(new LOD(0.25f, medium.ToArray()));
#endif
		*/

		var highest = CreateRenderers(mesh.meshHighest, item, "highest");
		lods.Add(new LOD(0.5f, highest.ToArray()));

		group.SetLODs(lods.ToArray());
		group.fadeMode = LODFadeMode.SpeedTree;
		group.animateCrossFading = true;
		group.RecalculateBounds();
		if (item.Primitive.IsAttachment)
			group.size = 10f;
		else
			group.size = 10f;


		group.enabled = false;
	}

	private List<Renderer> CreateRenderers(Mesh[] meshes, MeshRequest request, string level)
	{
		var result = new List<Renderer>();
		var decodedMesh = request.DecodedMesh;
		
		// decodedMesh.isSkinned = false;

		if (request.Primitive == null || request.GameObject == null) return result;


		for (var i = 0; i < meshes.Length; i++)
		{
			var allChildren = request.GameObject.GetComponentsInChildren<Transform>();

			var namePart = $"face {i}.{level}";

			if (allChildren.Any(c => c.gameObject.name.Contains(namePart))) continue;

			GameObject adder = null;
			
			if (decodedMesh.isSkinned)
				adder = CreateMeshObjectSkinned(meshes[i], level, i, request);
			else
				adder = CreateMeshObjectStatic(meshes[i], level, i, request);
			
			if (adder != null) result.Add(adder.GetComponent<Renderer>());
		}

		if (decodedMesh.isSkinned)
		{
			// This makes sure the skeleton is the only one transforming the mesh
			request.GameObject.transform.localPosition = Vector3.zero;
			request.GameObject.transform.localRotation = Quaternion.identity;
			request.GameObject.transform.localScale = Vector3.one;

			var skeletonManager = FindOrCreateAvatarForObject(request.MeshHolder)?.GetComponent<SkeletonManager>();
			if  (skeletonManager == null) throw new Exception("SkeletonManager not found");
			skeletonManager.AddMeshObject(request.Primitive.ID, request.DecodedMesh, request.MeshHolder);
			var prim = request.Primitive;
			var ID = prim.ParentID;
			if (ID == 0)
			{
				ID = prim.LocalID;
			}
			// here we mapped skeletonManager with parentID of prim which is Avatar prim Local ID that we get from DecodedAnimationData
			this.avatarPrimiToSkMan.TryAdd(ID, skeletonManager);
		}

		return result;
	}

	// This one just creates the game object to hold the mesh. The bindposes and the bones are setup by the SkeletonManager
	private GameObject CreateMeshObjectSkinned(Mesh mesh, string detail, int i, MeshRequest request)
	{
		var go = Instantiate(Resources.Load<GameObject>("CubeSkinned"));

		go.name = $"mesh skinned face {i}.{detail}";
		go.transform.parent = request.MeshHolder.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;

		var skmr = go.GetComponent<SkinnedMeshRenderer>();
		skmr.sharedMesh = mesh;


		PrimInfo pi = go.GetComponent<PrimInfo>();
		pi.face = i;
		pi.localID = request.Primitive.LocalID;
		pi.uuid = request.Primitive.ID;
		pi.prim = request.Primitive;

		SimManager.PreTextureFace(request.Primitive, i, go.GetComponent<Renderer>());

		return go;
	}

	private GameObject CreateMeshObjectStatic(Mesh mesh, string detail, int i, MeshRequest request)
	{
		var go = Instantiate(Resources.Load<GameObject>("Cube"));
		//if (detail != "highest")Destroy(go.GetComponent<MeshFilter>());
		go.name = $"mesh face {i}.{detail}";

		var mf = go.GetComponent<MeshFilter>();
		mf.mesh = mesh;
		go.transform.parent = request.MeshHolder.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;
		go.transform.localRotation = Quaternion.identity;

		PrimInfo pi = go.GetComponent<PrimInfo>();
		pi.face = i;
		pi.localID = request.Primitive.LocalID;
		pi.uuid = request.Primitive.ID;
		pi.prim = request.Primitive;

		if (detail == "highest" && mf.mesh.vertices.Distinct().Count() >= 3)
		{
			var mc = go.GetComponent<MeshCollider>();
			//mc.enabled = false;
			mc.cookingOptions = MeshColliderCookingOptions.None;//MeshColliderCookingOptions.WeldColocatedVertices;//MeshColliderCookingOptions.CookForFasterSimulation;
			mc.sharedMesh = mf.mesh;
			mc.enabled = true;
		}

		SimManager.PreTextureFace(request.Primitive, i, go.GetComponent<MeshRenderer>());

		return go;
	}



}
