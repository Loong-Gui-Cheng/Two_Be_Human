using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Diagnostics;

public class StatTreeSystem : MonoBehaviour
{
    [System.Serializable]
    private class StatUnlockPath
    {
        public StatData.TYPE statType;
        public List<Image> linkImageList;
    }

    [Header("Player Save")]
    [SerializeField] private PlayerData playerData;

    [Header("Unlockable Stats")]
    [SerializeField] private List<StatUnlockPath> statUnlockPathList;
    [SerializeField] private List<StatUnlockableUI> statUnlockables;

    [Header("User Interface (UI)")]
    [SerializeField] private TextMeshProUGUI teamAvailablePoints_TMP;
    [SerializeField] private Sprite lineSprite;
    [SerializeField] private Sprite lineGlowSprite;

    public static event System.Action OnUpdateStatUI;


    private void Start()
    {
        OnLoad();
    }
    private void OnLoad()
    {
        for (int i = 0; i < statUnlockables.Count; i++)
        {
            StatData.TYPE type = statUnlockables[i].data.type;
            statUnlockables[i].button.onClick.AddListener(() => { UnlockStat(type); });
        }

        UpdateStat();
    }

    private List<StatData.TYPE> GetStatRequirements(StatData.TYPE statType)
    {
        List<StatData.TYPE> requirements = new();

        switch (statType)
        {
            case StatData.TYPE.MAX_HP_BOOST_2:
                requirements.Add(StatData.TYPE.MAX_HP_BOOST_1);
                break;

            case StatData.TYPE.ATK_BOOST_2:
                requirements.Add(StatData.TYPE.ATK_BOOST_1);
                break;

            case StatData.TYPE.CRIT_CHANCE_BOOST_2:
                requirements.Add(StatData.TYPE.CRIT_CHANCE_BOOST_1);
                break;

            case StatData.TYPE.BASE_POWER_BOOST:
                requirements.Add(StatData.TYPE.MAX_HP_BOOST_2);
                requirements.Add(StatData.TYPE.ATK_BOOST_2);
                requirements.Add(StatData.TYPE.CRIT_CHANCE_BOOST_2);
                break;

            case StatData.TYPE.INCREMENT_POWER_BOOST:
                requirements.Add(StatData.TYPE.BASE_POWER_BOOST);
                break;
        }

        return requirements;
    }

    private bool CanUnlock(StatData.TYPE statType)
    {
        if (playerData == null) return false;

        // ERROR: Not enough points
        if (playerData.teamAvailablePoints <= 0) return false;

        // Calculate matching conditionals 
        List<StatData.TYPE> prerequisites = GetStatRequirements(statType);
        int requirementsMet = 0;
        for (int i = 0; i < prerequisites.Count; i++)
        {
            if (IsStatUnlocked(prerequisites[i]))
                requirementsMet++;
        }

        // Unlock stat logic
        if (requirementsMet >= prerequisites.Count)
        {
            if (!IsStatUnlocked(statType)) return true;
            else
            {
                // ERROR: Already unlocked.
                return false;
            }
        }

        // ERROR: Conditions not met.
        return false;
    }
    private bool UnlockStat(StatData.TYPE statType)
    {
        if (CanUnlock(statType))
        {
            playerData.teamAvailablePoints -= 1;
            playerData.unlockedStatTypes.Add(statType);
            UpdateStat();
            return true;
        }

        return false;
    }


    private void UpdateStat()
    {
        // Reset stat to base, then re-calculate with new values.
        for (int i = 0; i < playerData.characters.Count; i++)
        {
            CharacterData character = playerData.characters[i];
            character.RecalculateStat();

            if (IsStatUnlocked(StatData.TYPE.MAX_HP_BOOST_1))
                character.MaxHP *= 1.15f;

            if (IsStatUnlocked(StatData.TYPE.MAX_HP_BOOST_2))
                character.MaxHP *= 1.25f;

            if (IsStatUnlocked(StatData.TYPE.ATK_BOOST_1))
                character.ATK *= 1.10f;

            if (IsStatUnlocked(StatData.TYPE.ATK_BOOST_2))
                character.ATK *= 1.20f;

            if (IsStatUnlocked(StatData.TYPE.CRIT_CHANCE_BOOST_1))
                character.CritChance *= 1.25f;

            if (IsStatUnlocked(StatData.TYPE.CRIT_CHANCE_BOOST_2))
                character.CritChance *= 1.25f;

            if (IsStatUnlocked(StatData.TYPE.BASE_POWER_BOOST))
                character.BaseCoinBoost += 1;

            if (IsStatUnlocked(StatData.TYPE.INCREMENT_POWER_BOOST))
                character.IncrementCoinBoost += 1;

            character.MaxHP = Mathf.RoundToInt(character.MaxHP);
            character.ATK = Mathf.RoundToInt(character.ATK);
        }

        UpdateUI();
    }
    private void UpdateUI()
    {
        if (playerData == null) return;

        teamAvailablePoints_TMP.text = string.Format("Points Remaining: {0}", playerData.teamAvailablePoints);

        for (int i = 0; i < statUnlockables.Count; i++)
        {
            UpdateStatUnlockableUI(statUnlockables[i]);
        }

        // Darken all links to reset it.
        foreach (StatUnlockPath path in statUnlockPathList)
        {
            foreach (Image link in path.linkImageList)
            {
                link.color = Color.gray;
                link.sprite = lineSprite;
            }
        }
        // Light-up links based on conditions.
        foreach (StatUnlockPath path in statUnlockPathList)
        {
            if (IsStatUnlocked(path.statType) || CanUnlock(path.statType))
            {
                foreach (Image link in path.linkImageList)
                {
                    link.color = Color.white;
                    link.sprite = lineGlowSprite;
                }
            }
        }

        OnUpdateStatUI?.Invoke();
    }
    private void UpdateStatUnlockableUI(StatUnlockableUI statUnlockableUI)
    {
        if (playerData == null) return;

        StatData.TYPE type = statUnlockableUI.data.type;
        Button button = statUnlockableUI.button;

        if (IsStatUnlocked(type))
        {
            statUnlockableUI.UpdateUI(StatUnlockableUI.STATUS.UNLOCKED);
            button.enabled = false;
        }
        else
        {
            if (CanUnlock(type))
            {
                statUnlockableUI.UpdateUI(StatUnlockableUI.STATUS.UNLOCKABLE);
                button.enabled = true;
            }
            else
            {
                statUnlockableUI.UpdateUI(StatUnlockableUI.STATUS.LOCKED);
                button.enabled = false;
            }
        }
    }
    private bool IsStatUnlocked(StatData.TYPE type)
    {
        if (playerData != null) 
            return playerData.unlockedStatTypes.Contains(type);

        return false;
    }
}
