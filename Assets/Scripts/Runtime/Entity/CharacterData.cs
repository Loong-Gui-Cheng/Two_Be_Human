using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Custom/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Statistics")]
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;
    [Range(1, 6)] public int SPD;

    [Header("Party Abilities")]
    public int slot;

    [Header("Sprite Work")]
    public Sprite idleSprite;
    public Sprite atkSprite;
    public Sprite clashSprite;

    [Header("Equipment")]
    public List<GameObject> equipment;

    // Skills

    // Equipment

    public void OnLoad(System.IO.BinaryReader binaryReader)
    {


    }
}
