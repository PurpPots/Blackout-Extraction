using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnController : MonoBehaviour
{
    public static ZombieSpawnController Instance { get; private set; }

    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public float spawnDelay = 1f; //Time between spawns
    public int maxZombiesAlive = 15; //Global zombie intensity
    public float zombieHealthMultiplier = 1.0f; //Difficulty scaling
    public float spawnCooldown = 3f; //Time before a spawn point can be reused

    [Header("Spawn Points")]
    public List<Transform> spawnPoints; // assign multiple spawn locations
    private Dictionary<Transform, float> spawnCooldowns = new Dictionary<Transform, float>(); //Tracks cooldowns

    private List<Zombie> currentZombiesAlive = new List<Zombie>();
    private bool isSpawning = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(SpawnZombies());
    }

    private IEnumerator SpawnZombies()
    {
        while (isSpawning)
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogError("No spawn points assigned in ZombieSpawnController");
                yield break; // Stop the coroutine to prevent errors
            }

            if (currentZombiesAlive.Count < maxZombiesAlive)
            {
                Transform spawnPoint = GetValidSpawnPoint(); // Find a hidden spawn point
                if (spawnPoint != null)
                {
                    GameObject zombie = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
                    Zombie enemyScript = zombie.GetComponent<Zombie>();
                    currentZombiesAlive.Add(enemyScript);

                    // Add cooldown for this spawn point
                    spawnCooldowns[spawnPoint] = Time.time + spawnCooldown;
                }
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void Update()
    {
        RemoveDeadZombies();
    }

    public void RemoveDeadZombies()
    {
        currentZombiesAlive.RemoveAll(z => z == null);
    }

    // Finds a spawn point that is NOT visible to the player & is NOT on cooldown
    private Transform GetValidSpawnPoint()
    {
        Transform bestSpawn = null;
        float bestDistance = 0f;
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        List<Transform> shuffledSpawnPoints = new List<Transform>(spawnPoints);
        ShuffleList(shuffledSpawnPoints);

        foreach (Transform spawnPoint in shuffledSpawnPoints)
        {
            if (!IsVisibleToPlayer(spawnPoint, playerTransform) && !IsSpawnPointOnCooldown(spawnPoint))
            {
                float distance = Vector3.Distance(spawnPoint.position, playerTransform.position);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestSpawn = spawnPoint;
                }
            }
        }

        return bestSpawn != null ? bestSpawn : shuffledSpawnPoints[0]; // Default to any spawn if all are visible
    }

    //Checks if the spawn point is visible to the player
    private bool IsVisibleToPlayer(Transform spawnPoint, Transform player)
    {
        Vector3 directionToPlayer = (player.position - spawnPoint.position).normalized;
        float distanceToPlayer = Vector3.Distance(spawnPoint.position, player.position);
        
        RaycastHit hit;
        if (Physics.Raycast(spawnPoint.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    // Checks if the spawn point is on cooldown
    private bool IsSpawnPointOnCooldown(Transform spawnPoint)
    {
        if (spawnCooldowns.ContainsKey(spawnPoint) && Time.time < spawnCooldowns[spawnPoint])
        {
            return true;
        }
        return false;
    }

    // Randomizes the order of spawn points to prevent predictable patterns
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Controls zombie intensity dynamically during different phases
    public void SetWaveIntensity(bool thin)
    {
        maxZombiesAlive = thin ? 5 : 15;
        spawnDelay = thin ? 2.0f : 0.5f;

        Debug.Log($"Zombie wave intensity set to: {(thin ? "THINNER" : "FULL")}");
    }

    // Called when the level progresses (Mini-Boss defeated)
    public void IncreaseZombieDifficulty()
    {
        zombieHealthMultiplier += 0.2f;
        maxZombiesAlive += 2;
        Debug.Log($"Zombies are now stronger! Health x{zombieHealthMultiplier}");
    }
}
