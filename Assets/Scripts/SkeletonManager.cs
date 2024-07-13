using CrystalFrost.Assets.Mesh;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System;
using CrystalFrost.Extensions;
using CrystalFrost.Assets.Animation;

// Responsible for managing all the meshes/objects using the same skeleton
// also responsbile for storing a state per skeleton and also used for overrides
[RequireComponent(typeof(SkeletonLoad)), RequireComponent(typeof(UnityEngine.Animation))]
public class SkeletonManager : MonoBehaviour
{
	private SkeletonLoad skeleton;
	private Dictionary<UUID, DecodedMesh> meshes = new();
	private Dictionary<UUID, GameObject> unityMeshHolders = new();
	private Dictionary<string, Tuple<UUID, JointInfo>> jointOverrides = new Dictionary<string, Tuple<UUID, JointInfo>>();
	private UnityEngine.Animation animation = null;
	private Queue<String> animationsToBePlayed = new();
	private Coroutine animationCoroutine = null;

	private static Dictionary<UUID, AnimationClip> cachedClips = new Dictionary<UUID, AnimationClip>();

	public static bool HasClip(UUID animID)
	{
		// Debug.Log($"Skeleton Manager current clips: {cachedClips.Count}");
		return cachedClips.ContainsKey(animID);
	}

	private Vector3 pelvisPos;

	public void Awake()
	{
		this.skeleton = GetComponent<SkeletonLoad>();
		this.skeleton.enabled = false;

		this.pelvisPos = skeleton.bones["mPelvis"].localPosition;

		foreach (var joint in this.skeleton.bones.Keys)
		{
			this.jointOverrides.Add(joint, new Tuple<UUID, JointInfo>(UUID.Zero, null));
		}

		foreach (var joint in this.skeleton.collisionVolumes.Keys)
		{
			this.jointOverrides.Add(joint, new Tuple<UUID, JointInfo>(UUID.Zero, null));
		}

		this.animation = GetComponent<UnityEngine.Animation>();
		animation.cullingType = AnimationCullingType.AlwaysAnimate;
		animation.playAutomatically = false;
		animation.enabled = true;
	}

	public void AddMeshObject(UUID primID, DecodedMesh decodedMesh, GameObject meshHolder)
	{
		DebugStatsManager.AddStateUpdate(DebugStatsType.DecodedSkinnedMeshProcess, "");
		if (!this.meshes.TryAdd(primID, decodedMesh) || !this.unityMeshHolders.TryAdd(primID, meshHolder))
		{
			Debug.LogError($"Failed to add mesh {primID} to skeleton {this.skeleton.name}");
			return;
		}

		skeleton.bones["mPelvis"].localPosition = pelvisPos;
		transform.localRotation = Quaternion.identity;

		this.ApplyOverrides(primID);

		this.UpdateBones();

		this.UpdateMesh(primID);

		this.pelvisPos = skeleton.bones["mPelvis"].localPosition;

		// This is a hack gotta find a fix for this!
		// gameObject.transform.position -= skeleton.bones["mPelvis"].localPosition;
		skeleton.bones["mPelvis"].localPosition = Vector3.zero; 
		transform.localRotation = Quaternion.Euler(new Vector3(0, 90.0f, 0));
	}

	private void ApplyOverrides(UUID primId)
	{
		if (this.meshes.TryGetValue(primId, out var decodedMesh))
		{
			var joints = decodedMesh.joints;


			for (var i = 0; i < joints.Length; i++)
			{
				var joint = joints[i];
				if (!skeleton.bones.ContainsKey(joint.Name))
				{
					continue;
				}

				if (jointOverrides.TryGetValue(joint.Name, out Tuple<UUID, JointInfo> jointOverride))
				{
					if (jointOverride.Item1.Equals(UUID.Zero) || decodedMesh.assetId.CompareTo(jointOverride.Item1) > 0)
					{
						jointOverrides[joint.Name] = new Tuple<UUID, JointInfo>(decodedMesh.assetId, joint);
					}
				}
			}
		}
	}

	private void UpdateBones()
	{
		// need to apply bind shape here somehow
		var skeletonHolder = this.skeleton.transform;

		foreach (var jointOverride in jointOverrides)
		{
			var jointName = jointOverride.Key;
			var joint = jointOverride.Value.Item2;
			var meshId = jointOverride.Value.Item1;

			Transform bone = null;

			if (meshId.Equals(UUID.Zero))
			{
				// No override for this joint
				continue;
			}


			if (!skeleton.bones.ContainsKey(jointName))
			{
				Debug.LogError($"Cannot find joint {jointName} for joint override {jointName}");
				if (!skeleton.collisionVolumes.ContainsKey(jointName))
				{
					Debug.LogError($"Cannot find joint {jointName} for joint override {jointName}");
					continue;
				}
				else
				{
					bone = skeleton.collisionVolumes[jointName];
				}
			}
			else
			{
				bone = skeleton.bones[jointName];
			}

			if (joint.AltInverseBindMatrix != null)
			{
				var jointPosition = joint.AltInverseBindMatrixUnity.InverseBindMatrixArrayHandConversion().GetPosition();
				// string ss = bone.localPosition + " -> " + jointPosition;
				// ss += "\n" + bone.position + " -> " + jointPosition;
				bone.localPosition = jointPosition;
				// Debug.LogError($"Joint {jointName} has an alt inverse bind matrix {joint.AltInverseBindMatrixUnity.InverseBindMatrixArrayHandConversion()}:\n {ss}");
			}
		}
	}

	public Matrix4x4[] GetBindposes(UUID primId)
	{
		if (this.meshes.TryGetValue(primId, out var decodedMesh))
		{
			var joints = decodedMesh.joints;
			var result = new Matrix4x4[joints.Length];

			var bindShape = decodedMesh.bindShapeMatrix.InverseBindMatrixArrayHandConversion();

			for (var i = 0; i < joints.Length; i++)
			{

				var joint = joints[i];
				result[i] = joint.InverseBindMatrixUnity.InverseBindMatrixArrayHandConversion() * bindShape;
				//if (joint.AltInverseBindMatrix != null)
				//{
				//	result[i] = (joint.AltInverseBindMatrixUnity.InverseBindMatrixArrayHandConversion() * bindShape);
				//}

			}

			return result;
		}

		Debug.LogError("Cannot GetBindposes for unknown mesh!");
		return null;
	}

	public Transform[] GetBones(UUID primId)
	{
		if (this.meshes.TryGetValue(primId, out var decodedMesh))
		{
			var joints = decodedMesh.joints;
			var result = new Transform[joints.Length];

			for (var i = 0; i < joints.Length; i++)
			{
				var joint = joints[i];
				if (skeleton.bones.ContainsKey(joint.Name))
				{
					result[i] = skeleton.bones[joint.Name];
				}
				else if (skeleton.collisionVolumes.ContainsKey(joint.Name))
				{
					result[i] = skeleton.collisionVolumes[joint.Name];
				}
				else
				{
					Debug.LogError($"The joint {joint.Name} is not supported");
				}
			}

			return result;
		}

		Debug.LogError("Cannot GetBones for unknown mesh!");
		return null;
	}

	public void UpdateMeshes()
	{
		foreach (var meshID in this.meshes.Keys)
		{
			this.UpdateMesh(meshID);
		}
	}

	public void UpdateMesh(UUID primId)
	{
		if (this.unityMeshHolders.TryGetValue(primId, out var meshHolder))
		{
			foreach (SkinnedMeshRenderer skmr in meshHolder.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (skmr == null)
				{
					Debug.LogError($"Cannot update prim mesh {primId} because it does not have a SkinnedMeshRenderer!");
					continue;
				}

				var mesh = skmr.sharedMesh;

				if (mesh == null)
				{
					Debug.LogError($"Cannot update prim mesh {primId} because it does not have a mesh!");
					continue;
				}
				mesh.bindposes = this.GetBindposes(primId);
				skmr.bones = this.GetBones(primId);
				//skmr.rootBone = this.skeleton.boneTransforms[0];
				skmr.rootBone = this.transform;
			}
		}
		else
		{
			Debug.LogError($"Cannot update prim mesh {primId} because it is not known to this skeleton!");
		}
	}

	public void AddAnimation(UUID animId)
	{
		if (HasClip(animId))
		{
			if (animation.GetClip(animId.ToString()) == null)
			{
				var clip = cachedClips[animId];
				animation.AddClip(clip, animId.ToString());
			}
		}
		else
		{
			Debug.LogWarning($"Cannot add animation {animId} because it is not yet cached!");
		}
		// Debug.LogError("Playing Animation : " + animId.ToString());
		animation.CrossFade(animId.ToString(),0.5f);
	}


	// Here we simplyu add the animation to the animatiopn component and let it keep track
	// of all the different clips
	public void AddAnimation(DecodedAnimation decodedAnimation)
	{
		var id = decodedAnimation.animationId.ToString();
		// do not decode or add if it already exists
		if (animation.GetClip(id) != null)
		{
			return;
		}

		var clip = decodedAnimation.ToUnityAnimationClip(skeleton.boneTransforms[0], skeleton.bones);

		if (clip == null)
		{
			Debug.LogError($"Failed to Get unity clip from decoded animation data animID: {decodedAnimation.animationId}");
			return;
		}

		if (!HasClip(decodedAnimation.animationId))
		{
			cachedClips.Add(decodedAnimation.animationId, clip);
		}

		animation.AddClip(clip, id);

		//Debug.LogError("Playing Animation : " + id);
		animation.CrossFade(id, 0.5f);
	}

}
