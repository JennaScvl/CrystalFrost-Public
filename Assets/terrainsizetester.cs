using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainsizetester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		Terrain terrain = GetComponent<Terrain>();
		Debug.Log($"Terrain Size {terrain.terrainData.size}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
