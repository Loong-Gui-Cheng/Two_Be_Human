using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
[CreateAssetMenu(fileName = "PlayerData", menuName = "Custom/PlayerData")]
public class PlayerData : ScriptableObject
{
    public int saveID;

    [Header("Saveables")]
    public int worldID;
    public Vector2 position;
    public int gold;

    [Header("Party Status")]
    public int teamLevel;
    public float teamEXP;
    public int teamAvailablePoints;
    public List<CharacterData> characters;
    public List<CharacterData> combatCharacters;

    [Header("Inventory")]
    public int inventory;

    public void OnLoad(System.IO.BinaryReader binaryReader)
    {

    }
    public void ResetSave()
    {

    }
}