using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipInspectorUI : MonoBehaviour
{
    [Header("Data Reference")]
    public EquipmentData.TYPE slot_type;
    [SerializeField, CE_ReadOnly] private EquipmentData equipmentData;

    [Header("User Interface (UI)")]
    [SerializeField] private GameObject highlight_border;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI stat_TMP;
    public Button inspect_button;

    public EquipmentData GetData()
    {
        return equipmentData;
    }
    public void SetData(EquipmentData data) => equipmentData = data;


    public void UpdateUI()
    {
        if (equipmentData == null)
        {
            name_TMP.text = "None";
            stat_TMP.text = "";
            return;
        }

        if (equipmentData.icon != null)
            icon.sprite = equipmentData.icon;

        name_TMP.text = string.Format("{0}", equipmentData.Name);
        switch (equipmentData.type)
        {
            case EquipmentData.TYPE.WEAPON:
                stat_TMP.text = string.Format("Attack: {0}", equipmentData.statPoint);
                break;

            case EquipmentData.TYPE.ARMOUR:
                stat_TMP.text = string.Format("Defense: {0}", equipmentData.statPoint);
                break;

            case EquipmentData.TYPE.ACCESSORY:
                stat_TMP.text = "";
                break;
        }
    }
    public void ToggleHighlightUI()
    {
        highlight_border.SetActive(!highlight_border.activeSelf);
    }
}
