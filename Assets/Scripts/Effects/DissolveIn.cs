using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveIn : MonoBehaviour
{
    // Start is called before the first frame update
    Material mat;
    public Material newMat;
    public Color color;
    public Texture2D texture;
    float value = 1f;
    void Awake()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetTexture("_MainMap", texture);
        mat.SetColor("_MainColor", color);
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        value = Mathf.Clamp(value - Time.deltaTime * 1.0f,0f,1f);
        GetComponent<Renderer>().sharedMaterial = null;
        GetComponent<Renderer>().material = mat;
        mat.SetFloat("_Dissolve", value);
        mat.SetTexture("_MainTex", texture);
        mat.SetColor("_MainColor", color);
        if (value<=0f)
        {
            gameObject.GetComponent<Renderer>().sharedMaterial = newMat;
            Destroy(this);
        }
	}
}
