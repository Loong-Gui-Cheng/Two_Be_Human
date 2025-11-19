using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
[CreateAssetMenu(fileName = "EnemyData", menuName = "Custom/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Identifier")]
    public int id;
    public string Name;

    [Header("Base Stats")]
    public int Level;
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;

    [Header("Speed Range")]
    [Range(1, 6)] public int minSPD;
    [Range(2, 7)] public int maxSPD;
    public int SPD;

    [Header("Sprite Work")]
    public Sprite idleSprite;
    public Sprite atkSprite;
    public Sprite clashSprite;

    [Header("Loot Drops")]
    public float exp;
    public int gold;
    public List<GameObject> equipmentDrop;

    // Skills

    public void OnValidate()
    {
        if (minSPD >= maxSPD) minSPD = maxSPD - 1;
        if (maxSPD < minSPD) maxSPD = minSPD + 1;
    }
}
