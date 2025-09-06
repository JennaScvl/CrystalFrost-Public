using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
    /// <summary>
    /// contains basic mesh data:
    /// Arrays of verticies, uvs, normals, and incdices
    /// </summary>
    public class RawMeshData
    {
        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        public Vector3[] vertices;
        /// <summary>
        /// The texture coordinates of the mesh.
        /// </summary>
        public Vector2[] uvs;
        /// <summary>
        /// The normals of the mesh.
        /// </summary>
        public Vector3[] normals;
        /// <summary>
        /// The triangle indices of the mesh.
        /// </summary>
        public ushort[] indices;
		/// <summary>
		/// The bone weights of the mesh.
		/// </summary>
		public BoneWeight[] boneWeights;
		/// <summary>
		/// A flag indicating whether the mesh is skinned.
		/// </summary>
		public bool isSkinned = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawMeshData"/> class.
        /// </summary>
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="uvs">The texture coordinates of the mesh.</param>
        /// <param name="normals">The normals of the mesh.</param>
        /// <param name="indices">The triangle indices of the mesh.</param>
        public RawMeshData(Vector3[] vertices, Vector2[] uvs, Vector3[] normals, ushort[] indices)
        {
            this.vertices = vertices;
            this.uvs = uvs;
            this.normals = normals;
            this.indices = indices;
        }
    }
}