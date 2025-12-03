using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private EnemyData data;
    public CombatEntity entity;

    [Header("Runtime (Stat)")]
    public int Level;
    public int countID;

    [Header("Action Dice")]
    public RectTransform actionGroup;
    public List<ActionSlot> actions;

    [Header("User Interface (UI)")]
    public HPUI hpUI;
    public CoinUI coinUI;


    public void Initialise(EnemyData input)
    {
        if (input == null) return;
        data = input;

        entity.Initialise(data);
    }
    public EnemyData GetData()
    {
        return data;
    }
}