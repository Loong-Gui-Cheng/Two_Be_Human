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
    public List<EquipmentData> equipments;
    public List<EquipmentData> items;

    public void OnLoad(System.IO.BinaryReader binaryReader)
    {

    }
    public void ResetSave()
    {

    }
    public void UpdateLoadout()
    {
        for (int i = 0; i < characters.Count; i++)
            characters[i].position = -1;

        for (int i = 0; i < combatCharacters.Count; i++)
            combatCharacters[i].position = i;
    }
    public void OnValidate()
    {
        UpdateLoadout();
    }
}