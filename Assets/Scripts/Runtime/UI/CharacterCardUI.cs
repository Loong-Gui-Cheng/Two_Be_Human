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

    [Header("Inspection (UI)")]
    [SerializeField] private GameObject inspectUIGroup;

    [Header("Loadout (UI)")]
    [SerializeField] private GameObject loadoutUIGroup;
    [SerializeField] private Image positionBG_Image;
    [SerializeField] private TextMeshProUGUI position_TMP;

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
        level_TMP.text = string.Format("LVL: {0}", characterData.Level);

        loadoutUIGroup.SetActive(false);
    }
    public void DisplayLoadoutUI(int position)
    {
        loadoutUIGroup.SetActive(true);
        position_TMP.text = string.Format("{0}", position + 1);
    }
}
