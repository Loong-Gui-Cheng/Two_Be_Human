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

    [Header("Base Stats")]
    public int Level;
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;

    [Header("Base Speed Range")]
    [Range(1, 6)] public int minSPD;
    [Range(2, 7)] public int maxSPD;

    [Header("Resistances")]
    [Range(0.5f, 2f)] public float slashResist;
    [Range(0.5f, 2f)] public float pierceResist;
    [Range(0.5f, 2f)] public float bluntResist;
    [Range(0.5f, 2f)] public float magicResist;

    [Header("Party Abilities")]
    public int slot;

    [Header("Sprite Work")]
    public Sprite portrait;

    [Header("Skills")]
    public List<SkillData> skills;

    [Header("Equipment")]
    public List<GameObject> equipments;


    public void OnLoad(System.IO.BinaryReader binaryReader)
    {
    }
    public void OnValidate()
    {
        if (minSPD >= maxSPD) minSPD = maxSPD - 1;
        if (maxSPD < minSPD) maxSPD = minSPD + 1;
    }
}
