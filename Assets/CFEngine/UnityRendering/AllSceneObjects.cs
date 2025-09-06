using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CrystalFrost.UnityRendering
{
	/// <summary>
	/// Defines a repository for managing all active <see cref="SceneObject"/> instances.
	/// </summary>
	public interface IAllSceneObjects
	{
		/// <summary>
		/// Adds a <see cref="SceneObject"/> to the repository.
		/// </summary>
		/// <param name="sceneObject">The scene object to add.</param>
		/// <returns>True if the object was added successfully; otherwise, false.</returns>
		bool Add(SceneObject sceneObject);

		/// <summary>
		/// Retrieves a <see cref="SceneObject"/> by its local ID.
		/// </summary>
		/// <param name="localID">The local ID of the object to retrieve.</param>
		/// <returns>The <see cref="SceneObject"/> if found; otherwise, default.</returns>
		SceneObject Get(uint localID);
	}

	/// <summary>
	/// Implements a thread-safe repository for managing all active <see cref="SceneObject"/> instances.
	/// </summary>
	public class AllSceneObjects : IAllSceneObjects
	{
		private readonly ConcurrentDictionary<uint, SceneObject> _objects = new();
		private readonly ILogger<AllSceneObjects> _log;

		/// <summary>
		/// Initializes a new instance of the <see cref="AllSceneObjects"/> class.
		/// </summary>
		/// <param name="log">The logger for recording messages.</param>
		public AllSceneObjects(ILogger<AllSceneObjects> log)
		{
			_log = log;
		}

		/// <summary>
		/// Adds a <see cref="SceneObject"/> to the repository.
		/// </summary>
		/// <param name="sceneObject">The scene object to add.</param>
		/// <returns>True if the object was added successfully; otherwise, false.</returns>
		public bool Add(SceneObject sceneObject)
		{
			if (_objects.TryAdd(sceneObject.LocalID, sceneObject)) return true;
			_log.FailedAddingToAllSceneObjects(sceneObject.LocalID);
			return false;
		}

		/// <summary>
		/// Retrieves a <see cref="SceneObject"/> by its local ID.
		/// </summary>
		/// <param name="localID">The local ID of the object to retrieve.</param>
		/// <returns>The <see cref="SceneObject"/> if found; otherwise, default.</returns>
		public SceneObject Get(uint localID)
		{
			if (_objects.ContainsKey(localID)) return _objects[localID];
			return default;
		}
	}
}
