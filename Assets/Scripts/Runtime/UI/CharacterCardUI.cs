using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCardUI : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private CharacterData characterData;

    [Header("User Interface (UI)")]
    [SerializeField] private Image portrait_Image;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI level_TMP;
    [SerializeField] private TextMeshProUGUI hpValue_TMP;
    [SerializeField] private Slider hpSlider;

    [Header("Inspection (UI)")]
    [SerializeField] private GameObject inspectUIGroup;
    public TextMeshProUGUI position_TMP;

    [Header("Dynamic Interaction")]
    public Button inspect_Button;
    public Button loadout_Button;

    public CharacterData GetData()
    {
        return characterData;
    }
    public void SetData(CharacterData data) => characterData = data;


    public void ToggleMode(bool IsTeam)
    {
        // Toggle Between Member and Team Mode
        inspect_Button.gameObject.SetActive(!IsTeam);
        loadout_Button.gameObject.SetActive(IsTeam);
    }

    public void ToggleInspectUI(bool state)
    {
        inspectUIGroup.SetActive(state);
    }
    public void UpdateLoadoutUI()
    {
        if (characterData == null) return;

        portrait_Image.sprite = characterData.portrait;
        name_TMP.text = characterData.Name;
        level_TMP.text = string.Format("Lv {0}", characterData.Level);
        hpValue_TMP.text = string.Format("{0}", characterData.HP);

        hpSlider.minValue = 0;
        hpSlider.maxValue = characterData.MaxHP;
        hpSlider.value = characterData.HP;

        if (characterData.position < 0) position_TMP.text = "-";
        else position_TMP.text = string.Format("{0}", characterData.position + 1);
    }
}
