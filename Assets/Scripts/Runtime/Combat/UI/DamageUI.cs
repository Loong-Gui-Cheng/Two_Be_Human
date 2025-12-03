using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private RectTransform groupTransform;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI damage_TMP;
    [SerializeField] private TextMeshProUGUI damage_multiplier_TMP;
    [SerializeField] private TextMeshProUGUI resistance_TMP;

    [Header("Damage Types (UI)")]
    [SerializeField] private Sprite slash_icon;
    [SerializeField] private Sprite blunt_icon;
    [SerializeField] private Sprite pierce_icon;
    [SerializeField] private Sprite magic_icon;

    private const float INDICATOR_DURATION = 1f;

    private void Start() => Destroy(gameObject, INDICATOR_DURATION + 0.1f);
    private void OnDestroy() => groupTransform.DOKill(true);
    public void SetupUI(SkillData input, float damage_multiplier, float weakness_multiplier, bool IsCrit, int damage)
    {
        groupTransform.localScale = new Vector3(2f, 2f, 2f);
        groupTransform.DOScale(Vector3.zero, INDICATOR_DURATION);

        switch (input.resistance)
        {
            case SkillData.RESISTANCE_TYPE.SLASH:
                icon.sprite = slash_icon;
                break;

            case SkillData.RESISTANCE_TYPE.BLUNT:
                icon.sprite = blunt_icon;
                break;

            case SkillData.RESISTANCE_TYPE.PIERCE:
                icon.sprite = pierce_icon;
                break;

            case SkillData.RESISTANCE_TYPE.MAGIC:
                icon.sprite = magic_icon;
                break;
        }

        damage_TMP.text = string.Format("{0}", damage);
        damage_multiplier_TMP.text = string.Format("+{0}%", Mathf.RoundToInt(damage_multiplier * 100));

        if (IsCrit) resistance_TMP.text = "Critical!";
        else if (weakness_multiplier > 1) resistance_TMP.text = "Weak";
        else if (weakness_multiplier < 1) resistance_TMP.text = "Endure";
        else resistance_TMP.text = "Normal";

        if (IsCrit)
        {
            damage_TMP.color = Color.yellow;
            damage_multiplier_TMP.color = Color.yellow;
            resistance_TMP.color = Color.yellow;
        }
    }
    public float GetIndicatorDuration()
    {
        return INDICATOR_DURATION;
    }
}
