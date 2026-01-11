using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Custom/SkillData")]
public class SkillData : ScriptableObject
{
    public enum TYPE
    {
        CLASH = 0,
        DEFENCE = 1,
        ITEM = 2
    }
    // Attack type
    public enum RESISTANCE_TYPE
    {
        SLASH = 0,
        PIERCE = 1,
        BLUNT = 2,
        MAGIC = 3
    }

    [Header("Identifier")]
    public TYPE type;
    [ConditionalHide("type", true, true)] public RESISTANCE_TYPE resistance;

    [Header("Details")]
    public Sprite icon;
    public string Name;
    [TextArea(3, 5)] public string description;

    [Header("Coin Amount")]
    public int baseCoinPower;
    public int incrementCoinPower;
    public int coins;

    [Header("Effect")]
    public GameObject effectPrefab;
}