using UnityEngine;

public class BossSpawnController : MonoBehaviour
{
    [Header("Mini-Boss Settings")]
    [SerializeField] private GameObject miniBossPrefab; //Single mini-boss type
    [SerializeField] private Transform miniBossSpawnPoint; // pawn location

    [Header("Final Boss Settings")]
    [SerializeField] private GameObject finalBossPrefab; //Final boss prefab
    [SerializeField] private Transform finalBossSpawnPoint; //spawn location

    private int miniBossSpawnCount = 0; // Tracks how many times the mini-boss has spawned

    private void OnEnable()
    {
        GameLevelManager.OnMiniBossSpawn += HandleMiniBossSpawn;
        GameLevelManager.OnFinalBossSpawn += HandleFinalBossSpawn;
    }

    private void OnDisable()
    {
        GameLevelManager.OnMiniBossSpawn -= HandleMiniBossSpawn;
        GameLevelManager.OnFinalBossSpawn -= HandleFinalBossSpawn;
    }

    private void HandleMiniBossSpawn()
    {
        if (GameLevelManager.Instance == null)
        {
            Debug.LogError("GameLevelManager Instance is NULL! Cannot spawn Mini-Boss");
            return;
        }

        if (miniBossSpawnCount < 2) // Only spawn mini-boss twice
        {
            SpawnMiniBoss();
            miniBossSpawnCount++;
        }
        else
        {
            Debug.Log("Mini-Boss Spawn Limit Reached! Preparing for Final Boss");
           // GameLevelManager.TriggerFinalBossSpawn(); // Triggers final boss event
        }
    }

    private void SpawnMiniBoss()
    {
        if (miniBossPrefab != null && miniBossSpawnPoint != null)
        {
            Instantiate(miniBossPrefab, miniBossSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Mini-Boss Prefab or Spawn Point is NULL");
        }
    }

    private void HandleFinalBossSpawn()
    {
        if (finalBossPrefab != null && finalBossSpawnPoint != null)
        {
            Instantiate(finalBossPrefab, finalBossSpawnPoint.position, Quaternion.identity);
            Debug.Log("Final Boss Spawned");
        }
        else
        {
            Debug.LogError("Final Boss Prefab or Spawn Point is NULL");
        }
    }
}
