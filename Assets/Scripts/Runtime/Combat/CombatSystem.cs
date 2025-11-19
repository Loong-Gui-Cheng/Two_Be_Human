using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class CombatSystem : MonoBehaviour
{
    [Header("Combat System")]
    [SerializeField] private PlayerData playerData;
    [SerializeField, CE_ReadOnly] private List<Character> characters;
    [SerializeField, CE_ReadOnly] private List<Enemy> enemies;

    [Header("Prefabs")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Tracker")]
    [SerializeField] private int turnCount;
    [SerializeField, CE_ReadOnly] private List<Enemy> onFieldEnemies;

    public void OnLoad(PlayerData playerData)
    {
        this.playerData = playerData;
    }

    public void InitialiseBattle(List<EnemyData> enemyData)
    {
        for (int i = 0; i < playerData.combatCharacters.Count; i++)
        {
            GameObject characterGO = Instantiate(characterPrefab);
            if (characterGO.TryGetComponent(out Character character))
            {
                characters.Add(character);
            }
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            GameObject enemyGO = Instantiate(enemyPrefab);
            if (enemyGO.TryGetComponent(out Enemy enemy))
            {
                enemies.Add(enemy);
            }
        }

        for (int i = 0; i < 3; i++)
        {

        }
    }
    private void Update()
    {
        
    }
    public void TurnStart()
    {
        // Randomize Speed Values
        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i];
            character.RandomiseSPD();
        }
        for (int i = 0; i < onFieldEnemies.Count; i++)
        {
            Enemy enemy = onFieldEnemies[i];
            enemy.RandomiseSPD();
        }
    }
    public void TurnProcess()
    {

    }

    public void TurnEnd()
    {

    }

}
