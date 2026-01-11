using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    [Header("Player Save")]
    [SerializeField] private PlayerData playerData;

    [Header("Equipment Inspection")]
    [SerializeField, CE_ReadOnly] private EquipmentData.TYPE type;
    [SerializeField] private TextMeshProUGUI equipmentType_TMP;
    [SerializeField] private TextMeshProUGUI equipmentStat_TMP;
    [SerializeField] private TextMeshProUGUI equipmentAMT_TMP;
    [SerializeField] private TextMeshProUGUI effect_TMP;
    [SerializeField] private TextMeshProUGUI description_TMP;

    [Header("Equipment Loadout")]
    [SerializeField] private GameObject equipmentUIPrefab;
    [SerializeField] private RectTransform equipmentGroup;
    [SerializeField] private RectTransform detailsGroup;
    [SerializeField] private RectTransform effectGroup;

    [Header("Asset (Stat Change)")]
    [SerializeField] private Sprite arrowDown_Icon;
    [SerializeField] private Sprite arrowUp_Icon;
    [SerializeField] private Sprite arrowNeutral_Icon;

    [Header("Member Inspector")]
    [SerializeField] private Image member_Icon;
    [SerializeField] private TextMeshProUGUI memberPosition_TMP;
    [SerializeField] private TextMeshProUGUI memberName_TMP;

    [Header("Member Held Equipment")]
    [SerializeField] private RectTransform heldDetailsGroup;
    [SerializeField] private RectTransform heldEffectsGroup;
    [SerializeField] private TextMeshProUGUI heldEquipmentType_TMP;
    [SerializeField] private TextMeshProUGUI heldEquipmentStatHeader_TMP;
    [SerializeField] private TextMeshProUGUI heldEquipmentName_TMP;
    [SerializeField] private TextMeshProUGUI heldEquipmentStat_TMP;
    [SerializeField] private TextMeshProUGUI heldEquipmentEffect_TMP;
    [SerializeField] private TextMeshProUGUI heldEquipmentDescription_TMP;
    [SerializeField] private Image heldEquipment_Icon;
    [SerializeField] private Button unequipHeld_Button;


    [Header("Tracker")]
    [SerializeField] private List<EquipmentUI> equipments;
    [SerializeField, CE_ReadOnly] private CharacterData currentCharacter;
    [SerializeField, CE_ReadOnly] private EquipmentData heldEquipment;
    [SerializeField, CE_ReadOnly] private EquipmentUI currentEquipment;

    private void OnEnable()
    {
        EquipmentInspector.OnEditEquipment += EditEquipment;
        EquipmentUI.OnInspectEquipment += InspectEquipment;
    }
    private void OnDisable()
    {
        EquipmentInspector.OnEditEquipment -= EditEquipment;
        EquipmentUI.OnInspectEquipment -= InspectEquipment;
    }


    private void Start()
    {
        OnLoad();
    }
    public void OnLoad()
    {
        unequipHeld_Button.onClick.AddListener(() => UnEquip());
    }
    public void InspectEquipment(EquipmentUI equipmentUI)
    {
        if (currentEquipment != null) currentEquipment.ToggleHighlightUI();

        currentEquipment = equipmentUI;
        currentEquipment.ToggleHighlightUI();
        EquipmentData data = currentEquipment.GetData();

        if (data == null)
        {
            detailsGroup.gameObject.SetActive(false);
            return;
        }

        detailsGroup.gameObject.SetActive(true);
        description_TMP.text = data.Description;

        if (data.effectPrefab == null)
            effectGroup.gameObject.SetActive(false);
        else
        {
            effectGroup.gameObject.SetActive(true);
            effect_TMP.text = string.Format("+ {0}", data.effectName);
        }
    }
    private void EditEquipment(CharacterCardUI characterCardUI, EquipInspectorUI equipInspectorUI)
    {
        currentCharacter = characterCardUI.GetData();
        EquipmentData selectedEquipment = equipInspectorUI.GetData();
        heldEquipment = selectedEquipment;
        type = equipInspectorUI.slot_type;

        // Update character ui
        if (currentCharacter.position < 0) memberPosition_TMP.text = "-"; 
        else memberPosition_TMP.text = string.Format("{0}", currentCharacter.position + 1);
        memberName_TMP.text = currentCharacter.Name;
        member_Icon.sprite = currentCharacter.portrait;


        // Clear dirty ui
        for (int i = equipments.Count - 1; i >= 0; i--)
            Destroy(equipments[i].gameObject);

        equipments.Clear();


        // Add in all relevant equipment to list.
        List<EquipmentData> equipmentInventory = playerData.equipments;
        for (int i = 0; i < equipmentInventory.Count; i++)
        {
            EquipmentData current = equipmentInventory[i];
            if (current.type == type)
            {
                GameObject go = Instantiate(equipmentUIPrefab, equipmentGroup);
                if (go.TryGetComponent(out EquipmentUI equipmentUI))
                {
                    equipmentUI.SetData(current);
                    equipmentUI.UpdateUI();
                    equipmentUI.swap_button.onClick.AddListener(() => ChangeEquipment(equipmentUI.GetData()));
                    equipments.Add(equipmentUI);
                }
            }
        }

        // Update equipment headers
        switch (type)
        {
            case EquipmentData.TYPE.WEAPON:
                equipmentType_TMP.text = "Weapon";
                equipmentStat_TMP.text = "Attack";
                break;

            case EquipmentData.TYPE.ARMOUR:
                equipmentType_TMP.text = "Armour";
                equipmentStat_TMP.text = "Defense";
                break;

            case EquipmentData.TYPE.ACCESSORY:
                equipmentType_TMP.text = "Accessory";
                equipmentStat_TMP.text = "";
                break;
        }

        detailsGroup.gameObject.SetActive(false);
        UpdateUI();
    }
    private void ChangeEquipment(EquipmentData input)
    {
        if (input == null)
        {
            return;
        }
        if (currentCharacter == null)
        {
            return;
        }


        // Slot is occupied.
        if (heldEquipment != null)
        {
            // Same equipment id, un-equip it. 
            if (input.id == heldEquipment.id) UnEquip();
            else Equip(input);

            return;
        }

        // Slot is empty, equip it.
        Equip(input);
    }
    private void UnEquip()
    {
        switch (type)
        {
            case EquipmentData.TYPE.WEAPON:
                currentCharacter.weapon = null;
                break;

            case EquipmentData.TYPE.ARMOUR:
                currentCharacter.armour = null;
                break;

            case EquipmentData.TYPE.ACCESSORY:
                currentCharacter.accessory = null;
                break;
        }

        heldEquipment = null;
        UpdateUI();
    }
    private void Equip(EquipmentData input)
    {
        switch (type)
        {
            case EquipmentData.TYPE.WEAPON:
                currentCharacter.weapon = input;
                break;

            case EquipmentData.TYPE.ARMOUR:
                currentCharacter.armour = input;
                break;

            case EquipmentData.TYPE.ACCESSORY:
                currentCharacter.accessory = input;
                break;
        }

        heldEquipment = input;
        UpdateUI();
    }
    private void UpdateUI()
    {
        switch (type)
        {
            case EquipmentData.TYPE.WEAPON:
                heldEquipmentType_TMP.text = "Current Weapon";
                heldEquipmentStatHeader_TMP.text = "Attack";
                break;

            case EquipmentData.TYPE.ARMOUR:
                heldEquipmentType_TMP.text = "Current Armour";
                heldEquipmentStatHeader_TMP.text = "Defense";
                break;

            case EquipmentData.TYPE.ACCESSORY:
                heldEquipmentType_TMP.text = "Current Accessory";
                heldEquipmentStatHeader_TMP.text = "N.A.";
                break;
        }

        // Update current equipment
        if (heldEquipment != null)
        {
            // Update current equipment
            if (heldEquipment.icon != null) heldEquipment_Icon.sprite = heldEquipment.icon;

            if (type == EquipmentData.TYPE.ACCESSORY) heldEquipmentStat_TMP.text = "-";
            else heldEquipmentStat_TMP.text = string.Format("{0}", heldEquipment.statPoint);

            heldEquipmentName_TMP.text = heldEquipment.Name;
            heldEquipmentDescription_TMP.text = string.Format("{0}", heldEquipment.Description);

            // Update details
            heldDetailsGroup.gameObject.SetActive(true);

            if (heldEquipment.effectPrefab == null)
                heldEffectsGroup.gameObject.SetActive(false);
            else
            {
                heldEquipmentEffect_TMP.text = string.Format("+ {0}", heldEquipment.effectName);
                heldEffectsGroup.gameObject.SetActive(true);
            }
        }
        else
        {
            heldEquipmentName_TMP.text = "None";
            heldEquipmentStat_TMP.text = "-";
            heldDetailsGroup.gameObject.SetActive(false);
        }


        // Update all other equipment stat change.
        for (int i = 0; i < equipments.Count; i++)
        {
            EquipmentUI equipmentUI = equipments[i];
            EquipmentData equipmentData = equipmentUI.GetData();

            if (heldEquipment != null)
            {
                float tempPoint = equipmentData.statPoint;
                float currentPoint = heldEquipment.statPoint;

                if (tempPoint > currentPoint)
                    equipmentUI.UpdateStatIcon(arrowUp_Icon);
                else if (tempPoint < currentPoint)
                    equipmentUI.UpdateStatIcon(arrowDown_Icon);
                else
                    equipmentUI.UpdateStatIcon(arrowNeutral_Icon);
            }
            else
            {
                equipmentUI.UpdateStatIcon(arrowUp_Icon);
            }
        }
    }
}