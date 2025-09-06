using CrystalFrost.WorldState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CrystalFrost.UnityRendering
{
	/// <summary>
	/// A counterpart to SimObject, but containing Data related
	/// to rendering with Unity.
	/// </summary>
	public class SceneObject
	{
		/// <summary>
		/// Relates this object to a SimObject (and to a LibreMetaverse Primitive).
		/// </summary>
		public uint LocalID;

		/// <summary>
		/// The parent GameObject that holds the hierarchy for this scene object.
		/// </summary>
		public GameObject HeirachyHolder;

		/// <summary>
		/// The primary GameObject for this scene object.
		/// </summary>
		public GameObject GameObject;

		/// <summary>
		/// The GameObject that holds the mesh for this scene object.
		/// </summary>
		public GameObject MeshHolder;

		/// <summary>
		/// The simulation object counterpart in the world state.
		/// </summary>
		public SimObject SimObject;

		/// <summary>
		/// The parent scene object in the hierarchy.
		/// </summary>
		public SceneObject Parent;

		/// <summary>
		/// A flag indicating whether this object represents water.
		/// </summary>
		public bool IsWater;

		/// <summary>
		/// An array of renderers associated with this scene object.
		/// </summary>
		public Renderer[] Renderers;

#if USE_KWS
	public WaterSystem WaterSystem;

	public GameObject WaterBox;
#endif
		/// <summary>
		/// The light component associated with this scene object.
		/// </summary>
		public GameObject Light;

	}
}
