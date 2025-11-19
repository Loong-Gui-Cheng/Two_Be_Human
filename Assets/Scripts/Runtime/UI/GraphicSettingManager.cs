using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicSettingManager : MonoBehaviour, ISaveable
{
    [Header("FPS Limit")]
    [SerializeField, CE_ReadOnly] private int currentFPSMode;
    [SerializeField] private TextMeshProUGUI FPSLimit_TMP;

    [Header("Full Screen")]
    [SerializeField, CE_ReadOnly] private bool IsFullScreen;
    [SerializeField] private Button fullScreenToggle;

    [SerializeField] private Color fsColorOn;
    [SerializeField] private Color fsColorOff;
    [SerializeField] private TextMeshProUGUI fullScreenOn_TMP;
    [SerializeField] private TextMeshProUGUI fullScreenOff_TMP;
    [SerializeField] private Image fullScreenState_BG;

    private bool IsAnimating;

    public void Load()
    {
        throw new System.NotImplementedException();
    }

    public void Save()
    {
        throw new System.NotImplementedException();
    }
    public void ChangeFPSLimit(int i = 0)
    {
        QualitySettings.vSyncCount = 0;
        currentFPSMode += i;

        if (currentFPSMode >= 3) currentFPSMode = 0;
        else if (currentFPSMode < 0) currentFPSMode = 2;

        switch (currentFPSMode)
        {
            case 0:
                Application.targetFrameRate = 60;
                break;

            case 1:
                Application.targetFrameRate = 120;
                break;

            case 2:
                Application.targetFrameRate = 165;
                break;

            default:
                Application.targetFrameRate = 60;
                break;
        }

        FPSLimit_TMP.text = string.Format("{0}", Application.targetFrameRate);
    }
    public void ToggleFullScreen()
    {
        if (IsAnimating) return;

        IsFullScreen = !IsFullScreen;
        StartCoroutine(DOTweenFullScreen());
    }

    private IEnumerator DOTweenFullScreen()
    {
        if (IsAnimating) yield break;

        fullScreenToggle.interactable = false;
        IsAnimating = true;

        float duration = 0.5f;

        if (IsFullScreen)
        {
            fullScreenOn_TMP.DOColor(fsColorOn, duration);
            fullScreenOff_TMP.DOColor(fsColorOff, duration);
            fullScreenState_BG.rectTransform.DOLocalMoveX(-125f, duration);
        }
        else
        {
            fullScreenOn_TMP.DOColor(fsColorOff, duration);
            fullScreenOff_TMP.DOColor(fsColorOn, duration);
            fullScreenState_BG.rectTransform.DOLocalMoveX(125f, duration);
        }

        yield return new WaitForSeconds(duration);

        Screen.fullScreen = IsFullScreen;
        fullScreenToggle.interactable = true;
        IsAnimating = false;
        yield break;
    }
}
