using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class UpgradeMenuManager : MonoBehaviour
{
    public TextMeshProUGUI availableEXPText, totalCostText, remainingEXPText;
    public Button readyButton;
    public List<UpgradeRowUI> upgradeRows; // Assign in Inspector or during setup

    private int availableEXP;
    public GameObject upgradePanel;
    public GameObject playerStatsMenu;
    public List<UpgradeOptionData> upgradeOptions = new List<UpgradeOptionData>();
    public float healthPerUpgrade;
    public float damagePerUpgrade;
    public float staminaPerUpgrade;
    public float fireRatePerUpgrade;

    private void Start()
    {
      //  Debug.Log("UpgradeMenuManager: Start()");
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyPressed);
        }
        else
        {
            Debug.LogError("Ready button not assigned!");
        }
    }

    public void ShowUpgradeMenu()
    {
        Debug.Log("UpgradeMenuManager: ShowUpgradeMenu()");
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        playerStatsMenu.SetActive(false);
        upgradePanel.SetActive(true);
        Weapon gunShoot = FindObjectOfType<Weapon>();
        if (gunShoot != null) gunShoot.readyToShoot = false;

        availableEXP = PlayerStats.Instance.GetEXP();
       // Debug.Log("Available EXP: " + availableEXP);

        ResetUpgradeSelections();
        UpdateUI();
    }

    public void Increase(string upgradeName)
    {
        Debug.Log($"Increase({upgradeName}) called");

        UpgradeOptionData option = upgradeOptions.Find(u => u.upgradeName == upgradeName);
        if (option != null)
        {
            if (GetTotalCost() + option.cost <= availableEXP)
            {
                option.count++;
                Debug.Log($"Upgrade '{upgradeName}' increased to {option.count}");
                UpdateUI();
            }
            else
            {
                Debug.Log("Not enough EXP to increase upgrade");
            }
        }
    }

    public void Decrease(string upgradeName)
    {
        Debug.Log($"Decrease({upgradeName}) called");

        UpgradeOptionData option = upgradeOptions.Find(u => u.upgradeName == upgradeName);
        if (option != null && option.count > 0)
        {
            option.count--;
            Debug.Log($"Upgrade '{upgradeName}' decreased to {option.count}");
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        Debug.Log("UpgradeMenuManager: UpdateUI()");

        int total = GetTotalCost();
        Debug.Log($"Total Cost: {total}");

        if (availableEXPText != null) availableEXPText.text = $"Available EXP: {availableEXP}";
        if (totalCostText != null) totalCostText.text = $"Total Cost: {total}";
        if (remainingEXPText != null) remainingEXPText.text = $"Remaining EXP: {availableEXP - total}";

        if (readyButton != null)
        {
            readyButton.interactable = total <= availableEXP;
            bool canUpgrade = total <= availableEXP;
            Debug.Log("Ready button interactable: " + readyButton.interactable);

            // Change text alpha when disabled
            TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                Color textColor = buttonText.color;
                textColor.a = canUpgrade ? 1f : 0.2f;  // Full opacity when enabled, 50% when disabled
                buttonText.color = textColor;
            }
        }
        else
        {
            Debug.LogWarning("Ready button is null during UpdateUI");
        }
    }

    private int GetTotalCost()
    {
        int total = 0;
        foreach (var option in upgradeOptions)
        {
            total += option.TotalCost;
            Debug.Log($"Option: {option.upgradeName}, Count: {option.count}, TotalCost: {option.TotalCost}");
        }
        return total;
    }

    public void OnReadyPressed()
    {
        Debug.Log("UpgradeMenuManager: OnReadyPressed()");
        StartCoroutine(ApplyUpgradesAndCloseMenu());
    }

    private IEnumerator ApplyUpgradesAndCloseMenu()
    {
        if (GetTotalCost() <= availableEXP)
        {
            Debug.Log("Enough EXP. Triggering upgrades...");
            GameEvents.TriggerUpgradesCompleted(upgradeOptions);

            foreach (var row in upgradeRows)
            {
                row.CommitUpgrades();
                Debug.Log($"Committed upgrades for: {row.upgradeName}");
            }

            // Wait a bit to visually animate bar filling or add delay before closing
            yield return new WaitForSecondsRealtime(1f);

            CloseMenu();
        }
        else
        {
            Debug.LogWarning("OnReadyPressed called but not enough EXP");
        }
    }


    private void CloseMenu()
    {
        Debug.Log("UpgradeMenuManager: CloseMenu()");
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Weapon gunShoot = FindObjectOfType<Weapon>();
        if (gunShoot != null) gunShoot.readyToShoot = true;

        if (upgradePanel != null)
        {
            playerStatsMenu.SetActive(true);
            upgradePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Upgrade panel is null!");
        }
    }

    private void ResetUpgradeSelections()
    {
        Debug.Log("UpgradeMenuManager: ResetUpgradeSelections()");
        foreach (var option in upgradeOptions)
        {
            Debug.Log($"Resetting: {option.upgradeName}");
            option.count = 0;
        }
    }

    public void NotifyUpgradeChange(string upgradeName, int newCount)
    {
        Debug.Log($"NotifyUpgradeChange: {upgradeName} → {newCount}");

        UpgradeOptionData option = upgradeOptions.Find(u => u.upgradeName == upgradeName);
        if (option != null)
        {
            option.count = newCount;
            UpdateUI();
        }
    }

    public int GetCostPerUpgrade(string upgradeName)
    {
        UpgradeOptionData option = upgradeOptions.Find(u => u.upgradeName == upgradeName);
        if (option != null)
        {
            return option.cost;
        }
        Debug.LogWarning($"Cost not found for upgrade: {upgradeName}");
        return 0;
    }


    public float GetHealthPerUpgrade()
    {
        return Mathf.FloorToInt(healthPerUpgrade);
    }
    public float GetDamagePerUpgrade()
    {
        return Mathf.FloorToInt(damagePerUpgrade);
    }
    public float GetStaminaPerUpgrade()
    {
        return Mathf.FloorToInt(staminaPerUpgrade);
    }
    public float GetFireRatePerUpgrade()
    {
        return Mathf.FloorToInt(fireRatePerUpgrade);
    }
}