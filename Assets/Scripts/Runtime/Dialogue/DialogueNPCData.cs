using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DIALOGUE_NPC_DATA", menuName = "Custom/Dialogues/NPC")]
public class DialogueNPCData : ScriptableObject
{
    #region
    // Voice-lines Organization
    [System.Serializable]
    private class VoiceClips
    {
        public string header;
        public List<AudioClip> clips;
    }
    #endregion

    public enum DIALOGUE_NPC_ID
    {
        EVA = 0, 
        ENVIRONMENT
    }

    [Header("Settings")]
    public DIALOGUE_NPC_ID ID; 
    public string alias;
    public Color aliasColor = Color.black;
    public Sprite avatar;

    [Header("Voicelines (SFX) [Organization]")]
    [SerializeField] private List<VoiceClips> voicelines;

    [Header("Animal-Crossing Style Voice (SFX)")]
    public List<AudioClip> voices;
    public List<AudioClip> punctuations;
}