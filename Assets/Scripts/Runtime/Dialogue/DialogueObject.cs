using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class stores runtime data for dialogue sound effects / voice overs.
It is also used as a reference to play Audio from, especially in 3D space.
It also stores the Dialogue Branch progress from one branch to another.

Class relation:
Stores Dialogue NPC Data for dialogue box display.
Stores Dialogue Data for Dialogue System to reference to.

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/
public class DialogueObject : MonoBehaviour
{
    [Header("Settings")]
    public DialogueNPCData data;
    public DialogueData dialogueBranch;
    public DialogueData defaultBranch;
    [Tooltip("Toggle to use both voice & punctuation to mimic animal crossing voices.")]
    [SerializeField] private bool useAnimalCrossingVoice;

    // Voice Source - Plays voice lines & animal pitch noise 1.
    // Punctuation Source - Plays animal pitch noise 2.
    // Effect Source - Plays sound effects such as window breaking.
    [Header("Voice (SFX)")]
    public AudioSource voiceSource;
    [ConditionalHide("useAnimalCrossingVoice", true)]
    public AudioSource punctuationSource;

    [Header("Effects (SFX)")]
    public AudioSource effectSource;

    private Animator animator;

    private void Start()
    {
        TryGetComponent(out animator);
    }

    public bool CanPlayACVoice()
    { 
        return voiceSource != null && 
            punctuationSource != null && 
            effectSource != null;
    }

    public void StopAudio()
    {
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        if (punctuationSource != null && punctuationSource.isPlaying)
            punctuationSource.Stop();

        if (effectSource != null && effectSource.isPlaying)
            effectSource.Stop();
    }

    public bool IsPlaying()
    {
        if (voiceSource != null && voiceSource.isPlaying)
            return true;

        if (punctuationSource != null && punctuationSource.isPlaying)
            return true;

        if (effectSource != null && effectSource.isPlaying)
            return true;

        return false;
    }

    // On Text Action Event
    public void SetAction(string action)
    {
        if (AudioController.Instance == null) return;
        if (DialogueSystem.Instance == null) return;

        // Empty action.
        if (action.CompareTo(string.Empty) == 0) return;


        if (action == "sound")
        {

        }
    }
    // On Text Reveal Event
    public void ReproduceSound(char c)
    {
        if (AudioController.Instance == null) return;
        if (DialogueSystem.Instance == null) return;

        // Not enough punctuations/voice clips.
        if (data.punctuations.Count <= 0 || data.voices.Count <= 0)
            return;


        if (char.IsPunctuation(c) && !punctuationSource.isPlaying)
        {
            voiceSource.Stop();
            AudioClip clip = data.punctuations[Random.Range(0, data.punctuations.Count)];
            AudioController.Instance.Play3D(punctuationSource, clip, false);
        }
        if (char.IsLetter(c) && !voiceSource.isPlaying)
        {
            punctuationSource.Stop();
            AudioClip clip = data.voices[Random.Range(0, data.voices.Count)];
            AudioController.Instance.Play3D(voiceSource, clip, false);
        }
    }
    // On Animation Event
    public void Animate(string stateName)
    {
        if (stateName == null) return;

        if (animator != null)
        {
            animator.Play(stateName);
        }
    }
}
