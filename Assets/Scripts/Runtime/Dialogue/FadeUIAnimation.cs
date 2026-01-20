using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeUIAnimation : MonoBehaviour
{
    [Header("IDialogueBind Settings")]
    [SerializeField] private List<Image> m_Graphics;
    [SerializeField] private List<TextMeshProUGUI> m_Texts;

    [Header("Fade Amount")]
    [Range(0f, 1f), SerializeField] private float InFade;
    [Range(0f, 1f), SerializeField] private float OutFade = 0f;

    [Header("Duration")]
    [Range(0f, 5f), SerializeField] private float InDuration;
    [Range(0f, 5f), SerializeField] private float OutDuration;

    [Header("Intervals")]
    [Range(0f, 5f), SerializeField] private float delay;
    [Range(1, 10), SerializeField] private int maxIntervals;

    private int interval;
    private bool IsAnimating;

    public void StartFade()
    {
        if (IsAnimating) return;
        StartCoroutine(FadeRoutine());
    }
    public void EndFade()
    {
        if (IsAnimating) IsAnimating = false;
        else return;

        for (int i = 0; i < m_Graphics.Count; i++)
            m_Graphics[i].DOFade(0f, 0.01f);

        for (int i = 0; i < m_Texts.Count; i++)
            m_Texts[i].DOFade(0f, 0.01f);

        StopAllCoroutines();
    }

    private IEnumerator FadeRoutine()
    {
        IsAnimating = true;

        for (int i = 0; i < m_Graphics.Count; i++)
            m_Graphics[i].DOFade(InFade, InDuration);

        for (int i = 0; i < m_Texts.Count; i++)
            m_Texts[i].DOFade(InFade, InDuration);

        yield return new WaitForSeconds(delay);

        for (int i = 0; i < m_Graphics.Count; i++)
            m_Graphics[i].DOFade(OutFade, OutDuration);

        for (int i = 0; i < m_Texts.Count; i++)
            m_Texts[i].DOFade(OutFade, OutDuration);

        yield return new WaitForSeconds(OutDuration);

        interval++;

        if (interval >= maxIntervals)
        {
            interval = 0;
            IsAnimating = false;
            StopAllCoroutines();
            yield break;
        }


        StartCoroutine(FadeRoutine());
        yield break;
    }
}
