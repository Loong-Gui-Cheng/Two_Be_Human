using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BurnEffect : MonoBehaviour, IStatusEffect
{
    [Header("Prefab")]
    [SerializeField] private GameObject statusDOTUIPrefab;

    public void OnEnter(Status statusParent, CombatEntity entity, List<Character> onFieldCharacters, List<Enemy> onFieldEnemies)
    {
        
    }

    public void OnHit(CombatEntity entity)
    {
    }

    public void OnExit(Status statusParent, CombatEntity entity)
    {
        statusParent.count -= 1;

        // Display damage indicator & character hp slider animation.
        float NewHP = entity.HP - statusParent.potency;
        entity.hpUI.AnimateHPSlider(entity.HP, NewHP);
        entity.HP = NewHP;

        GameObject go = Instantiate(statusDOTUIPrefab);
        go.transform.position = entity.transformStatusDOT.transform.position;

        if (go.TryGetComponent(out StatusDOTUI statusDOTUI))
            statusDOTUI.SetUpUI(statusParent.GetData(), statusParent.potency);

        statusParent.UpdateUI();
    }
}