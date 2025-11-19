using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField] private CharacterData characterData;

    [Header("Runtime (Stat)")]
    public int MaxHP;
    public int HP;
    public int ATK;
    public int DEF;
    [SerializeField] private int SPD; 

    public Character(CharacterData data)
    {
        if (data == null) return;
        characterData = data;
    }
    public CharacterData GetData()
    {
        return characterData;
    }
    public int GetSPD()
    {
        return SPD;
    }
    public void RandomiseSPD()
    {
        SPD = Random.Range(characterData.minSPD, characterData.maxSPD + 1);
    }
}
