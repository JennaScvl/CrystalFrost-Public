using CrystalFrost;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CFPendingMeshCounter : MonoBehaviour
{

	Text text;

	void Start()
	{
		text = GetComponent<Text>();
	}

	void Update()
	{
		text.text = $"{ClientManager.simManager.orphanedPrims.Count} orphaned prims";//\n{CFAssetManager.textureQueue.Count} pending textures";
	}
}
