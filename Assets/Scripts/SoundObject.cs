using OpenMetaverse.ImportExport.Collada14;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObject : MonoBehaviour
{
    // Start is called before the first frame update
    public List<AudioClip> clips = new List<AudioClip>();
    int index = 0;
    public AudioSource aud;
    public bool started = false;
    void Awake()
    {
        aud = gameObject.AddComponent<AudioSource>();
        aud.playOnAwake = false;
        aud.spatialize = true;
        aud.spatialBlend = 1f;
    }

    public void Play()
    {
        if (!aud.isPlaying)
        {
            aud.clip = clips[0];
            aud.Play();
            started = true;
        }
    }
	// Update is called once per frame
	void Update()
    {
        if (!aud.isPlaying && started)
        {
            index++;
            if (clips.Count < index)
            {
                aud.clip = clips[index];
                aud.Play();
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
