using UnityEngine;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private CharacterData data;
    public CombatEntity entity;

    [Header("Action Dice")]
    public RectTransform actionGroup;
    public List<ActionSlot> actions;

    [Header("User Interface (UI)")]
    public HPUI hpUI;
    public CoinUI coinUI;


    public void Initialise(CharacterData input)
    {
        if (input == null) return;
        data = input;

        entity.Initialise(data);
    }
    public CharacterData GetData()
    {
        return data;
    }
}