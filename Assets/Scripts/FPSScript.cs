using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CrystalFrost;

namespace FBCapture
{
	public class FPSScript : MonoBehaviour
	{
		/// <summary>
		/// Delta time
		/// </summary>
		float deltaTime = 0.0f;

		/// <summary>
		/// It will be used for printing out fps text on screen
		/// </summary>
		Text text;

		void Start()
		{
			text = GetComponent<Text>();
		}

		void Update()
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			float msec = deltaTime * 1000.0f;
			float fps = 1.0f / deltaTime;
			text.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
			//text.text += $"\n{CFAssetManager.concurrentMeshQueue.Count} pending meshes\n{CFAssetManager.textureQueue.Count} pending textures";
		}
	}

}