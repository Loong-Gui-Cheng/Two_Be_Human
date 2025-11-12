using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class is an interface to play audio & background ambience.
These include user interface sound effects, background music, 3D spatial sounds.

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/
[RequireComponent(typeof(AudioSource))]
public class AudioController : Singleton<AudioController>
{
    // This class is a singleton so that you can call its functions from anywhere and whenever you want.
    // * Recommended to store all sound effects under its own ID here for easy access.
    public enum MUSIC_ID
    {
        MAIN_MENU = 0,
        DAY,
        NIGHT,
        VICTORY,
        DEFEAT
    }
    public enum SOUND_ID
    {
        CLICK = 0,
        DIALOGUE_CLICK = 1,
        ERROR = 2,
        SUCCESS = 3,
        SUBMIT_SWEEP = 4,
        LIGHT_TOGGLE = 5,
        QUEST_LINE_ACQUIRED = 6,
        QUEST_LINE_COMPLETE = 7,
        DING = 8,
        ALERT = 9
    }

    [Header("Audio Singleton Settings")]
    [SerializeField] private bool shouldCycleBGM;
    [SerializeField] private List<AudioClip> BGMList;
    [SerializeField] private List<AudioClip> SFX_UI_List;
    [SerializeField] private GameObject m_SoundBubble;

    // Sound Pooling
    private float beforeSamples = 1f;
    private int iBGM = -1;
    private AudioSource m_BGMSrc;

    private readonly List<AudioSource> SFXSrcPool = new();
    private readonly Dictionary<int, AudioClip> SFX_UI_Map = new();

    public override void Awake()
    {
        base.Awake();
        AudioSource[] sources = GetComponents<AudioSource>();
        m_BGMSrc = sources[0];

        // If there's BGM, play it.
        if (BGMList.Count > 0)
        {
            iBGM = 0;
            Play2D(BGMList[iBGM], false, true, true);
        }

        // Add SFX Sound Source for pooling later.
        if (sources.Length > 1)
        {
            for (int i = 1; i < sources.Length; i++)
                SFXSrcPool.Add(sources[i]);
        }

        // Add to hash map for performance.
        for (int i = 0; i < SFX_UI_List.Count; i++)
        {
            if (SFX_UI_List[i] != null)
                SFX_UI_Map.Add(i, SFX_UI_List[i]);
        }
    }

    private void Update()
    {
        if (shouldCycleBGM)
        {
            // BGM is still playing 
            if (beforeSamples < m_BGMSrc.timeSamples)
            {
                beforeSamples = m_BGMSrc.timeSamples - 1f;
            }
            // BGM is trying to loop
            else if (beforeSamples >=  m_BGMSrc.timeSamples)
            {
                iBGM++;

                // Clamp value 
                if (iBGM >= BGMList.Count)
                    iBGM = 0;

                Play2D(BGMList[iBGM], false, true, true);
                beforeSamples = m_BGMSrc.timeSamples - 1f;
            }
        }
    }

    // For Switching Between BGMs
    public void PlayBGM(MUSIC_ID ID)
    {
        iBGM = (int)ID;

        if (iBGM >= BGMList.Count)
            iBGM = 0;

        Play2D(BGMList[iBGM], false, true, true);
    }

    // For 2D Sounds
    public void Play2D(AudioClip clip, bool isSFX, bool overwriteSound, bool isLooping = false)
    {
        if (clip == null) return;

        if (!isSFX)
        {
            if (m_BGMSrc.isPlaying)
                m_BGMSrc.Stop();

            m_BGMSrc.loop = true;
            m_BGMSrc.clip = clip;
            m_BGMSrc.Play();
            return;
        }


        if (SFXSrcPool.Count > 0)
        {
            for (int i = 0; i < SFXSrcPool.Count; i++)
            {
                if (!SFXSrcPool[i].isPlaying)
                {
                    SFXSrcPool[i].loop = isLooping;
                    SFXSrcPool[i].clip = clip;
                    SFXSrcPool[i].Play();
                    return;
                }
            }

            if (overwriteSound)
            {
                SFXSrcPool[0].Stop();
                SFXSrcPool[0].loop = isLooping;
                SFXSrcPool[0].clip = clip;
                SFXSrcPool[0].Play();
            }
        }
    }

    // For UI 
    public void PlayUI(AudioClip clip) => Play2D(clip, true, false, false);
    public void PlayUI(SOUND_ID id)
    {
        if (SFX_UI_Map.TryGetValue((int)id, out AudioClip clip))
            Play2D(clip, true, false, false);
    }

    // For Independent Sounds
    public void Play3D(AudioSource SFXSrc, AudioClip clip, bool isLooping = false)
    {
        if (SFXSrc == null) return;
        if (clip == null) return;

        if (SFXSrc.isPlaying)
            SFXSrc.Stop();

        SFXSrc.loop = isLooping;
        SFXSrc.clip = clip;
        SFXSrc.Play();
    }
    public void Play3D(AudioSource SFXSrc, SOUND_ID id, bool isLooping = false)
    {
        if (SFX_UI_Map.TryGetValue((int)id, out AudioClip clip))
            Play3D(SFXSrc, clip, isLooping);
    }

    // For Independent Sounds that plays back every x interval timer.
    public void Play3DInterval(AudioSource SFXSrc, AudioClip clip, ref float timer, float intervalDuration)
    {
        if (SFXSrc == null) return;
        if (clip == null) return;

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            return;
        }

        if (SFXSrc.isPlaying)
            SFXSrc.Stop();

        SFXSrc.loop = false;
        SFXSrc.clip = clip;
        SFXSrc.Play();

        timer = intervalDuration;
    }

    // For when you want to spawn an audio sfx object when something is already destroyed.
    public void Spawn3D(AudioClip clip, Vector3 position, float lifeTime)
    {
        if (m_SoundBubble == null) return;
        if (clip == null) return;

        GameObject go = Instantiate(m_SoundBubble, position, Quaternion.identity);

        if (go.TryGetComponent(out AudioSource sfxSrc))
        {
            sfxSrc.clip = clip;
            sfxSrc.Play();
        }

        Destroy(go, lifeTime);
    }
}
