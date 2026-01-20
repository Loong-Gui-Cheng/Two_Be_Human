using UnityEngine;

[CreateAssetMenu(fileName = "STATUS_DATA", menuName = "Custom/StatusData")]
public class StatusData : ScriptableObject
{
    public enum TYPE
    {
        ATTACK_POWER_UP_1 = 0,
        FRAGILE = 1,
        BURN = 2, 
        RUPTURE = 3
    }

    [Header("Identifier")]
    public TYPE type;
    public Sprite icon;
    public string Name;
    public string description;

    [Header("Effect")]
    public GameObject effectPrefab;
}