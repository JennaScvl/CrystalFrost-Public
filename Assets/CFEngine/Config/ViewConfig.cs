
namespace CrystalFrost.Config
{
    /// <summary>
    /// Configuration that effects the computation of the view
    /// </summary>
    public class ViewConfig
    {
        public const string subsectionName = "View";

        // =============== Frustum Culling ===============
        public bool FrustumCulling { get; set; } = true;
        /// Default sphere around object's center to determine if it is in view
        public float DefaultSphereRadius { get; set; } = 40f;
        public float NonMeshSculptSphereRadius { get; set; } = 20f;
        // Milliseconds between checks for new objects to add to the view
        public int NewObjectPollMS { get; set; } = 100;
    }
}
