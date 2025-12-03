using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkillUI : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI minRoll_TMP;
    [SerializeField] private TextMeshProUGUI maxRoll_TMP;
    [SerializeField] private TextMeshProUGUI basePower_TMP;
    [SerializeField] private TextMeshProUGUI incrementPower_TMP;
    [SerializeField] private TextMeshProUGUI multiplier_TMP;
    [SerializeField] private TextMeshProUGUI skillName_TMP;
    [SerializeField] private Image skill_icon;
    [SerializeField] private Image type_icon;
    [SerializeField] private RectTransform coinGroup;
    [SerializeField] private List<Image> coins;

    [Header("Types (UI)")]
    [SerializeField] private Sprite slash_icon;
    [SerializeField] private Sprite blunt_icon;
    [SerializeField] private Sprite pierce_icon;
    [SerializeField] private Sprite magic_icon;

    [Header("Prefab")]
    [SerializeField] private GameObject coinPrefab;

    public void ToggleUI(bool show) => canvas.enabled = show;
    public void SetupUI(ActionSlot action)
    {
        SkillData skill = action.skillData;
        if (skill == null)
        {
            canvas.enabled = false;
            return;
        }

        for (int i = coins.Count - 1; i >= 0; i--)
            Destroy(coins[i].gameObject);

        coins.Clear();

        minRoll_TMP.color = Color.white;
        maxRoll_TMP.color = Color.white;
        basePower_TMP.text = string.Format("{0}", skill.baseCoinPower);
        incrementPower_TMP.text = string.Format("+{0}", skill.incrementCoinPower);
        skillName_TMP.text = skill.Name;
        skill_icon.sprite = skill.icon;

        switch (skill.resistance)
        {
            case SkillData.RESISTANCE_TYPE.SLASH:
                type_icon.sprite = slash_icon;
                break;

            case SkillData.RESISTANCE_TYPE.BLUNT:
                type_icon.sprite = blunt_icon;
                break;

            case SkillData.RESISTANCE_TYPE.PIERCE:
                type_icon.sprite = pierce_icon;
                break;

            case SkillData.RESISTANCE_TYPE.MAGIC:
                type_icon.sprite = magic_icon;
                break;
        }

        for (int i = 0; i < skill.coins; i++)
        {
            GameObject coinGO = Instantiate(coinPrefab, coinGroup);
            if (coinGO.TryGetComponent(out Image coinImage)) coins.Add(coinImage);
        }

        canvas.enabled = true;
    }
    public void SetMultiplier(float multiplier)
    {
        if (multiplier < 0)
        {
            multiplier_TMP.color = Color.white;
            multiplier_TMP.text = string.Format("?%");
        }

        int iMultiplier = Mathf.RoundToInt(multiplier * 100f);
        multiplier_TMP.text = string.Format("+{0}%", iMultiplier);

        if (multiplier > 1f) multiplier_TMP.color = Color.yellow;
        else if (multiplier < 1f) multiplier_TMP.color = Color.red;
        else multiplier_TMP.color = Color.white;
    }
    public void SetMin(Color color, int min)
    {
        minRoll_TMP.color = color;
        minRoll_TMP.text = string.Format("{0}", min);
    }
    public void SetMax(Color color, int max)
    {
        maxRoll_TMP.color = color;
        maxRoll_TMP.text = string.Format("{0}", max);
    }
}