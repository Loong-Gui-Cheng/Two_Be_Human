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
        allySkillUI.ToggleUI(false);
        enemySkillUI.ToggleUI(false);

        attackResult_TMP.color = Color.white;
        attackResult_TMP.text = string.Empty;

        int aMin = 0;
        int aMax = 0;

        if (allySlot.skillData != null)
        {
            aMin = allySlot.skillData.baseCoinPower;
            aMax = aMin + (allySlot.skillData.incrementCoinPower * allySlot.skillData.coins);
            allySkillUI.SetupUI(allySlot, aMin, aMax);
            SetupMultiplierUI(allySlot, enemySlot);

            attackResult_TMP.text = "Unopposed";
        }

        // WIP: Find if enemy unopposes you on this slot. (Find all enemy slot and check who attacking this.)
        if (enemySlot == null) return;

        int bMin = 0;
        int bMax = 0;

        // Opponent also has action.
        if (enemySlot.skillData != null)
        {
            bMin = enemySlot.skillData.baseCoinPower;
            bMax = bMin + (enemySlot.skillData.incrementCoinPower * enemySlot.skillData.coins);
            enemySkillUI.SetupUI(enemySlot, bMin, bMax);
            SetupMultiplierUI(enemySlot, allySlot);

            // Check if this action slot clashes with enemy.
            bool clashingSkills = allySlot.skillData.type == SkillData.TYPE.CLASH && enemySlot.skillData.type == SkillData.TYPE.CLASH;

            // No skills clashes, unopposed.
            if (!(clashingSkills && enemySlot.clashedBy.Count > 0))
            {
                attackResult_TMP.text = "Unopposed";
                return;
            }

            ActionSlot enemyClashID = enemySlot.clashedBy[0];
            string allyID = allySlot.GetID();
            
            // Ally clash matches with enemy clash by.
            if (string.Compare(allyID, enemyClashID.GetID(), System.StringComparison.Ordinal) == 0)
            {
                // Compare min-max rolls of both skills
                if (aMin > bMin)
                {
                    allySkillUI.SetMinColor(Color.yellow);
                    enemySkillUI.SetMinColor(Color.red);
                }
                else if (aMin < bMin)
                {
                    allySkillUI.SetMinColor(Color.red);
                    enemySkillUI.SetMinColor(Color.yellow);
                }
                if (aMax > bMax)
                {
                    allySkillUI.SetMaxColor(Color.yellow);
                    enemySkillUI.SetMaxColor(Color.red);
                }
                else if (aMax < bMax)
                {
                    allySkillUI.SetMaxColor(Color.red);
                    enemySkillUI.SetMaxColor(Color.yellow);
                }

                attackResult_TMP.color = Color.yellow;
                attackResult_TMP.text = "Clashing!";
            }
            else
            {
                attackResult_TMP.text = "Unopposed";
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
