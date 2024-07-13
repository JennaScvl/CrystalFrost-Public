using CrystalFrost;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CFPendingTextureCounter : MonoBehaviour
{

	Text text;

	void Start()
	{
		text = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
    {
		text.text = $"{ClientManager.simManager.objectsToRez.Count} pending objects";//\n{CFAssetManager.textureQueue.Count} pending textures";

	}
}
