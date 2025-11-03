using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 10, height = 15;
    public float cellSize = 4f;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject gunPrefab;
    public Transform playerPrefab;
    public GameObject enemyPrefab;
    public GameObject sprintItemPrefab;

    [Header("Enemy Settings")]
    public int enemyCount = 1;
    [Tooltip("Minimum distance (world units) enemies must be from the player spawn")]
    public float enemyMinDistanceFromPlayer = 8f;
    [Tooltip("Minimum distance (world units) enemies must be from the gun pickup spawn (if any)")]
    public float enemyMinDistanceFromGun = 6f;

    [Header("Spawn Settings")]
    public bool spawnPlayer = true;
    public bool spawnEnemy = true;
    public Vector2Int spawnLocation = new(1, 1); // Default spawn at (1,1)

    private byte[,] maze; // 0 = wall, 1 = path
    private List<Vector2Int> pathCells = new();
    private Vector3 playerSpawnPosition;
    private Vector3 gunSpawnPosition = Vector3.positiveInfinity;
    private Vector3 sprintItemSpawnPosition = Vector3.positiveInfinity;
    private GameObject playerObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maze = new byte[width, height];


        spawnLocation.x = Mathf.Max(1, spawnLocation.x);
        spawnLocation.y = Mathf.Max(1, spawnLocation.y);
        spawnLocation.x = Mathf.Min(width - 2, spawnLocation.x);
        spawnLocation.y = Mathf.Min(height - 2, spawnLocation.y);
        if (spawnLocation.x % 2 == 0) spawnLocation.x++;
        if (spawnLocation.y % 2 == 0) spawnLocation.y++;

        GenerateMathRecursiveBacktrackerThingyMagibIDontKnowImStillInGrade9(spawnLocation.x, spawnLocation.y);
        EnsureFullConnectivity();
        DrawMaze();
        GetComponent<NavMeshSurface>()?.BuildNavMesh();
        SpawnPlayer();
        PlacePewPewInDeadEnd();
        PlaceSprintItem();
        SpawnEnemy();
    }

    void EnsureFullConnectivity()
    {
        for (int x = 1; x < width - 1; x += 2)
        {
            for (int y = 1; y < height - 1; y += 2)
            {
                if (maze[x, y] == 0)
                {
                    ConnectIsolatedArea(x, y);
                }
            }
        }
    }

    void ConnectIsolatedArea(int x, int y)
    {
        var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            int wallX = x + dir.x;
            int wallY = y + dir.y;
            int pathX = x + dir.x * 2;
            int pathY = y + dir.y * 2;

            if (pathX > 0 && pathY > 0 && pathX < width - 1 && pathY < height - 1 && maze[pathX, pathY] == 1)
            {
                maze[x, y] = 1;
                maze[wallX, wallY] = 1;
                pathCells.Add(new Vector2Int(x, y));
                GenerateMathRecursiveBacktrackerThingyMagibIDontKnowImStillInGrade9(x, y);
                break;
            }
        }
    }

    void GenerateMathRecursiveBacktrackerThingyMagibIDontKnowImStillInGrade9(int x, int y)
    {
        maze[x, y] = 1;
        pathCells.Add(new Vector2Int(x, y));

        var directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };


        for (int i = 0; i < directions.Count; i++)
        {
            var temp = directions[i];
            int randomIndex = UnityEngine.Random.Range(i, directions.Count);
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }

        foreach (var dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;


            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[nx, ny] == 0)
            {
                maze[x + dir.x, y + dir.y] = 1;
                GenerateMathRecursiveBacktrackerThingyMagibIDontKnowImStillInGrade9(nx, ny);
            }
        }
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    Vector3 pos = new(x * cellSize, 1, y * cellSize);
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
        CreateBoundaryWalls();
    }

    void CreateBoundaryWalls()
    {
        for (int x = -1; x <= width; x++)
        {
            Vector3 topPos = new(x * cellSize, 1, -1 * cellSize);
            Vector3 bottomPos = new(x * cellSize, 1, height * cellSize);
            Instantiate(wallPrefab, topPos, Quaternion.identity, transform);
            Instantiate(wallPrefab, bottomPos, Quaternion.identity, transform);
        }

        for (int y = -1; y <= height; y++)
        {
            Vector3 leftPos = new(-1 * cellSize, 1, y * cellSize);
            Vector3 rightPos = new(width * cellSize, 1, y * cellSize);
            Instantiate(wallPrefab, leftPos, Quaternion.identity, transform);
            Instantiate(wallPrefab, rightPos, Quaternion.identity, transform);
        }
    }

    void PlacePewPewInDeadEnd()
    {
        List<Vector2Int> deadEnds = new();

        foreach (var cell in pathCells)
        {
            int openNeighbors = 0;

            foreach (var dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int nx = cell.x + dir.x;
                int ny = cell.y + dir.y;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height && maze[nx, ny] == 1)
                    openNeighbors++;
            }

            if (openNeighbors == 1)
                deadEnds.Add(cell);
        }

        if (deadEnds.Count > 0 && gunPrefab != null)
        {
            var chosenDeadEnd = deadEnds[UnityEngine.Random.Range(0, deadEnds.Count)];
            Vector3 gunPos = new(chosenDeadEnd.x * cellSize, 1.5f, chosenDeadEnd.y * cellSize);

            int attempts = 0;
            while (Vector3.Distance(gunPos, playerSpawnPosition) < cellSize * 2 && attempts < 100)
            {
                chosenDeadEnd = deadEnds[UnityEngine.Random.Range(0, deadEnds.Count)];
                gunPos = new Vector3(chosenDeadEnd.x * cellSize, 1.5f, chosenDeadEnd.y * cellSize);
                attempts++;
            }

            GameObject gunPickup = Instantiate(gunPrefab, gunPos, Quaternion.identity, transform);
            gunPickup.transform.position = gunPos;
            if (gunPickup.GetComponent<GunPickup>() == null)
            {
                gunPickup.AddComponent<GunPickup>();
            }

            gunSpawnPosition = gunPos;
        }
    }

    void PlaceSprintItem()
    {
        if (sprintItemPrefab == null || pathCells == null || pathCells.Count == 0)
            return;

        
        List<Vector2Int> candidates = new List<Vector2Int>(pathCells);

        if (gunSpawnPosition != Vector3.positiveInfinity)
        {
            
            candidates.RemoveAll(c =>
            {
                var gunGridX = Mathf.RoundToInt(gunSpawnPosition.x / cellSize);
                var gunGridY = Mathf.RoundToInt(gunSpawnPosition.z / cellSize);
                return c.x == gunGridX && c.y == gunGridY;
            });
        }

        if (candidates.Count == 0)
            return;

        System.Random rng = new();
        int attempts = 0;
        Vector2Int chosen = candidates[rng.Next(candidates.Count)];
        Vector3 sprintPos = new(chosen.x * cellSize, 1.2f, chosen.y * cellSize);

        
        while ((Vector3.Distance(sprintPos, playerSpawnPosition) < cellSize * 1.5f ||
               (gunSpawnPosition != Vector3.positiveInfinity && Vector3.Distance(sprintPos, gunSpawnPosition) < cellSize)) &&
               attempts < 200)
        {
            chosen = candidates[rng.Next(candidates.Count)];
            sprintPos = new Vector3(chosen.x * cellSize, 1.2f, chosen.y * cellSize);
            attempts++;
        }

        GameObject sprintItem = Instantiate(sprintItemPrefab, sprintPos, Quaternion.identity, transform);
        sprintItem.transform.position = sprintPos;
        sprintItemSpawnPosition = sprintPos;
        
        if (sprintItem.GetComponent<SprintItem>() == null)
        {
            
            
            sprintItem.AddComponent<SprintItem>();
        }
    }

    void SpawnPlayer()
    {
        if (spawnPlayer && playerPrefab != null)
        {
            playerSpawnPosition = new Vector3(spawnLocation.x * cellSize, 1.5f, spawnLocation.y * cellSize);
            playerPrefab.position = playerSpawnPosition;
            Debug.Log($"Player spawned at maze position ({spawnLocation.x}, {spawnLocation.y}) - World position {playerSpawnPosition}");
        }
    }

    void SpawnEnemy()
    {
        
        if (TryGetComponent<ContinuousEnemySpawner>(out var continuousSpawner))
        {
            Debug.Log("Continuous enemy spawner detected - skipping initial enemy spawn");
            return;
        }

        if (!spawnEnemy || enemyPrefab == null)
            return;

        if (pathCells == null || pathCells.Count == 0)
            return;

        int spawned = 0;
        int guard = 0;
        System.Random rng = new();

        while (spawned < enemyCount && guard < enemyCount * 200)
        {
            guard++;
            var cell = pathCells[rng.Next(pathCells.Count)];
            Vector3 enemyPos = new(cell.x * cellSize, 1.5f, cell.y * cellSize);

            if (Vector3.Distance(enemyPos, playerSpawnPosition) < enemyMinDistanceFromPlayer)
                continue;

            if (gunSpawnPosition != Vector3.positiveInfinity && Vector3.Distance(enemyPos, gunSpawnPosition) < enemyMinDistanceFromGun)
                continue;

            if (Vector3.Distance(enemyPos, playerSpawnPosition) < 0.1f)
                continue;

            GameObject enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity, transform);
            if (!enemy.TryGetComponent(out AI aiComponent))
            {
                aiComponent = enemy.AddComponent<AI>();
            }

            if (playerObject != null)
            {
                aiComponent.player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning($"Spawned enemy '{enemy.name}' but no player object found to assign to its AI.");
            }

            spawned++;
        }

        if (spawned < enemyCount)
        {
            Debug.LogWarning($"Requested {enemyCount} enemies but only spawned {spawned} after trying {guard} times. Consider lowering distance constraints or increasing maze size.");
        }
    }

    
    public List<Vector2Int> GetPathCells()
    {
        return new List<Vector2Int>(pathCells);
    }
}
