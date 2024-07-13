using CrystalFrost.Lib;
using CrystalFrost.WorldState;

namespace CrystalFrost.UnityRendering
{
	/// <summary>
	/// Defines a queue for Sim Objects that the state manager knows about,
	/// and has decided that are ready for the render thread to initialize.
	/// </summary>
	public interface INewSimObjectQueue : IConcurrentQueue<SimObject> { }

	public class NewSimObjectQueue : AbstractedConcurrentQueue<SimObject>, INewSimObjectQueue { }

	/// <summary>
	/// Defines a queue for scene objects that have been setup,
	/// need to have Renderer data added to them.
	/// </summary>
	/// <remarks>
	/// Mesh Data can be a 'Mesh' asset downloaded from the grid, or
	/// a 'Sculp' asset downloaded from the grid, or if its a class 'Primitive'
	/// the vertex data can be generated from from the PrimData.
	/// </remarks>
	public interface ISceneObjectsNeedingRenderersQueue : IConcurrentQueue<SceneObject> { }

	public class SceneObjectsNeedingRenderersQueue : AbstractedConcurrentQueue<SceneObject>, ISceneObjectsNeedingRenderersQueue { }
}
