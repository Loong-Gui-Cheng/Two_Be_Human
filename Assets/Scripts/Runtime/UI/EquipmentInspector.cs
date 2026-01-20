using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInspector : MonoBehaviour
{
    [Header("Member Inspection")]
    [SerializeField] private TextMeshProUGUI memberName_TMP;
    [SerializeField] private TextMeshProUGUI effect_TMP;
    [SerializeField] private TextMeshProUGUI description_TMP;

    [SerializeField] private RectTransform detailsGroup;
    [SerializeField] private RectTransform effectGroup;

    [Header("Team Loadout")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject characterCardUIPrefab;

    [Header("Tracker")]
    [SerializeField] private Transform cardGroup;
    [SerializeField] private List<EquipInspectorUI> equipments;

    [SerializeField, CE_ReadOnly] private CharacterCardUI currentCharacter;
    [SerializeField, CE_ReadOnly] private EquipInspectorUI currentEquipment;
    [SerializeField, CE_ReadOnly] private List<CharacterCardUI> characters;

    public static event System.Action<CharacterCardUI, EquipInspectorUI> OnEditEquipment;
    public static System.Action OnUpdateEquipment;

    private void OnEnable()
    {
        TeamManager.OnUpdateLoadoutUI += ReceiveLoadoutUpdate;
        OnUpdateEquipment += UpdateCurrentCharacter;
    }
    private void OnDisable()
    {
        TeamManager.OnUpdateLoadoutUI -= ReceiveLoadoutUpdate;
        OnUpdateEquipment -= UpdateCurrentCharacter;
    }

    private void Start()
    {
        OnLoad();
    }
    private void OnLoad()
    {
        // Initialise equipment inspection
        for (int i = 0; i < equipments.Count; i++)
        {
            EquipInspectorUI gear = equipments[i];
            gear.inspect_button.onClick.AddListener(() => EditEquipment());
        }

        // Initialise Currently Unlocked Characters.
        if (characterCardUIPrefab != null)
        {
            List<CharacterData> charUnlocked = playerData.characters;
            for (int i = 0; i < charUnlocked.Count; i++)
            {
                if (charUnlocked[i] == null) continue;

                GameObject character = Instantiate(characterCardUIPrefab, cardGroup);
                character.TryGetComponent(out CharacterCardUI card);

                // Update UI
                card.inspect_Button.onClick.AddListener(() => InspectMember(card));
                card.inspect_Button.onClick.AddListener(() => { AudioController.Instance.PlayUI(AudioController.SOUND_ID.CHARACTER_INSPECT); });
                card.SetData(charUnlocked[i]);
                card.UpdateLoadoutUI();

                // Set references
                characters.Add(card);
            }
        }

        // Update UI on launch.
        if (characters.Count >= 1) InspectMember(characters[0]);
    }

    private void InspectMember(CharacterCardUI card)
    {
        if (card.GetData() == null) return;
        if (currentCharacter != null) currentCharacter.ToggleInspectUI(false);

        currentCharacter = card;
        currentCharacter.ToggleInspectUI(true);
        CharacterData character = currentCharacter.GetData();

        memberName_TMP.text = character.Name;

        UpdateEquipmentUI(character);

        detailsGroup.gameObject.SetActive(false);
        effectGroup.gameObject.SetActive(true);
    }
    public void InspectEquipment(EquipInspectorUI equipmentUI)
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
    private void EditEquipment()
    {
        OnEditEquipment?.Invoke(currentCharacter, currentEquipment);
    }

    private void UpdateEquipmentUI(CharacterData character)
    {
        if (equipments.Count != 3) return;
        if (currentEquipment != null) currentEquipment.ToggleHighlightUI();

        currentEquipment = null;
        detailsGroup.gameObject.SetActive(false);

        equipments[0].SetData(character.weapon);
        equipments[1].SetData(character.armour);
        equipments[2].SetData(character.accessory);

        for (int i = 0; i < equipments.Count; i++)
            equipments[i].UpdateUI();
    }
    private void UpdateCurrentCharacter()
    {
        UpdateEquipmentUI(currentCharacter.GetData());
    }
    private void ReceiveLoadoutUpdate()
    {
        for (int i = 0; i < characters.Count; i++)
            characters[i].UpdateLoadoutUI();
    }
}
