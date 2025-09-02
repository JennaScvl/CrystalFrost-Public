using OpenMetaverse;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
#endif
using UnityEngine;
using OpenMetaverse.Assets;
using System.Linq;
using UnityEngine.UIElements;
using OMVVector3 = OpenMetaverse.Vector3;
using Vector3 = UnityEngine.Vector3;
using CrystalFrost.Extensions;

public class SoundManager : MonoBehaviour
{
    // Start is called before the first frame update

    ConcurrentQueue<Asset> soundClipsQueue = new ConcurrentQueue<Asset>();
    ConcurrentDictionary<UUID, Queue<SoundTriggerEventArgs>> soundTriggerEvents = new ConcurrentDictionary<UUID, Queue<SoundTriggerEventArgs>>();
    ConcurrentDictionary<UUID, Queue<AttachedSoundEventArgs>> playSoundEvents = new ConcurrentDictionary<UUID, Queue<AttachedSoundEventArgs>>();
    ConcurrentDictionary<UUID, byte> uiSoundEvents = new ConcurrentDictionary<UUID, byte>();
    ConcurrentDictionary<UUID, AudioClip> audioClipCache = new ConcurrentDictionary<UUID, AudioClip>();
    ConcurrentQueue<PlaySoundData> triggerSoundQueue = new ConcurrentQueue<PlaySoundData>();
    ConcurrentQueue<PlaySoundData> playSoundQueue = new ConcurrentQueue<PlaySoundData>();
    class PlaySoundData
    {
        public UUID uuid;
        public Vector3 position;
        public float volume;
        public UUID parent;
        public SoundFlags flags;
        //public AudioClip clip;

        public PlaySoundData(UUID uuid, float volume, UUID parent, SoundFlags flags)
        {
            this.uuid = uuid;
            position = Vector3.zero;
            this.volume = volume;
            this.parent = parent;
            this.flags = flags;
        }
        public PlaySoundData(UUID uuid, Vector3 position, float volume)
        {
            this.uuid = uuid;
            this.position = position;
            this.volume = volume;
            parent = UUID.Zero;
            flags = SoundFlags.None;
        }
        public void Play()
        {
            if (!ClientManager.soundManager.audioClipCache.ContainsKey(uuid)) return;
            AudioClip clip = ClientManager.soundManager.audioClipCache[uuid];

            GameObject go;
            if (parent == UUID.Zero)
            {
                go = Instantiate(Resources.Load<GameObject>("SoundTrigger"));
                AudioSource aud = go.GetComponent<AudioSource>();
                go.transform.position = position;
                aud.clip = clip;
                aud.Play();
                Destroy(go, 10.1f);
                return;
            }

            if (!ClientManager.simManager.scenePrimIndexUUID.ContainsKey(parent)) return;

            if (!ClientManager.simManager.scenePrims.ContainsKey(ClientManager.simManager.scenePrimIndexUUID[parent])) return;

            go = ClientManager.simManager.scenePrims[ClientManager.simManager.scenePrimIndexUUID[parent]].obj;

            SoundObject so = go.GetComponent<SoundObject>();

            if (so == null) so = go.AddComponent<SoundObject>();

            if (flags.HasFlag(SoundFlags.Stop))
            {
                so.aud.loop = false;
                so.aud.Stop();
                Destroy(so);
                return;
            }

            if (flags.HasFlag(SoundFlags.Queue) || so.clips.Count == 0)
            {
                so.clips.Add(clip);
            }

            // Re-enable looping sounds. The Stop flag is handled above.
            if (flags.HasFlag(SoundFlags.Loop))
            {
                so.aud.loop = true;
            }

            so.aud.volume = volume;
            so.Play();
        }
    }
    public void TriggerSoundEventHandler(object sender, SoundTriggerEventArgs e)
    {
        //Debug.Log(e.ParentID.ToString());
        RequestSound(e);
    }

    public void AttachedSoundEventHandler(object sender, AttachedSoundEventArgs e)
    {
        RequestSound(e);
    }

    public void PreloadSoundEventHandler(object sender, PreloadSoundEventArgs e)
    {
        RequestSound(e);
    }

    void RequestSound(SoundTriggerEventArgs e)
    {
        if (audioClipCache.ContainsKey(e.SoundID))
        {
            triggerSoundQueue.Enqueue(new PlaySoundData(e.SoundID, e.Position.ToVector3(), e.Gain));
        }
        else
        {
            soundTriggerEvents.TryAdd(e.SoundID, new Queue<SoundTriggerEventArgs>());
            soundTriggerEvents[e.SoundID].Enqueue(e);
            ClientManager.client.Assets.RequestAsset(e.SoundID, AssetType.Sound, true, SoundDownloadCallback);
        }
    }

    void RequestSound(AttachedSoundEventArgs e)
    {
        if (audioClipCache.ContainsKey(e.SoundID))
        {
            triggerSoundQueue.Enqueue(new PlaySoundData(e.SoundID, e.Gain, e.ObjectID, e.Flags));
        }
        else
        {
            playSoundEvents.TryAdd(e.SoundID, new Queue<AttachedSoundEventArgs>());
            playSoundEvents[e.SoundID].Enqueue(e);
            ClientManager.client.Assets.RequestAsset(e.SoundID, AssetType.Sound, true, SoundDownloadCallback);
        }
    }

    void RequestSound(UUID uuid)
    {
        if (audioClipCache.ContainsKey(uuid))
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("SoundTrigger"));
            AudioSource aud = go.GetComponent<AudioSource>();
            aud.clip = audioClipCache[uuid];
            aud.spatialize = false;
            aud.spatialBlend = 0f;
            aud.Play();
            Destroy(go, 10.1f);
        }
        else
        {
            uiSoundEvents.TryAdd(uuid, 0);
            ClientManager.client.Assets.RequestAsset(uuid, AssetType.Sound, true, SoundDownloadCallback);
        }
    }

    void RequestSound(PreloadSoundEventArgs e)
    {
        if (!audioClipCache.ContainsKey(e.SoundID))
        {
            ClientManager.client.Assets.RequestAsset(e.SoundID, AssetType.Sound, true, SoundDownloadCallback);
        }
    }


    void PlaySounds()
    {
        while (triggerSoundQueue.TryDequeue(out var psd) && psd != null)
        {
            psd.Play();
        }
    }

    void DecodeSounds()
    {
        while (soundClipsQueue.TryDequeue(out var asset) && asset != default)
        {
            
            using (var vorbis = new NVorbis.VorbisReader(new System.IO.MemoryStream(asset.AssetData, false)))
            {
                //Debug.Log($"Found ogg ch={vorbis.Channels} freq={vorbis.SampleRate} samp={vorbis.TotalSamples}");
                float[] _audioBuffer = new float[vorbis.TotalSamples]; // Just dump everything
                int read = vorbis.ReadSamples(_audioBuffer, 0, (int)vorbis.TotalSamples);
                AudioClip audioClip = AudioClip.Create(asset.AssetID.ToString(), (int)(vorbis.TotalSamples / vorbis.Channels), vorbis.Channels, vorbis.SampleRate, false);
                audioClip.SetData(_audioBuffer, 0);
                audioClipCache.TryAdd(asset.AssetID, audioClip);
            }
            if (soundTriggerEvents.ContainsKey(asset.AssetID))
            {
                while (soundTriggerEvents[asset.AssetID].TryDequeue(out var e) && e != default)
                {
                    triggerSoundQueue.Enqueue(new PlaySoundData(asset.AssetID, e.Position.ToVector3(), e.Gain));
                }
                soundTriggerEvents.TryRemove(asset.AssetID, out _);
            }
            else
            if (playSoundEvents.ContainsKey(asset.AssetID))
            {
                while (playSoundEvents[asset.AssetID].TryDequeue(out var e) && e != default)
                {
                    triggerSoundQueue.Enqueue(new PlaySoundData(asset.AssetID, e.Gain, e.ObjectID, e.Flags));
                }
                playSoundEvents.TryRemove(asset.AssetID, out _);
            }
            else
            {
                if (uiSoundEvents.ContainsKey(asset.AssetID))
                {
                    GameObject go = Instantiate(Resources.Load<GameObject>("SoundTrigger"));
                    AudioSource aud = go.GetComponent<AudioSource>();
                    uiSoundEvents.TryRemove(asset.AssetID, out _);
                    aud.clip = audioClipCache[asset.AssetID];
                    aud.spatialize = false;
                    aud.spatialBlend = 0f;
                    aud.Play();
                    Destroy(go, 10.1f);
                }
            }
        }
    }

    public void SoundDownloadCallback(AssetDownload download, Asset asset)
    {
        if (download.Success)
        {
            soundClipsQueue.Enqueue(asset);
        }
    }

    private void Awake()
    {
        ClientManager.soundManager = this;
    }

    void Start()
    {
        ClientManager.client.Sound.SoundTrigger += new EventHandler<SoundTriggerEventArgs>(TriggerSoundEventHandler);
        ClientManager.client.Sound.AttachedSound += new EventHandler<AttachedSoundEventArgs>(AttachedSoundEventHandler);
        ClientManager.client.Sound.PreloadSound += new EventHandler<PreloadSoundEventArgs>(PreloadSoundEventHandler);

        triggerSoundQueue = new ConcurrentQueue<PlaySoundData>();

    }

    // Update is called once per frame
    void Update()
    {
        DecodeSounds();
        PlaySounds();
    }

    public void PlayUISound(UUID id)
    {
        //Debug.Log($"Playing Sound {id}");
        RequestSound(id);
        //SoundTriggerEventArgs e = new SoundTriggerEventArgs(null, id, UUID.Zero, UUID.Zero, UUID.Zero, 1.0f, 0, OMVVector3.Zero);
    }
}
