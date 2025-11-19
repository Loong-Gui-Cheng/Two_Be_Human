using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamManager : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private Canvas memberModeCanvas;
    [SerializeField] private Canvas teamModeCanvas;
    [SerializeField] private Button mode_Button;
    [SerializeField] private TextMeshProUGUI mode_TMP;

    [Header("ColorBlock: Edit Team")]
    [SerializeField] private ColorBlock editTeam_CB;
    [Header("ColorBlock: Confirm Team")]
    [SerializeField] private ColorBlock confirm_CB;


    [Header("Member Inspection")]
    [SerializeField, CE_ReadOnly] private CharacterCardUI currentCharacter;
    [SerializeField] private TextMeshProUGUI atk_TMP;
    [SerializeField] private TextMeshProUGUI def_TMP;
    [SerializeField] private TextMeshProUGUI spd_TMP;
    [SerializeField] private TextMeshProUGUI level_TMP;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI weakness_TMP;
    [SerializeField] private TextMeshProUGUI hp_TMP;
    [SerializeField] private Slider hp_Slider;

    [Header("Team Loadout")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject characterCardUIPrefab;

    [Header("Tracker")]
    [SerializeField] private Transform characterGroup;
    [SerializeField] private Transform slotGroup;

    // Read Only
    [SerializeField, CE_ReadOnly] private List<CharacterCardUI> characters;
    [SerializeField] private List<CharacterSlotUI> slots;
    [SerializeField, CE_ReadOnly] private bool IsTeamMode;

    private readonly Dictionary<CharacterData.ID, CharacterCardUI> cardDictionary = new();
    private readonly Dictionary<CharacterData.ID, CharacterSlotUI> slotDictionary = new();

    public void Start()
    {
        OnLoad();
    }
    public void OnLoad()
    {
        // Initialise Currently Unlocked Characters.
        if (characterCardUIPrefab != null)
        {
            List<CharacterData> charUnlocked = playerData.characters;
            for (int i = 0; i < charUnlocked.Count; i++)
            {
                if (charUnlocked[i] == null) continue;

                GameObject character = Instantiate(characterCardUIPrefab, characterGroup);
                character.TryGetComponent(out CharacterCardUI card);

                // Update UI
                card.loadout_Button.onClick.AddListener(() => LoadoutMember(card));
                card.inspect_Button.onClick.AddListener(() => InspectMember(card));
                card.SetData(charUnlocked[i]);
                card.UpdateLoadoutUI();

                // Set references
                characters.Add(card);
                cardDictionary.Add(card.GetData().id, card);
            }
        }

        // Update UI on launch.
        if (characters.Count >= 1) InspectMember(characters[0]);
        UpdateLoadoutUI();
    }

    public void SwitchMode()
    {
        // Swap between combat loadout and individual member inspection
        IsTeamMode = !IsTeamMode;
        teamModeCanvas.enabled = IsTeamMode;
        memberModeCanvas.enabled = !IsTeamMode;

        for (int i = 0; i < characters.Count; i++)
            characters[i].ToggleMode(IsTeamMode);

        if (currentCharacter != null) 
            currentCharacter.ToggleInspectUI(!IsTeamMode);

        if (IsTeamMode)
        {
            mode_Button.colors = confirm_CB;
            mode_TMP.text = "Confirm";
        }
        else
        {
            mode_Button.colors = editTeam_CB;
            mode_TMP.text = "Edit Team";
        }

        UpdateLoadoutUI();
    }

    public void LoadoutMember(CharacterCardUI card)
    {
        // Edit Combat Loadout
        CharacterData characterData = card.GetData();
        if (characterData == null) return;

        if (slotDictionary.ContainsKey(characterData.id)) RemoveFromLoadout(card);
        else AddToLoadout(card);

        UpdateLoadoutUI();

    }
    private void InspectMember(CharacterCardUI card)
    {
        if (card.GetData() == null) return;
        if (currentCharacter != null) currentCharacter.ToggleInspectUI(false);

        currentCharacter = card;
        currentCharacter.ToggleInspectUI(true);
        CharacterData character = currentCharacter.GetData();


        atk_TMP.text = string.Format("ATK: {0}", character.ATK);
        def_TMP.text = string.Format("DEF: {0}", character.DEF);
        spd_TMP.text = string.Format("SPD: {0} - {1}", character.minSPD, character.maxSPD);
        level_TMP.text = string.Format("Lvl: {0}", character.Level);
        name_TMP.text = string.Format("{0}", character.Name);
        weakness_TMP.text = string.Format("Weak to: {0}", "Slash");
        hp_TMP.text = string.Format("{0} / {1}", character.HP, character.MaxHP);

        hp_Slider.maxValue = character.MaxHP;
        hp_Slider.value = character.HP;
    }
    private void AddToLoadout(CharacterCardUI card)
    {
        List<CharacterData> combatLoadout = playerData.combatCharacters;
        if (combatLoadout.Count >= 3)
        {
            return;
        }
        combatLoadout.Add(card.GetData());
    }
    private void RemoveFromLoadout(CharacterCardUI card)
    {
        List<CharacterData> combatLoadout = playerData.combatCharacters;
        if (combatLoadout.Count <= 1)
        {
            return;
        }
        for (int i = 0; i < combatLoadout.Count; i++)
        {
            if (combatLoadout[i].id == card.GetData().id)
            {
                combatLoadout.RemoveAt(i);
                break;
            }
        }
    }

    public void UpdateLoadoutUI()
    {
        List<CharacterData> combatLoadout = playerData.combatCharacters;

        // Clear dirty loadout ui arrangement
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetData(null);
            slots[i].UpdateUI();
            slotDictionary.Clear();
        }
        // Clear dirty card loadout ui 
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterCardUI card = characters[i];
            card.UpdateLoadoutUI();
        }


        // Update new loadout ui arrangement
        if (IsTeamMode)
        {
            for (int i = 0; i < combatLoadout.Count; i++)
            {
                CharacterData combatCharData = combatLoadout[i];
                if (combatCharData == null) continue;

                for (int j = 0; j < slots.Count; j++)
                {
                    if (slots[j] == null) continue;
                    if (slots[j].GetData() != null) continue;
                    if (slotDictionary.ContainsKey(combatCharData.id)) continue;

                    slots[j].SetData(combatCharData);
                    slots[j].UpdateUI();
                    slotDictionary.Add(combatCharData.id, slots[j]);
                }

                // Update card ui
                if (cardDictionary.TryGetValue(combatCharData.id, out CharacterCardUI card))
                    card.DisplayLoadoutUI(i);
            }
        }
    }
}
