using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePlate : MonoBehaviour
{
	// Start is called before the first frame update
	public float fixedSize = 16f;
    void Start()
    {
        
    }

	// Update is called once per frame
	void Update()
	{
		var distance = (Camera.main.transform.position - transform.position).magnitude;
		var size = distance * fixedSize * Camera.main.fieldOfView;
		transform.localScale = Vector3.one * size;
		transform.forward = transform.position - Camera.main.transform.position;

	}
}
