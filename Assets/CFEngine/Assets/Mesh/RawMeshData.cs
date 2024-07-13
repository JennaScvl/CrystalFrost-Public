using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
    /// <summary>
    /// contains basic mesh data:
    /// Arrays of verticies, uvs, normals, and incdices
    /// </summary>
    public class RawMeshData
    {
        public Vector3[] vertices;
        public Vector2[] uvs;
        public Vector3[] normals;
        public ushort[] indices;
		public BoneWeight[] boneWeights;
		public bool isSkinned = false;

        public RawMeshData(Vector3[] vertices, Vector2[] uvs, Vector3[] normals, ushort[] indices)
        {
            this.vertices = vertices;
            this.uvs = uvs;
            this.normals = normals;
            this.indices = indices;
        }
    }
}