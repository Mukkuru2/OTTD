using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Waves;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;
    public GameObject enemyPrefab3;
    public float spawnInterval = 3f;
    public int maxEnemies = 10;
    public int enemiesSpawned = 0;
    public int enemiesKilled = 0;
    public int enemiesEscaped = 0;

    public TextMeshProUGUI waveText;
    
    private float spawnTimer;

    Waves.Wave currentWave;
    int currentWaveIndex = 0;

    public static readonly List<Enemy> Enemies = new();

    private GameObject _startTile;


    public void Start()
    {
        Waves.Init();
        currentWave = Waves.waves[0];
        
        LevelState.StateChanged.AddListener(OnStateChanged);

        FieldGenerator.OnLevelGenerated.AddListener(StartSpawning);
    }

    public void FixedUpdateEnemies()
    {
        SpawnEnemies();
        for (var index = 0; index < Enemies.Count; index++)
        {
            var enemy = Enemies[index];
            enemy.UpdateEnemy();
        }
    }

    private void OnStateChanged()
    {
        // Loop through all enemies and set their state to the current state.
        foreach (Enemy enemy in Enemies)
        {
            Enemy.EnemyState enemyState = LevelState.State == LevelState.GameState.Running
                ? Enemy.EnemyState.Moving
                : Enemy.EnemyState.Paused;
            enemy.SetState(enemyState);
        }
    }

    private void StartSpawning()
    {
        // Find the start tile
        foreach (GameObject child in FieldGenerator.Field)
        {
            if (child.GetComponent<Tile>().tileType == Tile.TileType.Start)
            {
                _startTile = child.gameObject;
                break;
            }
        }
    }

    private IEnumerator SetWave(int i, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        
        // Get the wave from the Waves class.
        spawnTimer = 0;
        enemiesSpawned = 0;
        currentWave = Waves.waves[i];
        currentWaveIndex = i;
        
        // Set the wave text
        waveText.text = $"Wave: {currentWaveIndex + 1}";
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void SpawnEnemies()
    {
        spawnTimer += Time.fixedDeltaTime;

        // Exit if all enemies spawned
        if (enemiesSpawned >= currentWave.spawnTimes.Length)
        {
            // Check if all enemies are killed
            if (Enemies.Count == 0)
            {
                // Check if there are more waves
                StartCoroutine(SetWave(currentWaveIndex + 1, 5));
            }
            return;
        }
        
        if (spawnTimer < currentWave.spawnTimes[enemiesSpawned]) return;
        
        // Spawn an enemy on the pos of the start tile
        switch (currentWave.enemies[enemiesSpawned])
        {
            case 1:
                SpawnEnemy(enemyPrefab1);
                break;
            case 2:
                SpawnEnemy(enemyPrefab2);
                break;
            case 3:
                SpawnEnemy(enemyPrefab3);
                break;
        }
        enemiesSpawned++;
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        GameObject enemy = Instantiate(enemyPrefab, _startTile.transform.position, Quaternion.identity, transform);
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        enemyComponent.Create(FieldGenerator.GetTilePosition(_startTile));
        Enemies.Add(enemyComponent);
    }

    public static void RemoveEnemy(Enemy enemy)
    {
        Enemies.Remove(enemy);
        Economy.Money += (int)(enemy.reward * enemy.rewardPercentage);
        print(Economy.Money);
        Destroy(enemy.gameObject);
    }

    public static void EnemyFinished(Enemy enemy)
    {
        Enemies.Remove(enemy);
        Economy.Health -= (int)enemy.damage;
        Destroy(enemy.gameObject);
    }
}