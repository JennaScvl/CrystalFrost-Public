using CrystalFrost.Extensions;
using OpenMetaverse;
using System.Collections.Generic;

namespace CrystalFrost.WorldState
{
	/// <summary>
	/// Represents an object the simulator has told us about.
	/// Could be a classic Prim, Sculpt, or Mesh
	/// </summary>
	public class SimObject
	{
		/// <summary>
		/// Indicates if the object is attached to an avatar.
		/// </summary>
		public bool IsAttachment = false;

		/// <summary>
		/// The local ID of the object.
		/// uint is faster to compare than a UUID, so we use it for a key instead in most places
		/// </summary>
		public uint LocalID;

		/// <summary>
		/// Establishes a Parent/Child relationship to another WorldObject.
		/// </summary>
		public uint ParentID;

		/// <summary>
		/// a reference to the parent object.
		/// </summary>
		public SimObject Parent;

		/// <summary>
		/// where the object is attached.
		/// </summary>
		public AttachmentPoint AttachmentPoint;

		/// <summary>
		/// The object's UUID
		/// </summary>
		public UUID UUID;

		/// <summary>
		/// The object's position according to the simulator
		/// </summary>
		public UnityEngine.Vector3 SimPosition;

		/// <summary>
		/// The object's rotation according to the simulator
		/// </summary>
		public UnityEngine.Quaternion SimRotation;

		/// <summary>
		/// What direction is the object moving according to the simulator
		/// </summary>
		public UnityEngine.Vector3 SimVelocity;

		/// <summary>
		/// What Spin/Omega does the object have according to the simulator.
		/// </summary>
		public UnityEngine.Vector3 SimAngularVelocity;

		/// <summary>
		/// Linked children
		/// </summary>
		public List<SimObject> Children = new();

		/// <summary>
		/// The region the object is in.
		/// </summary>
		public Region Region;

		/// <summary>
		/// The scale of the object.
		/// </summary>
		public UnityEngine.Vector3 Scale;

		/// <summary>
		/// What type of prim is it, Mesh, Sculpt, Classic Prim, etc.
		/// </summary>
		public PrimType PrimType = PrimType.Unknown;
		
		/// <summary>
		/// A flag indicating whether this object emits light.
		/// </summary>
		public bool IsLight = false;

		/// <summary>
		/// The radius of the light emitted by this object.
		/// </summary>
		public float LightRadius = 0;

		/// <summary>
		/// The color of the light emitted by this object.
		/// </summary>
		public UnityEngine.Color LightColor = UnityEngine.Color.black;

		/// <summary>
		/// The intensity of the light emitted by this object.
		/// </summary>
		public float LightIntensity = 0;

		/// <summary>
		/// The particle system associated with this object.
		/// </summary>
		public Primitive.ParticleSystem ParticleSystem;

	}

	/// <summary>
	/// Contains data about a single face on a world object.
	/// </summary>
	public class SimObjectFace
	{
		/// <summary>
		/// What object does this face belong to?
		/// </summary>
		public SimObject Parent;

		/// <summary>
		/// What texture is this face using.
		/// </summary>
		public UUID TextureID;

		/// <summary>
		/// Which Face / Side
		/// </summary>
		public uint Index;

		/// <summary>
		/// What color is the face tinted?
		/// </summary>
		public UnityEngine.Color Color;

		/// <summary>
		/// How much is the face glowing?
		/// </summary>
		public float Glow;

		/// <summary>
		/// Is the face Full Bright?
		/// </summary>
		public bool Fullbright;
	}

	public static class SimObjectExtensions
	{
		public static bool IsHud(this SimObject worldObject)
		{
			return
				worldObject != null &&
				worldObject.IsAttachment &&
				worldObject.AttachmentPoint.IsHudAttachmentPoint();
		}

		public static bool ParentIsHud(this SimObject worldObject)
		{
			return worldObject.IsHud() ||
				worldObject.Parent.IsHud();
		}

		public static bool IsOrphan(this SimObject worldObject)
		{
			return worldObject.ParentID != 0 && worldObject.Parent is null;
		}
	}
}
