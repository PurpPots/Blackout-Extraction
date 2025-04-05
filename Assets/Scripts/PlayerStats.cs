using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 1f;

    [Header("Upgrades System")]
    private float healthPerUpgrade;
    private float damagePerUpgrade;
    private float staminaPerUpgrade;
    private float fireRatePerUpgrade;

    [Header("EXP System")]
    private float currentEXP = 0;
    private float zombieKillExp = 5f;
    private float miniBossKillExp = 20f;

    [Header("UI References")]
    public TextMeshProUGUI EXPText;
    public TextMeshProUGUI EXPAddedText;
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    public Slider expSlider;
    public TextMeshProUGUI xpPopupText;
    public TextMeshProUGUI healthPopupText;
    public TextMeshProUGUI ammoPopupText;
    public float FadeDuration = 2.5f;
    private Coroutine xpFadeCoroutine;
    private Coroutine healthFadeCoroutine;
    private Coroutine ammoFadeCoroutine;

    public Image damageOverlay; 
    public float fadeDuration = 1f;

    public GameObject headShotEffect;

    public static PlayerStats Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void Start()
    {
        currentHealth = maxHealth;
        InitializeHealthUI();
        UpdateHealthUI();
        UpdateEXPUI();
        healthPerUpgrade = FindFirstObjectByType<UpgradeMenuManager>().GetHealthPerUpgrade();
        damagePerUpgrade = FindFirstObjectByType<UpgradeMenuManager>().GetDamagePerUpgrade();
        staminaPerUpgrade = FindFirstObjectByType<UpgradeMenuManager>().GetStaminaPerUpgrade();
        fireRatePerUpgrade = FindFirstObjectByType<UpgradeMenuManager>().GetFireRatePerUpgrade();

        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 0; 
            damageOverlay.color = overlayColor;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnUpgradesCompleted += ApplyUpgrades;
    }

    private void OnDisable()
    {
        GameEvents.OnUpgradesCompleted -= ApplyUpgrades;
    }

    private void InitializeHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;  // Set max value of health bar dynamically
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (damageOverlay != null)
        {
            StartCoroutine(FadeDamageOverlay());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FadeDamageOverlay()
    {
        Color overlayColor = damageOverlay.color;
        overlayColor.a = 0.1f;
        damageOverlay.color = overlayColor;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            overlayColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            damageOverlay.color = overlayColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        overlayColor.a = 0f; // Fully invisible at the end
        damageOverlay.color = overlayColor;
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        SceneManager.LoadScene("GameOver");
    }

    public void UpdateTypeUI(string type)
    {
        float gainedEXP = 0;

        switch (type)
        {
            case "zombie":
                gainedEXP = zombieKillExp;
                break;
            case "miniBoss":
                gainedEXP = miniBossKillExp;
                break;
            case "levelupBonus":
                gainedEXP = 50;
                Debug.Log("LevelUpItem Bonus: +50 EXP");
                break;
        }

        currentEXP += gainedEXP;
        UpdateEXPUI();
        ShowNotification(gainedEXP, "exp");
    }

    private void ShowNotification(float amt, string type)
    {
        if (xpPopupText != null && type == "exp") //EXP
        {
            xpPopupText.text = $"+{amt} XP";
            xpPopupText.alpha = 1;

            if (xpFadeCoroutine != null) StopCoroutine(xpFadeCoroutine);
            xpFadeCoroutine = StartCoroutine(FadeOutText(xpPopupText));
        }
        else if (healthPopupText != null && type == "health") //Health
        {
            healthPopupText.text = $"+{amt} Health";
            healthPopupText.alpha = 1;

            if (healthFadeCoroutine != null) StopCoroutine(healthFadeCoroutine);
            xpFadeCoroutine = StartCoroutine(FadeOutText(healthPopupText));
        }
        else if (ammoPopupText != null && type == "ammo") //Health
        {
            ammoPopupText.text = $"+{amt} Ammo";
            ammoPopupText.alpha = 1;

            if (ammoFadeCoroutine != null) StopCoroutine(ammoFadeCoroutine);
            ammoFadeCoroutine = StartCoroutine(FadeOutText(ammoPopupText));
        }
    }

    private void ApplyUpgrades(List<UpgradeOptionData> upgrades)
    {
        int totalSpentEXP = 0;

        foreach (var upgrade in upgrades)
        {
            if (upgrade.count <= 0) continue;

            int cost = upgrade.cost * upgrade.count;
            totalSpentEXP += cost;

            switch (upgrade.upgradeName)
            {
                case "Damage":
                    damage += damagePerUpgrade * upgrade.count;
                    Debug.Log($"Damage Increased: {damage}");
                    break;
                case "FireRate":
                    fireRate += fireRatePerUpgrade * upgrade.count;
                    Debug.Log($"fire Rate Increased: {fireRate}");
                    break;
                case "MaxHealth":
                    maxHealth += healthPerUpgrade * upgrade.count;
                    Debug.Log($"Max Health Increased: {maxHealth}");
                    UpdateHealthUI();
                    break;
            }
        }

        // Subtract spent EXP
        currentEXP -= totalSpentEXP;
        currentEXP = Mathf.Max(0, currentEXP); // Clamp to prevent negative

        Debug.Log($"EXP Spent: {totalSpentEXP}, Remaining EXP: {currentEXP}");

        UpdateEXPUI();
    }



    void UpdateEXPUI()
    {
        if (EXPText != null)
        {
            EXPText.text = $"EXP:                              {currentEXP}";
        }

        if (expSlider != null)
        {
            expSlider.value = currentEXP;
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Health :                             {currentHealth}/{maxHealth}";
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private IEnumerator FadeOutText(TextMeshProUGUI txt)
    {
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = txt.GetComponent<CanvasGroup>();

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / FadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
    }

    public float GetBulletDamage()
    {
        return damage;
    }
    public float GetFireRate()
    {
        return fireRate;
    }
    public int GetEXP()
    {
        return Mathf.FloorToInt(currentEXP); // or just (int)currentEXP;
    }
    public int GetMaxHealth()
    {
        return Mathf.FloorToInt(maxHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure it doesn't exceed maxHealth
        UpdateHealthUI();
        ShowNotification(amount, "health");
        Debug.Log($"Player healed by {amount}. Current health: {currentHealth}");
    }

    public void addAmmo(float amount)
    {
        ShowNotification(amount, "ammo");
    }

    public void ShowHeadshotEffect()
    {
        Debug.Log("showing head shot effect");
        StartCoroutine(HeadshotEffectRoutine());
    }

    private IEnumerator HeadshotEffectRoutine()
    {
        headShotEffect.SetActive(true);

        // If it has an Image or SpriteRenderer, fade it out
        Image img = headShotEffect.GetComponent<Image>();
        //  SpriteRenderer sr = headShotEffect.GetComponent<SpriteRenderer>();

        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            if (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        headShotEffect.SetActive(false); // Deactivate after fading
    }

}