using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrystalFrost.UnityRendering
{
	public interface IUnityRenderManager
	{
		public IAllSceneObjects SceneObjects { get; }
	}

	public class UnityRenderManager : IUnityRenderManager
	{
		public IAllSceneObjects SceneObjects { get; private set; }

		public UnityRenderManager(IAllSceneObjects allSceneObjects)
		{
			SceneObjects = allSceneObjects;
		}
	}
}
