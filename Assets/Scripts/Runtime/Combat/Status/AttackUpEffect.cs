using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttackUpEffect : MonoBehaviour, IStatusEffect
{
    public void OnEnter(Status statusParent, CombatEntity entity, List<Character> onFieldCharacters, List<Enemy> onFieldEnemies)
    {
        entity.ATK = entity.BaseATK;
        entity.ATK *= 1.10f * statusParent.potency;
    }

    public void OnHit(CombatEntity entity)
    {
    }

    public void OnExit(Status statusParent, CombatEntity entity)
    {
        statusParent.count -= 1;
        statusParent.UpdateUI();
    }
}
