using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ClashUI : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI attackResult_TMP;
    public SkillUI allySkillUI;
    public SkillUI enemySkillUI;

    private RectTransform rectTransform;
    [SerializeField, CE_ReadOnly] private bool IsClashUIDisplaying;
    [SerializeField, CE_ReadOnly] private bool IsAnimating;

    private void Start()
    {
        TryGetComponent(out rectTransform);
    }
    public void UpdateUI(ActionSlot allySlot, ActionSlot enemySlot)
    {
        attackResult_TMP.color = Color.white;
        attackResult_TMP.text = string.Empty;

        allySkillUI.SetupUI(allySlot);
        if (allySlot.skillData) attackResult_TMP.text = "Unopposed";

        // Check if enemy unopposes you. (Find all enemy slot and check who attacking this.)


        if (enemySlot == null)
        {
            enemySkillUI.ToggleUI(false);
            return;
        }
        enemySkillUI.SetupUI(enemySlot);

        // Player to enemy
        SetupMultiplierUI(allySlot, enemySlot);
        // Enemy to Player
        SetupMultiplierUI(enemySlot, allySlot);

        // Ally has action.


        // Opponent also has action.
        if (enemySlot.skillData != null)
        {
            // Check if this action slot clashes with enemy.
            bool clashingSkills = allySlot.skillData.type == SkillData.TYPE.CLASH && enemySlot.skillData.type == SkillData.TYPE.CLASH;
            if (clashingSkills && enemySlot.clashedBy.Count > 0)
            {
                ActionSlot enemyClashID = enemySlot.clashedBy[0];
                string allyID = allySlot.GetID();

                // Compare min-max rolls of both skills
                if (string.Compare(allyID, enemyClashID.GetID(), System.StringComparison.Ordinal) == 0)
                {
                    int aMin = allySlot.skillData.baseCoinPower;
                    int bMin = enemySlot.skillData.baseCoinPower;

                    int aMax = aMin + (allySlot.skillData.incrementCoinPower * allySlot.skillData.coins);
                    int bMax = bMin + (enemySlot.skillData.incrementCoinPower * enemySlot.skillData.coins);

                    if (aMin > bMin)
                    {
                        allySkillUI.SetMin(Color.yellow, aMin);
                        enemySkillUI.SetMin(Color.red, bMin);
                    }
                    else if (aMin < bMin)
                    {
                        allySkillUI.SetMin(Color.red, aMin);
                        enemySkillUI.SetMin(Color.yellow, bMin);
                    }

                    if (aMax > bMax)
                    {
                        allySkillUI.SetMax(Color.yellow, aMax);
                        enemySkillUI.SetMax(Color.red, bMax);
                    }
                    else if (aMax < bMax)
                    {
                        allySkillUI.SetMax(Color.red, aMax);
                        enemySkillUI.SetMax(Color.yellow, bMax);
                    }

                    attackResult_TMP.color = Color.yellow;
                    attackResult_TMP.text = "Clashing!";
                }
            }
        }
    }
    private void SetupMultiplierUI(ActionSlot host, ActionSlot target)
    {
        bool IsHostEnemy = false;
        CombatEntity targetEntity = null;

        // Check which entity is the target of the host.
        if (target.character != null)
        {
            IsHostEnemy = true;
            targetEntity = target.character.entity;
        }
        else targetEntity = target.enemy.entity;

        // Don't calculate if it is not an attack.
        if (host.skillData == null || host.skillData.type != SkillData.TYPE.CLASH)
        {
            if (IsHostEnemy) enemySkillUI.SetMultiplier(-1f);
            else allySkillUI.SetMultiplier(-1f);

            return;
        }

        // Calculate the attack multiplier if it goes through.
        SkillData hostSkill = host.skillData;
        float multiplier = 1f;

        switch (hostSkill.resistance)
        {
            case SkillData.RESISTANCE_TYPE.SLASH:
                multiplier *= targetEntity.slashResist;
                break;

            case SkillData.RESISTANCE_TYPE.BLUNT:
                multiplier *= targetEntity.bluntResist;
                break;

            case SkillData.RESISTANCE_TYPE.PIERCE:
                multiplier *= targetEntity.pierceResist;
                break;

            case SkillData.RESISTANCE_TYPE.MAGIC:
                multiplier *= targetEntity.magicResist;
                break;
        }

        if (IsHostEnemy) enemySkillUI.SetMultiplier(multiplier);
        else allySkillUI.SetMultiplier(multiplier);
    }

    public void AnimateClashUI() => StartCoroutine(DOTweenClashUI());
    private IEnumerator DOTweenClashUI()
    {
        if (IsAnimating) yield break;

        IsAnimating = true;
        IsClashUIDisplaying = !IsClashUIDisplaying;

        if (IsClashUIDisplaying)
        {
            canvas.enabled = true;
            rectTransform.DOLocalMoveY(0f, 0.3f);
        }
        else
            rectTransform.DOLocalMoveY(350f, 0.3f);


        yield return new WaitForSeconds(0.3f);

        if (!IsClashUIDisplaying) canvas.enabled = false;

        IsAnimating = false;
        yield break;
    }
}
