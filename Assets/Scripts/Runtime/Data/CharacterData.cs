using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "CharacterData", menuName = "Custom/CharacterData")]
public class CharacterData : ScriptableObject
{
    public enum ID
    {
        FIRST = 0,
        SECOND = 1,
        THIRD = 2,
        FOURTH = 3
    }

    [Header("Identifier")]
    public ID id;
    public string Name;
    public int position;
    public Sprite portrait;

    [Header("Base Stats")]
    public int Level;
    public float EXP;
    public float RequiredEXP;
    public int BaseMaxHP;
    public int BaseATK;
    public int BaseDEF;
    public float BaseCritChance = 0.1f;
    public int Base_BaseCoinBoost;
    public int Base_IncrementCoinBoost;

    [Header("Tabulated Stats")]
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;
    public float CritChance;
    public int BaseCoinBoost;
    public int IncrementCoinBoost;

    [Header("Base Speed Range")]
    [Range(1, 6)] public int minSPD;
    [Range(2, 7)] public int maxSPD;

    [Header("Resistances")]
    [Range(0.5f, 2f)] public float slashResist;
    [Range(0.5f, 2f)] public float pierceResist;
    [Range(0.5f, 2f)] public float bluntResist;
    [Range(0.5f, 2f)] public float magicResist;

    [Header("Skills")]
    public List<SkillData> skills;

    [Header("Equipment")]
    public EquipmentData weapon;
    public EquipmentData armour;
    public EquipmentData accessory;


    public void OnLoad(System.IO.BinaryReader binaryReader)
    {
    }
    public void RecalculateStat()
    {
        // Reset to base first, then change accordingly.
        MaxHP = BaseMaxHP;
        ATK = BaseATK;
        DEF = BaseDEF;
        CritChance = BaseCritChance;
        BaseCoinBoost = Base_BaseCoinBoost;
        IncrementCoinBoost = Base_IncrementCoinBoost;

        if (weapon != null) ATK += weapon.statPoint;
        if (armour != null) DEF += armour.statPoint;
    }
    public void OnValidate()
    {
        if (minSPD >= maxSPD) minSPD = maxSPD - 1;
        if (maxSPD < minSPD) maxSPD = minSPD + 1;

        if (weapon != null)
        {
            if (weapon.type != EquipmentData.TYPE.WEAPON)
                weapon = null;
        }
        if (armour != null)
        {
            if (armour.type != EquipmentData.TYPE.ARMOUR)
                armour = null;
        }
        if (accessory != null)
        {
            if (accessory.type != EquipmentData.TYPE.ACCESSORY)
                accessory = null;
        }

        RequiredEXP = 100f * (1.25f) * (Level - 1);
        RecalculateStat();
    }
}
