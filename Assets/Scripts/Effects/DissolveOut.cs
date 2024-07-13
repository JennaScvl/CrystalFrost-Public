using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveOut : MonoBehaviour
{
    // Start is called before the first frame update
    Material mat;
    float value = 0f;
    void Awake()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        value = Mathf.Clamp(value + Time.deltaTime * 1.0f,0f,1f);
        mat.SetFloat("_Dissolve", value);
        if(value==1f)
        {
            Destroy(gameObject);
        }
    }
}
