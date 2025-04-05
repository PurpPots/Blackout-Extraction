using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager Instance { get; private set; }

    [Header("Game Level Settings")]
    public int currentGameLevel = 1;
    public int maxGameLevel = 3;
    public TextMeshProUGUI currLevelDisplayText;

    [Header("Level Up Item Settings")]
    public GameObject levelUpItemPrefab;
    private GameObject currentLevelUpItem;
    public float levelUpItemSpawnDelay = 10f;
    private bool levelUpItemSpawned = false;
    public List<Transform> levelUpItemSpawnPoints;

    [Header("Mini-Boss & Final Boss Settings")]

    private float levelUpItemSpawnTime;
    private const float bonusExpThreshold = 10f;
    private const int bonusExpAmount = 50;
    private bool gotEXPBonus = false;

    private PlayerStats playerStats;
    private LevelTimerUI levelTimerUI;
    private UpgradeMenuManager upgradeMenuManager;

    public delegate void GameLevelUpAction(int newLevel);
    public static event GameLevelUpAction OnGameLevelUp;

    public delegate void MiniBossSpawnAction();
    public static event MiniBossSpawnAction OnMiniBossSpawn;

    public delegate void FinalBossSpawnAction();
    public static event FinalBossSpawnAction OnFinalBossSpawn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        levelTimerUI = FindFirstObjectByType<LevelTimerUI>();
        upgradeMenuManager = FindFirstObjectByType<UpgradeMenuManager>();

        if (playerStats == null) Debug.LogError("PlayerStats not found");
        if (levelTimerUI == null) Debug.LogError("LevelTimerUI not found");
        if (upgradeMenuManager == null) Debug.LogError("UpgradeMenuManager not found");

        currLevelDisplayText.gameObject.SetActive(true);
        currLevelDisplayText.text = $"Level: {currentGameLevel}";

        StartCoroutine(LevelUpItemSpawnTimer());
    }

    private void OnEnable()
    {
        GameEvents.OnLevelUpItemCollected += HandleUpgrade;
        GameEvents.OnMiniBossDefeated += ProgressGameLevel;
        GameEvents.OnUpgradesCompleted += SpawnBossAfterUpgrade; //Listen for upgrade completion
        GameEvents.OnFinalBossDefeated += HandleFinalBossDefeated;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelUpItemCollected -= HandleUpgrade;
        GameEvents.OnMiniBossDefeated -= ProgressGameLevel;
        GameEvents.OnUpgradesCompleted -= SpawnBossAfterUpgrade;
        GameEvents.OnFinalBossDefeated -= HandleFinalBossDefeated;
    }

    private IEnumerator LevelUpItemSpawnTimer()
    {
        float timeRemaining = levelUpItemSpawnDelay;
        levelTimerUI.UpdateObjective("Objective: Survive");

        while (timeRemaining > 0)
        {
            levelTimerUI.UpdateTimer(timeRemaining);
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        StartCoroutine(BonusEXPCountdown());
        SpawnLevelUpItem();
        StartCoroutine(HighlightLevelUpItem());
    }

    public void SpawnLevelUpItem()
    {
        if (!levelUpItemSpawned && levelUpItemPrefab != null)
        {
            Transform spawnPoint = GetRandomSpawnPoint();

            if (spawnPoint != null)
            {
                currentLevelUpItem = Instantiate(levelUpItemPrefab, spawnPoint.position, Quaternion.identity);
                levelUpItemSpawned = true;
                levelUpItemSpawnTime = Time.time;

                levelTimerUI.UpdateObjective("Objective: Collect the item");
                levelTimerUI.ClearTimer();
                StartCoroutine(BonusEXPCountdown());

             //   Debug.Log($" LevelUpItem Spawned at: {spawnPoint.position}");
            }
            else
            {
                Debug.LogError(" No spawn points assigned for LevelUpItem");
            }
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        if (levelUpItemSpawnPoints == null || levelUpItemSpawnPoints.Count == 0)
            return null;

        return levelUpItemSpawnPoints[Random.Range(0, levelUpItemSpawnPoints.Count)];
    }

    private IEnumerator BonusEXPCountdown()
    {
        float timeRemaining = bonusExpThreshold;
        if (LevelTimerUI.Instance == null)
        {
            Debug.LogError("LevelTimerUI.Instance is NULL");
            yield break;
        }

        LevelTimerUI.Instance.StartBonusEXPCountdown(timeRemaining);

        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }
    }

    private IEnumerator HighlightLevelUpItem()
    {
        yield return new WaitForSeconds(bonusExpThreshold);

        if (currentLevelUpItem != null)
        {
            Renderer renderer = currentLevelUpItem.GetComponent<Renderer>();
            if (renderer != null)
            {
                Outline outline = currentLevelUpItem.GetComponent<Outline>();

                if (outline != null)
                {
                    outline.enabled = true; // Enable the outline effect
              //      Debug.Log(":sparkles: Outline enabled on LevelUpItem");
                }
                else
                {
                    Debug.LogError("No Outline component found on LevelUpItem");
                }
            }
        }
    }


    private void HandleUpgrade()
    {
        float timeSinceSpawn = Time.time - levelUpItemSpawnTime;

        if (playerStats != null && timeSinceSpawn <= bonusExpThreshold)
        {
            gotEXPBonus = true;
            playerStats.UpdateTypeUI("levelupBonus");
            LevelTimerUI.Instance.GotBonusEXPUIHandler();
            Debug.Log($"Bonus EXP Awarded: {bonusExpAmount}");
        }
        else
        {
            gotEXPBonus = false;
            Debug.Log("LevelUpItem collected, but no bonus EXP");
        }

        LevelTimerUI.Instance.UpdateObjective("Objective: Choose an upgrade");
        upgradeMenuManager.ShowUpgradeMenu(); // ⏸Pause game and show upgrade menu
    }

    private void SpawnBossAfterUpgrade(List<UpgradeOptionData> upgrades)
    {
        Debug.Log("Upgrade completed! Spawning Mini-Boss");

        if (currentGameLevel < maxGameLevel)
        {
            Debug.Log("Spawning Mini-Boss");
            LevelTimerUI.Instance.UpdateObjective("Objective: Defeat the mini boss");
            OnMiniBossSpawn?.Invoke();
        }
        else
        {
            Debug.Log("Spawning Final-Boss...");
            LevelTimerUI.Instance.UpdateObjective("Objective: Defeat the final boss");
            OnFinalBossSpawn?.Invoke();
        }

    }

    private void ProgressGameLevel()
    {
            currentGameLevel++;
            Debug.Log($"Level Increased → Now Level {currentGameLevel}");

            OnGameLevelUp?.Invoke(currentGameLevel);
            ZombieSpawnController.Instance.IncreaseZombieDifficulty();

            levelUpItemSpawned = false;
            levelTimerUI.UpdateObjective("Objective: Survive");
            currLevelDisplayText.text = $"Level: {currentGameLevel}";

            StartCoroutine(LevelUpItemSpawnTimer());
    }
    private void HandleFinalBossDefeated()
    {
        Debug.Log("Final boss defeated! Returning to main scene");

        // Optional: add delay or fade effect here
        SceneManager.LoadScene("GameComplete"); 
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

    }

}
