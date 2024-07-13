using System;
using OpenMetaverse;
using OpenMetaverse.Assets;
using UnityEngine;

namespace CrystalFrost.Assets.Animation
{
	public class AnimationRequest
	{

		/// <summary>
		/// The Primitive to whom this request belongs.
		/// </summary>
		public Primitive Primitive { get; set; }

		/// <summary>
		/// The UUID of the Animation
		/// </summary>
		public UUID UUID { get; set; }

		/// <summary>
		/// The Animation data that was decoded for use in Unity.
		/// </summary>
		public DecodedAnimation DecodedAnimation { get; set; }

		/// <summary>
		/// The Animation asset that the grid client downloaded.
		/// </summary>
		public AssetAnimation AssetAnimation { get; set; }
	}
}

