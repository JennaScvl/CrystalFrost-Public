
namespace CrystalFrost.Config
{
    /// <summary>
    /// Configuration that effects the computation of the view
    /// </summary>
    public class ViewConfig
    {
        public const string subsectionName = "View";

        // =============== Frustum Culling ===============
        /// <summary>
        /// Gets or sets a value indicating whether frustum culling is enabled.
        /// </summary>
        public bool FrustumCulling { get; set; } = true;
        /// Default sphere around object's center to determine if it is in view
        public float DefaultSphereRadius { get; set; } = 40f;
        /// <summary>
        /// Gets or sets the sphere radius for non-mesh sculpts to determine if they are in view.
        /// </summary>
        public float NonMeshSculptSphereRadius { get; set; } = 20f;
        /// <summary>
        /// Gets or sets the time in milliseconds between checks for new objects to add to the view.
        /// </summary>
        public int NewObjectPollMS { get; set; } = 100;
    }
}
