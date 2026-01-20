using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;

public class Tooltip_Skill : Singleton<Tooltip_Skill>
{
    [System.Serializable]
    private class CoinStatus_UI
    {
        public GameObject go;
        public TextMeshProUGUI tmp;
    }


    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private SkillData skillData;
    private Canvas canvas;

    [Header("User Interface (UI)")]
    [SerializeField] private Image skillIcon_Image;
    [SerializeField] private Image resistanceIcon_Image;
    [SerializeField] private TextMeshProUGUI skillName_TMP;
    [SerializeField] private TextMeshProUGUI basePower_TMP;
    [SerializeField] private TextMeshProUGUI incrementPower_TMP;
    [SerializeField] private TextMeshProUGUI skillOrder_TMP;
    [SerializeField] private RectTransform coinGroup;
    [SerializeField] private RectTransform skillResonanceGroup;
    [SerializeField] private List<GameObject> coins;
    [SerializeField] private List<GameObject> resonancePoints;
    [SerializeField] private List<CoinStatus_UI> coinEffects;

    [Header("Assets")]
    [SerializeField] private Sprite slashSprite;
    [SerializeField] private Sprite pierceSprite;
    [SerializeField] private Sprite bluntSprite;

    [Header("Tooltip")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private RectTransform canvasRectTransform;

    [Header("UI Camera")]
    [SerializeField] private Camera uiCam;

    private void Start()
    {
        TryGetComponent(out canvas);
    }
    private void Update()
    {
        if (!canvas.enabled) return;

        //Vector2 anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform, Input.mousePosition, null, out mousePos);

        mousePos.x = mousePos.x + (rectTransform.sizeDelta.x / 2);
        mousePos.y = mousePos.y + (rectTransform.sizeDelta.y / 2);

        if (mousePos.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            mousePos.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (mousePos.y - backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            mousePos.y = canvasRectTransform.rect.height + backgroundRectTransform.rect.height;
        }


        rectTransform.anchoredPosition = mousePos;
    }
    public void ShowTooltip(SkillData input, int order)
    {
        if (skillData == null)
        {
            skillData = input;
            SetupUI(order);
        }
        else if (!(string.Compare(skillData.Name, input.Name, System.StringComparison.Ordinal) == 0))
        {
            skillData = input;
            SetupUI(order);
        }


        canvas.enabled = true;
        transform.SetAsLastSibling();
    }
    public void HideTooltip()
    {
        canvas.enabled = false;
    }
    private void SetupUI(int order)
    {
        // Reset UI
        for (int i = 0; i < coins.Count; i++)
            coins[i].SetActive(false);

        for (int i = 0; i < resonancePoints.Count; i++)
            resonancePoints[i].SetActive(false);

        for (int i = 0; i < coinEffects.Count; i++)
            coinEffects[i].go.SetActive(false);


        // Update UI
        skillName_TMP.text = string.Format("{0}", skillData.Name);
        basePower_TMP.text = string.Format("{0}", skillData.baseCoinPower);
        incrementPower_TMP.text = string.Format("+{0}", skillData.incrementCoinPower);
        skillOrder_TMP.text = string.Format("Skill {0}", order);

        skillIcon_Image.sprite = skillData.icon;

        switch (skillData.resistance)
        {
            case SkillData.RESISTANCE_TYPE.SLASH:
                resistanceIcon_Image.sprite = slashSprite;
                break;
            case SkillData.RESISTANCE_TYPE.PIERCE:
                resistanceIcon_Image.sprite = pierceSprite;
                break;
            case SkillData.RESISTANCE_TYPE.BLUNT:
                resistanceIcon_Image.sprite = bluntSprite;
                break;
            case SkillData.RESISTANCE_TYPE.MAGIC:
                resistanceIcon_Image.sprite = slashSprite;
                break;
        }


        for (int i = 0; i < skillData.coins.Count; i++)
        {
            coins[i].gameObject.SetActive(true);
            coinEffects[i].tmp.text = skillData.coins[i].description;
            coinEffects[i].go.SetActive(true);
        }
    }
}
