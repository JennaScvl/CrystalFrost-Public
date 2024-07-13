using OpenMetaverse;
using OpenMetaverse.Rendering;
using System.Collections.Generic;

namespace CrystalFrost.Assets.Mesh
{
    public class DecodedMesh
    {
        public List<RawMeshData> meshData = new();
		public bool isSkinned = false;
		public UnityEngine.Matrix4x4 bindShapeMatrix = UnityEngine.Matrix4x4.identity;
		public UnityEngine.Matrix4x4 pelvisOffsetMatrix = UnityEngine.Matrix4x4.identity;
		public JointInfo[] joints = null;
		public UUID assetId;
    }
}
