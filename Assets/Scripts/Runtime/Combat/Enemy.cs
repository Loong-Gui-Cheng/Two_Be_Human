using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private int countID;
    [SerializeField] private int Level;

    [Header("Runtime (Stat)")]
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;
    [SerializeField] private int SPD;

    public Enemy(EnemyData enemyData)
    {
        this.enemyData = enemyData;
        MaxHP = enemyData.MaxHP;
        HP = enemyData.HP;
        MaxHP = enemyData.MaxHP;
        ATK = enemyData.ATK;
        DEF = enemyData.DEF;
    }
    public void RandomiseSPD()
    {
        SPD = Random.Range(enemyData.minSPD, enemyData.maxSPD + 1);
    }
}
