using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public interface ISaveable
{
    // Save and Load Functions
    void Save();
    void Load();
}


[System.Serializable]
public class AudioSettingsConfig
{
    public float fmasterVolume;
    public float fmasterMusic;
    public float fmasterSoundSFX;
    public float fmasterAmbience;

    public void Awake()
    {
        fmasterVolume = 0f;
        fmasterMusic = 0f;
        fmasterSoundSFX = 0f;
        fmasterAmbience = 0f;
    }
    public AudioSettingsConfig(AudioSettingsConfig saveable)
    {
        fmasterVolume = saveable.fmasterVolume;
        fmasterMusic = saveable.fmasterMusic;
        fmasterSoundSFX = saveable.fmasterSoundSFX;
        fmasterAmbience = saveable.fmasterAmbience;
    }
}

public class AudioSettingManager : MonoBehaviour, ISaveable
{
    [Header("Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [HideInInspector] public AudioSettingsConfig config;

    #region Class AudioSlider
    [System.Serializable]
    public class AudioSlider
    {
        public Slider slider;
        public TextMeshProUGUI indicator;
        public void UpdateUI()
        {
            if (slider.value == slider.minValue)
            {
                indicator.text = "0%";
                return;
            }
            if (slider.value == slider.maxValue)
            {
                indicator.text = "100%";
                return;
            }

            float percent = ((slider.value - slider.minValue) / (slider.maxValue - slider.minValue)) * 100f;
            string strSound = percent.ToString("#.#");
            indicator.text = string.Format("{0}%", strSound);
        }

    }
    #endregion
    [Header("Audio Sliders")]
    [SerializeField] private AudioSlider masterVolume;
    [SerializeField] private AudioSlider masterBGM;
    [SerializeField] private AudioSlider masterSFX;
    [SerializeField] private AudioSlider masterAmbience;

    private void Start()
    {
        Load();
    }

    public void SetMasterVolume()
    {
        audioMixer.SetFloat("Master", masterVolume.slider.value);
        masterVolume.UpdateUI();
    }

    public void SetMusicVolume()
    {
        audioMixer.SetFloat("BGM", masterBGM.slider.value);
        masterBGM.UpdateUI();
    }

    public void SetSoundSFXVolume()
    {
        audioMixer.SetFloat("SFX", masterSFX.slider.value);
        masterSFX.UpdateUI();
    }

    public void SetAmbienceVolume()
    {
        audioMixer.SetFloat("Ambience", masterAmbience.slider.value);
        masterAmbience.UpdateUI();
    }

    public void Save()
    {
        config.fmasterVolume = masterVolume.slider.value;
        config.fmasterMusic = masterBGM.slider.value;
        config.fmasterSoundSFX = masterSFX.slider.value;
        config.fmasterAmbience = masterAmbience.slider.value;

        AudioSaveSystem.SaveAudioSettings(config);
    }

    public void Load()
    {
        AudioSettingsConfig data = AudioSaveSystem.LoadAudioSettings();

        if (data != null)
        {
            masterVolume.slider.value = data.fmasterVolume;
            masterBGM.slider.value = data.fmasterMusic;
            masterSFX.slider.value = data.fmasterSoundSFX;
            masterAmbience.slider.value = data.fmasterAmbience;
        }
        else
        {
            masterVolume.slider.value = config.fmasterVolume;
            masterBGM.slider.value = config.fmasterMusic;
            masterSFX.slider.value = config.fmasterSoundSFX;
            masterAmbience.slider.value = config.fmasterAmbience;
        }

        masterVolume.UpdateUI();
        masterBGM.UpdateUI();
        masterSFX.UpdateUI();
        masterAmbience.UpdateUI();
    }
}
