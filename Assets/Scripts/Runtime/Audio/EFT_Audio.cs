using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EFT_Audio : MonoBehaviour, IEffector
{
    [Header("IEffector Settings")]
    [SerializeField] private AudioSource sfxSrc;
    [SerializeField] private AudioClip sfxClip;
    [SerializeField] private bool IsLooping;

    public void IEffectorExecute()
    {
        AudioController.Instance.Play3D(sfxSrc, sfxClip, IsLooping);
    }

    public void IEffectorExit()
    {
    }
}
