using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the enemy encounters before player goes into the combat scene.
/// </summary>
public class EncounterSystem : Singleton<EncounterSystem>
{
    [Header("Data Reference")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private List<EnemyData> enemies;

    public static event System.Action<List<EnemyData>> EncounterStarted;

    public void OnLoad(PlayerData input)
    {
        playerData = input;
    }
    public void EncounterEnemy(List<EnemyData> input)
    {
        if (input == null) return;

        for (int i = 0; i < input.Count; i++)
        {
            EnemyData enemyData = input[i];
            if (enemyData == null) continue;
            
            enemies.Add(input[i]);
        }
    }
}
