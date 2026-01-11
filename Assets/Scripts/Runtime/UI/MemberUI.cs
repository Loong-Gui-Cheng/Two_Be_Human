using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemberUI : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private CharacterData characterData;

    [Header("User Interface (UI)")]
    [SerializeField] private Image portrait_Image;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI level_TMP;
    [SerializeField] private TextMeshProUGUI speed_TMP;
    [SerializeField] private TextMeshProUGUI hpValue_TMP;
    [SerializeField] private Slider hpSlider;

    public CharacterData GetData()
    {
       return characterData;
    }
    public void SetData(CharacterData input) => characterData = input;

    public void UpdateUI()
    {
        portrait_Image.sprite = characterData.portrait;
        name_TMP.text = characterData.Name;
        level_TMP.text = string.Format("LVL {0}", characterData.Level);
        speed_TMP.text = string.Format("{0}-{1}", characterData.minSPD, characterData.maxSPD + 1);
        hpValue_TMP.text = string.Format("{0}", Mathf.RoundToInt(characterData.HP));

        hpSlider.value = Mathf.RoundToInt(characterData.HP);
        hpSlider.minValue = 0;
        hpSlider.maxValue = Mathf.RoundToInt(characterData.MaxHP);
    }
}
