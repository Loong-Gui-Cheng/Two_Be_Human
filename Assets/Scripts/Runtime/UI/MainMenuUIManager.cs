using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Player Save")]
    [SerializeField] private PlayerData playerData;

    [Header("Player Statistics")]
    [SerializeField] private TextMeshProUGUI teamLevel_TMP;
    [SerializeField] private TextMeshProUGUI teamGold_TMP;

    [Header("Combat Loadout")]
    [SerializeField] private List<MemberUI> combatMembers;

    private readonly Dictionary<CharacterData.ID, MemberUI> combatDictionary = new();
    public static System.Action OnUpdatePlayer;
    public static System.Action OnUpdateTeam;

    private void OnEnable()
    {
        OnUpdatePlayer += UpdatePlayerUI;
        OnUpdateTeam += UpdateTeamUI;    
    }
    private void OnDisable()
    {
        OnUpdatePlayer -= UpdatePlayerUI;
        OnUpdateTeam -= UpdateTeamUI;
    }


    private void Start()
    {
        OnLoad();
    }
    private void OnLoad()
    {
        UpdatePlayerUI();
        UpdateTeamUI();
    }
    private void UpdatePlayerUI()
    {
        teamLevel_TMP.text = string.Format("Team LV: {0}", playerData.teamLevel);
        teamGold_TMP.text = string.Format("Gold: {0}", playerData.gold);
    }
    private void UpdateTeamUI()
    {
        // Clear dirty ui
        for (int i = 0; i < combatMembers.Count; i++)
            combatMembers[i].gameObject.SetActive(false);

        combatDictionary.Clear();


        List<CharacterData> combatReady = playerData.combatCharacters;
        for (int i = 0; i < combatReady.Count; i++)
        {
            CharacterData character = combatReady[i];

            if (!combatDictionary.ContainsKey(character.id))
            {
                combatMembers[i].SetData(character);
                combatMembers[i].UpdateUI();
                combatMembers[i].gameObject.SetActive(true);
                combatDictionary.Add(character.id, combatMembers[i]);
            }
        }
    }
}
