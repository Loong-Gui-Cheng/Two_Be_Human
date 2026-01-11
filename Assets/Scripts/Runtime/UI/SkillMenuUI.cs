using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenuUI : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private SkillData skillData;
    [SerializeField] private GameObject coinTossPrefab;

    [Header("User Interface (UI)")]
    [SerializeField] private Image skill_Icon;
    [SerializeField] private Image resistance_Icon;
    [SerializeField] private TextMeshProUGUI skillName_TMP;
    [SerializeField] private TextMeshProUGUI baseCoinPower_TMP;
    [SerializeField] private TextMeshProUGUI incrementPower_TMP;
    [SerializeField] private RectTransform coinTossGroup;

    public SkillData GetData()
    {
        return skillData;
    }
    public void SetData(SkillData input, Sprite resistance, string skillOrder)
    {
        skillData = input;
        resistance_Icon.sprite = resistance;

        skillName_TMP.text = skillOrder;

        skill_Icon.sprite = skillData.icon;
        baseCoinPower_TMP.text = string.Format("{0}", skillData.baseCoinPower);
        incrementPower_TMP.text = string.Format("{0}", skillData.incrementCoinPower);

        for (int i = 0; i < skillData.coins; i++)
        {
            GameObject coin = Instantiate(coinTossPrefab, coinTossGroup);
        }
    }
}