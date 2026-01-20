using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Data Reference")]
    public SkillData data;
    public int order;

    [Header("User Interface (UI)")]
    public Image frame;
    public Image icon;
    public Button button;

    public void SetupSkill(SkillData data, int order)
    {
        this.data = data;
        this.order = order;
        icon.sprite = data.icon;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data == null) return;

        transform.DOComplete();
        transform.DOScale(1.1f, 0.2f);

        Tooltip_Skill.Instance.ShowTooltip(data, order);
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.ACTION_HOVER);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (data == null) return;

        transform.DOComplete();
        transform.DOScale(1f, 0.2f);

        Tooltip_Skill.Instance.HideTooltip();
    }
}