using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalFrost.UnityRendering
{
	/// <summary>
	/// Defines the interface for a manager that handles Unity rendering operations.
	/// </summary>
	public interface IUnityRenderManager
	{
		/// <summary>
		/// Gets the repository of all scene objects.
		/// </summary>
		public IAllSceneObjects SceneObjects { get; }
	}

	/// <summary>
	/// Manages Unity rendering operations, including scene objects and their states.
	/// </summary>
	public class UnityRenderManager : IUnityRenderManager
	{
		/// <summary>
		/// Gets the repository of all scene objects.
		/// </summary>
		public IAllSceneObjects SceneObjects { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityRenderManager"/> class.
		/// </summary>
		/// <param name="allSceneObjects">The repository for all scene objects.</param>
		public UnityRenderManager(IAllSceneObjects allSceneObjects)
		{
			SceneObjects = allSceneObjects;
		}
	}
}
