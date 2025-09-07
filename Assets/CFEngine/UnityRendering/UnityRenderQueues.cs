using CrystalFrost.Lib;
using CrystalFrost.WorldState;

namespace CrystalFrost.UnityRendering
{
	/// <summary>
	/// Defines a queue for Sim Objects that the state manager knows about,
	/// and has decided that are ready for the render thread to initialize.
	/// </summary>
	public interface INewSimObjectQueue : IConcurrentQueue<SimObject> { }

	/// <summary>
	/// Implements a thread-safe queue for new <see cref="SimObject"/> instances
	/// that are ready for processing by the render thread.
	/// </summary>
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

	/// <summary>
	/// Implements a thread-safe queue for <see cref="SceneObject"/> instances
	/// that have been set up and require renderer data to be added.
	/// </summary>
	public class SceneObjectsNeedingRenderersQueue : AbstractedConcurrentQueue<SceneObject>, ISceneObjectsNeedingRenderersQueue { }
}
