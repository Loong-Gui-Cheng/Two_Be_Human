using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Custom/EquipmentData")]
public class EquipmentData : ScriptableObject
{
    public enum TYPE
    {
        WEAPON = 0,
        ARMOUR = 1,
        ACCESSORY = 2,
    }

    [Header("Identifier")]
    public TYPE type;
    public int id;


    [Header("Details")]
    public Sprite icon;
    public string Name;
    [TextArea(3, 5)] public string Description;


    [Header("Stat")]
    public int amount;
    public float statPoint;
    public float goldValue;

    [Header("Effect")]
    public GameObject effectPrefab;
    public string effectName;
}
