using OpenMetaverse;
using OpenMetaverse.Rendering;
using System.Collections.Generic;

namespace CrystalFrost.Assets.Mesh
{
    /// <summary>
    /// Represents a decoded mesh, including its raw mesh data and skinning information.
    /// </summary>
    public class DecodedMesh
    {
        /// <summary>
        /// A list of raw mesh data for each submesh.
        /// </summary>
        public List<RawMeshData> meshData = new();
		/// <summary>
		/// A flag indicating whether the mesh is skinned.
		/// </summary>
		public bool isSkinned = false;
		/// <summary>
		/// The bind shape matrix of the mesh.
		/// </summary>
		public UnityEngine.Matrix4x4 bindShapeMatrix = UnityEngine.Matrix4x4.identity;
		/// <summary>
		/// The pelvis offset matrix of the mesh.
		/// </summary>
		public UnityEngine.Matrix4x4 pelvisOffsetMatrix = UnityEngine.Matrix4x4.identity;
		/// <summary>
		/// An array of joints in the mesh.
		/// </summary>
		public JointInfo[] joints = null;
		/// <summary>
		/// The asset ID of the mesh.
		/// </summary>
		public UUID assetId;
    }
}
