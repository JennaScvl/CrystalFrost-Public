using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CrystalFrost.UnityRendering
{
	public interface IAllSceneObjects
	{
		bool Add(SceneObject sceneObject);
		SceneObject Get(uint localID);
	}

	public class AllSceneObjects : IAllSceneObjects
	{
		private readonly ConcurrentDictionary<uint, SceneObject> _objects = new();
		private readonly ILogger<AllSceneObjects> _log;

		public AllSceneObjects(ILogger<AllSceneObjects> log)
		{
			_log = log;
		}

		public bool Add(SceneObject sceneObject)
		{
			if (_objects.TryAdd(sceneObject.LocalID, sceneObject)) return true;
			_log.FailedAddingToAllSceneObjects(sceneObject.LocalID);
			return false;
		}

		public SceneObject Get(uint localID)
		{
			if (_objects.ContainsKey(localID)) return _objects[localID];
			return default;
		}
	}
}
