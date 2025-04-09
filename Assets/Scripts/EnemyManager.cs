using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

/// <summary>
/// Manages the spawning and resetting of enemies within the level.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Spawning Configuration")]
    [Tooltip("The prefab of the enemy to spawn (e.g., Goomba).")]
    public GameObject enemyPrefab;

    [Tooltip("The number of enemies to attempt to spawn at the start.")]
    public int numberOfEnemiesToSpawn = 5;

    [Tooltip("The bottom-left corner of the rectangular area where enemies can spawn.")]
    public Vector2 spawnAreaMin = new(-5f, -2f);

    [Tooltip("The top-right corner of the rectangular area where enemies can spawn.")]
    public Vector2 spawnAreaMax = new(20f, 0f);

    [Tooltip("Minimum distance required between spawned enemies.")]
    public float minSpawnSeperation = 1.0f;

    [Tooltip("Minimum distance required from the player's initial position when spawning.")]
    public float minDistanceFromPlayer = 3.0f;

    [Tooltip("Reference to the player's transform to avoid spawning on top of them.")]
    public Transform playerTransform;

    [Tooltip("LayerMask to check for obstacles (like Ground, Blocks) before spawning.")]
    public LayerMask obstacleLayerMask;

    [Tooltip("Maximum attempts to find a valid spawn position for each enemy.")]
    public int maxSpawnAttemptsPerEnemy = 20;

    /// <summary>
    /// List to keep track of positions where enemies have been successfully spawned in the current wave.
    /// Used to ensure minimum separation.
    /// </summary>
    private List<Vector2> _spawnedPositions = new();

    /// <summary>
    /// Clears existing enemies and spawns a new set at random locations.
    /// Typically called at the very start of the game/level.
    /// </summary>
    public void InitialSpawnEnemies()
    {
        ClearAllEnemies();
        _spawnedPositions.Clear();

        Vector2 playerStartPosition = playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;

        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            bool positionFound = false;
            for (int attempt = 0; attempt < maxSpawnAttemptsPerEnemy; attempt++)
            {
                float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
                float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
                Vector2 potentialPosition = new(randomX, randomY);

                if (IsValidSpawnPosition(potentialPosition, playerStartPosition))
                {
                    SpawnEnemyAt(potentialPosition);
                    _spawnedPositions.Add(potentialPosition);
                    positionFound = true;
                    break;
                }
            }

            if (!positionFound)
                Debug.LogWarning($"EnemyManager: Could not find a valid spawn position for enemy {i + 1} after {maxSpawnAttemptsPerEnemy} attempts.");
        }
    }

    /// <summary>
    /// Resets all managed enemies (children of this GameObject) to their respective starting states and positions.
    /// Typically called when the player respawns after losing a life.
    /// </summary>
    public void GameRestart()
    {
        Debug.Log("EnemyManager: Resetting all enemies.");
        foreach (Transform child in transform)
        {
            EnemyMovement enemyMovement = child.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.GameRestart();
            }
            else
            {
                Debug.LogWarning($"Child {child.name} does not have EnemyMovement component for reset.");
            }
        }
    }

    /// <summary>
    /// Checks if a potential position is suitable for spawning an enemy.
    /// </summary>
    /// <param name="position">The potential spawn position to check.</param>
    /// <param name="playerPos">The player's current position.</param>
    /// <returns>True if the position is valid, false otherwise.</returns>
    private bool IsValidSpawnPosition(Vector2 position, Vector2 playerPosition)
    {
        // Check 1: Minimum distance from player.
        if (playerTransform != null && Vector2.Distance(position, playerPosition) < minDistanceFromPlayer)
            return false;

        // Check 2: Minimum distance from other already spawned enemies.
        foreach (Vector2 spawnedPosition in _spawnedPositions)
            if (Vector2.Distance(position, spawnedPosition) < minSpawnSeperation)
                return false;

        // Check 3: Collision check (avoid spawning inside walls/ground).
        float checkRadius = 0.4f;
        if (Physics2D.OverlapCircle(position, checkRadius, obstacleLayerMask))
            return false;

        return true;
    }

    /// <summary>
    /// Instantiates a single enemy prefab at the specified position.
    /// </summary>
    /// <param name="position">The world position where the enemy should be spawned.</param>
    private void SpawnEnemyAt(Vector2 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyManager: Enemy Prefab is not assigned!");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, position, UnityEngine.Quaternion.identity, transform);
        newEnemy.name = $"{enemyPrefab.name}_{_spawnedPositions.Count}";
    }

    /// <summary>
    /// Destroys all enemy GameObjects that are children of this manager.
    /// </summary>
    private void ClearAllEnemies()
    {
        Debug.Log("EnemyManager: Clearing all existing enemies.");
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        _spawnedPositions.Clear();
    }
}