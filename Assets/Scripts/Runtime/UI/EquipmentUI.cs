using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentUI : MonoBehaviour, IPointerEnterHandler
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private EquipmentData equipmentData;

    [Header("User Interface (UI)")]
    [SerializeField] private GameObject border_img;
    [SerializeField] private Image equipment_icon;
    [SerializeField] private Image stat_change_icon;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI stat_TMP;
    [SerializeField] private TextMeshProUGUI amount_TMP;
    public Button swap_button;

    public static event System.Action<EquipmentUI> OnInspectEquipment;

    public EquipmentData GetData()
    {
        return equipmentData;
    }
    public void SetData(EquipmentData data) => equipmentData = data;


    public void UpdateUI()
    {
        if (equipmentData == null) return;
        if (equipmentData.icon != null) equipment_icon.sprite = equipmentData.icon;

        name_TMP.text = string.Format("{0}", equipmentData.Name);
        switch (equipmentData.type)
        {
            case EquipmentData.TYPE.WEAPON:
                stat_TMP.text = string.Format("{0}", equipmentData.statPoint);
                break;

            case EquipmentData.TYPE.ARMOUR:
                stat_TMP.text = string.Format("{0}", equipmentData.statPoint);
                break;

            case EquipmentData.TYPE.ACCESSORY:
                stat_TMP.text = "-";
                break;
        }
        amount_TMP.text = string.Format("x {0}", equipmentData.amount);
    }
    public void UpdateStatIcon(Sprite icon)
    {
        stat_change_icon.sprite = icon;
    }
    public void ToggleHighlightUI()
    {
        border_img.SetActive(!border_img.activeSelf);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnInspectEquipment?.Invoke(this);
    }
}