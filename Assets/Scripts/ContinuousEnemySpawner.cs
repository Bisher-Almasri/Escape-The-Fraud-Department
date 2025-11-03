using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousEnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject enemyPrefab;
    public int initialEnemies = 5;
    public int bonusEnemiesPerWave = 2;
    public int killsForBonusWave = 5;
    public float spawnDelay = 2f;
    public float minDistanceFromPlayer = 8f;
    public float maxDistanceFromPlayer = 20f;
    
    [Header("Difficulty Scaling")]
    public float healthIncreaseRate = 1.1f;
    public float speedIncreaseRate = 1.05f;
    public int enemiesKilledForIncrease = 5;
    
    private MazeGenerator mazeGenerator;
    private Transform playerTransform;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int enemiesKilled = 0;
    private int currentTargetEnemyCount;
    private float currentHealthMultiplier = 1f;
    private float currentSpeedMultiplier = 1f;
    private bool isSpawning = false;
    private bool initialSpawnComplete = false;

    void Start()
    {
        
        currentTargetEnemyCount = initialEnemies;
        
        
        mazeGenerator = TryGetComponent<MazeGenerator>(out var mazeGen) ? mazeGen : FindObjectOfType<MazeGenerator>();
        FindPlayer();
        
        
        StartCoroutine(WaitForMazeAndSpawn());
    }
    
    private IEnumerator WaitForMazeAndSpawn()
    {
        
        while (mazeGenerator == null || mazeGenerator.GetPathCells().Count == 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("Maze generation complete! Waiting for player...");
        
        
        while (playerTransform == null)
        {
            FindPlayer();
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("Player found! Starting continuous enemy spawning...");
        
        
        StartCoroutine(SpawnEnemiesRoutine());
    }

    void Update()
    {
        
        activeEnemies.RemoveAll(enemy => enemy == null);
        
        
        if (initialSpawnComplete && !isSpawning && activeEnemies.Count < currentTargetEnemyCount)
        {
            StartCoroutine(SpawnEnemyWithDelay());
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
                playerTransform = playerMovement.transform;
        }
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        yield return new WaitForSeconds(1f); 
        
        Debug.Log($"Starting to spawn {initialEnemies} initial enemies...");
        
        
        for (int i = 0; i < initialEnemies; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log($"Initial enemy spawning complete! {activeEnemies.Count} enemies active.");
        initialSpawnComplete = true;
    }

    private IEnumerator SpawnEnemyWithDelay()
    {
        isSpawning = true;
        yield return new WaitForSeconds(spawnDelay);
        SpawnEnemy();
        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is null! Please assign an enemy prefab to the ContinuousEnemySpawner.");
            return;
        }
        
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator is null! Make sure the MazeGenerator exists in the scene.");
            return;
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("Player transform is null! Make sure the player has the 'Player' tag or PlayerMovement component.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("Could not find valid spawn position for enemy!");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        
        AI aiComponent = enemy.GetComponent<AI>();
        if (aiComponent == null)
            aiComponent = enemy.AddComponent<AI>();
            
        aiComponent.player = playerTransform;
        aiComponent.moveSpeed = 3.5f * currentSpeedMultiplier;
        
        
        if (!enemy.TryGetComponent<EnemyHealth>(out var healthComponent))
            healthComponent = enemy.AddComponent<EnemyHealth>();
            
        healthComponent.maxHealth = 100f * currentHealthMultiplier;
        
        
        EnemyDeathTracker deathTracker = enemy.AddComponent<EnemyDeathTracker>();
        deathTracker.spawner = this;
        
        activeEnemies.Add(enemy);
        
        Debug.Log($"Spawned enemy at {spawnPosition} with {healthComponent.maxHealth} health and {aiComponent.moveSpeed} speed");
    }

    private Vector3 GetValidSpawnPosition()
    {
        if (mazeGenerator == null || playerTransform == null)
            return Vector3.zero;

        List<Vector2Int> pathCells = mazeGenerator.GetPathCells();
        
        if (pathCells == null || pathCells.Count == 0)
            return Vector3.zero;

        int attempts = 0;
        while (attempts < 50)
        {
            Vector2Int cell = pathCells[Random.Range(0, pathCells.Count)];
            Vector3 worldPos = new Vector3(cell.x * mazeGenerator.cellSize, 1.5f, cell.y * mazeGenerator.cellSize);
            
            float distanceToPlayer = Vector3.Distance(worldPos, playerTransform.position);
            
            if (distanceToPlayer >= minDistanceFromPlayer && distanceToPlayer <= maxDistanceFromPlayer)
            {
                return worldPos;
            }
            
            attempts++;
        }
        
        
        if (pathCells.Count > 0)
        {
            Vector2Int fallbackCell = pathCells[Random.Range(0, pathCells.Count)];
            return new Vector3(fallbackCell.x * mazeGenerator.cellSize, 1.5f, fallbackCell.y * mazeGenerator.cellSize);
        }
        
        return Vector3.zero;
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"Enemy killed! Total killed: {enemiesKilled}");
        
        
        if (enemiesKilled % killsForBonusWave == 0)
        {
            currentTargetEnemyCount += bonusEnemiesPerWave;
            Debug.Log($"{killsForBonusWave} enemies killed! Target enemy count increased to {currentTargetEnemyCount}");
        }
        
        
        if (enemiesKilled % enemiesKilledForIncrease == 0)
        {
            currentHealthMultiplier *= healthIncreaseRate;
            currentSpeedMultiplier *= speedIncreaseRate;
            Debug.Log($"Difficulty increased! Health: {currentHealthMultiplier:F2}x, Speed: {currentSpeedMultiplier:F2}x");
        }
    }
    


    
    public int ActiveEnemyCount => activeEnemies.Count;
    public int TargetEnemyCount => currentTargetEnemyCount;
    public int EnemiesKilled => enemiesKilled;
    public float HealthMultiplier => currentHealthMultiplier;
    public float SpeedMultiplier => currentSpeedMultiplier;
}