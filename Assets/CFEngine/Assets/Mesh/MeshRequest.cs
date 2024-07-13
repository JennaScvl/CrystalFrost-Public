using OpenMetaverse;
using OpenMetaverse.Assets;
using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
    public class MeshRequest
    {
        /// <summary>
        /// The Primitive to whom this request belongs.
        /// </summary>
        public Primitive Primitive { get; set; }

        /// <summary>
        /// The GameObject that is related to the Primitive.
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// The MeshHolder for the Prim.
        /// </summary>
        public GameObject MeshHolder { get; set; }

        /// <summary>
        /// The UUID of the Mesh
        /// </summary>
        public UUID UUID { get; set; }

        /// <summary>
        /// The mesh data that was decoded for use in Unity.
        /// </summary>
        public DecodedMesh DecodedMesh {get; set; }

        /// <summary>
        /// The mesh asset that the grid client downloaded.
        /// </summary>
        public AssetMesh AssetMesh { get; set; }
    }
}
