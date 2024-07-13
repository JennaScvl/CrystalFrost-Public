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
		/// Relates this object to a SimObject (and to a LibreMetavers Primitive)
		/// </summary>
		public uint LocalID;

		public GameObject HeirachyHolder;

		public GameObject GameObject;

		public GameObject MeshHolder;

		public SimObject SimObject;

		public SceneObject Parent;

		public bool IsWater;

		public Renderer[] Renderers;

#if USE_KWS
	public WaterSystem WaterSystem;

	public GameObject WaterBox;
#endif
		public GameObject Light;

	}
}
