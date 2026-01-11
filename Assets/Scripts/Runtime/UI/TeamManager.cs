using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamManager : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private Button mode_Button;
    [SerializeField] private TextMeshProUGUI mode_TMP;

    [Header("ColorBlock: Edit Team")]
    [SerializeField] private ColorBlock editTeam_CB;
    [Header("ColorBlock: Confirm Team")]
    [SerializeField] private ColorBlock confirm_CB;

    [Header("Resistance (Asset)")]
    [SerializeField] private Sprite slashSprite;
    [SerializeField] private Sprite pierceSprite;
    [SerializeField] private Sprite bluntSprite;
    [SerializeField] private Sprite magicSprite;

    [Header("Member Inspection")]
    [SerializeField, CE_ReadOnly] private CharacterCardUI currentCharacter;
    [SerializeField] private RectTransform skillGroup;
    [SerializeField] private GameObject skillMenuUIPrefab;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI level_TMP;
    [SerializeField] private TextMeshProUGUI hp_TMP;
    [SerializeField] private TextMeshProUGUI nextEXP_TMP;
    [SerializeField] private TextMeshProUGUI atk_TMP;
    [SerializeField] private TextMeshProUGUI def_TMP;
    [SerializeField] private TextMeshProUGUI spd_TMP;
    [SerializeField] private Slider hp_Slider;
    [SerializeField] private List<SkillMenuUI> skills;
    [SerializeField] private List<ResistanceUI> resistances;

    [Header("Team Loadout")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject characterCardUIPrefab;

    [Header("Tracker")]
    [SerializeField] private Transform characterGroup;

    // Read Only
    [SerializeField, CE_ReadOnly] private List<CharacterCardUI> characters;
    [SerializeField, CE_ReadOnly] private bool IsTeamMode;

    private readonly Dictionary<CharacterData.ID, CharacterCardUI> cardDictionary = new();
    private readonly Dictionary<CharacterData.ID, CharacterCardUI> slotDictionary = new();

    public static event System.Action OnUpdateLoadoutUI;

    private void Start()
    {
        OnLoad();
    }
    private void OnLoad()
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

        // Clear dirty ui
        for (int i = skills.Count - 1; i >= 0; i--)
            Destroy(skills[i].gameObject);

        skills.Clear();

        // Update new ui
        for (int i = 0; i < character.skills.Count; i++)
        {
            SkillData skillData = character.skills[i];
            if (skillData == null) continue;

            GameObject go = Instantiate(skillMenuUIPrefab, skillGroup);
            if (go.TryGetComponent(out SkillMenuUI skillMenuUI))
            {
                string skillOrder = string.Format("Skill {0}", i + 1);
                switch (skillData.resistance)
                {
                    case SkillData.RESISTANCE_TYPE.SLASH:
                        skillMenuUI.SetData(skillData, slashSprite, skillOrder);
                        break;
                    case SkillData.RESISTANCE_TYPE.PIERCE:
                        skillMenuUI.SetData(skillData, pierceSprite, skillOrder);
                        break;
                    case SkillData.RESISTANCE_TYPE.BLUNT:
                        skillMenuUI.SetData(skillData, bluntSprite, skillOrder);
                        break;
                    case SkillData.RESISTANCE_TYPE.MAGIC:
                        skillMenuUI.SetData(skillData, magicSprite, skillOrder);
                        break;
                }

                skills.Add(skillMenuUI);
            }
        }

        for (int i = 0; i < resistances.Count; i++)
            resistances[i].SetData(character);

        name_TMP.text = string.Format("{0}", character.Name);
        level_TMP.text = string.Format("LV {0}", character.Level);
        hp_TMP.text = string.Format("{0} / {1}", character.HP, character.MaxHP);
        nextEXP_TMP.text = string.Format("Next EXP: {0}", character.RequiredEXP - character.EXP);

        atk_TMP.text = string.Format("ATK: {0}", character.ATK);
        def_TMP.text = string.Format("DEF: {0}", character.DEF);
        spd_TMP.text = string.Format("SPD: {0} - {1}", character.minSPD, character.maxSPD);

        hp_Slider.maxValue = character.MaxHP;
        hp_Slider.minValue = 0;
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
        playerData.UpdateLoadout();
        List<CharacterData> combatLoadout = playerData.combatCharacters;


        // Clear dirty loadout ui arrangement
        slotDictionary.Clear();

        // Clear dirty card loadout ui 
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterCardUI card = characters[i];
            card.UpdateLoadoutUI();
        }

        // Update new loadout ui arrangement
        for (int i = 0; i < combatLoadout.Count; i++)
        {
            CharacterData combatCharData = combatLoadout[i];
            if (combatCharData == null) continue;
            if (slotDictionary.ContainsKey(combatCharData.id)) continue;

            if (cardDictionary.TryGetValue(combatCharData.id, out CharacterCardUI card))
                slotDictionary.Add(combatCharData.id, card);
        }

        OnUpdateLoadoutUI?.Invoke();
    }
}
