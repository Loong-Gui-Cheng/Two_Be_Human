using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Custom/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Statistics")]
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;
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
}
