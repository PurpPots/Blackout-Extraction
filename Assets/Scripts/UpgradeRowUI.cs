using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UpgradeRowUI : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI statNumberText;

    public Button plusButton, minusButton;
    public Image baseBar;
    public Image upgradeOverlayBar;

    public string upgradeName;
    public int maxBarUnits = 10; // Max width steps
    private int costPerUpgrade;
    public float barUnitWidth = 5f; // Width of 1 stat step
    private float statValuePerUpgrade; // How much 1 upgrade adds
    public float baseWidth = 100f;
    private float currentValue;
    private int selectedCount;
    private int finalStat;
    private float acquiredBarWidth;
    private float barStore;
    public float fillSpeed = 300f; // pixels per second



    private void Start()
    {
        plusButton.onClick.AddListener(Increase);
        minusButton.onClick.AddListener(Decrease);

        acquiredBarWidth = baseWidth;


        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        UpgradeMenuManager manager = FindFirstObjectByType<UpgradeMenuManager>();

        if (player != null && manager != null)
        {
            costPerUpgrade = manager.GetCostPerUpgrade(upgradeName); // Fetch cost dynamically

            switch (upgradeName)
            {
                case "Damage":
                    currentValue = Mathf.RoundToInt(player.GetBulletDamage());
                    statValuePerUpgrade = manager.GetDamagePerUpgrade();
                    break;
                case "FireRate":
                    currentValue = Mathf.RoundToInt(player.GetFireRate());
                    statValuePerUpgrade = manager.GetFireRatePerUpgrade();
                    break;
                case "MaxHealth":
                    currentValue = Mathf.RoundToInt(player.GetMaxHealth());
                    statValuePerUpgrade = manager.GetHealthPerUpgrade();
                    break;
                default:
                    Debug.LogWarning($"Unknown stat key: {upgradeName}");
                    currentValue = 0;
                    break;
            }
        }
        else
        {
            Debug.LogError("PlayerStats or UpgradeMenuManager not found!");
        }
    }



    public void Increase()
    {
        selectedCount++;
        UpdateCountText();
        UpdateBars();
        FindFirstObjectByType<UpgradeMenuManager>()?.NotifyUpgradeChange(upgradeName, selectedCount);
    }

    public void Decrease()
    {
        if (selectedCount > 0) selectedCount--;
        UpdateCountText();
        UpdateBars();
        FindFirstObjectByType<UpgradeMenuManager>()?.NotifyUpgradeChange(upgradeName, selectedCount);

    }

    private void UpdateCountText()
    {
        int totalCost = selectedCount * costPerUpgrade;
        countText.text = $"Price:  {costPerUpgrade} EXP  x  {selectedCount}  =  {totalCost} EXP";
    }


    private void UpdateBars()
    {
        // Calculate bar widths
        float pendingUpgradeWidth = selectedCount * barUnitWidth;
        float totalBarWidth = acquiredBarWidth + pendingUpgradeWidth;

        // Apply width to bars
        baseBar.rectTransform.sizeDelta = new Vector2(acquiredBarWidth, baseBar.rectTransform.sizeDelta.y);
        upgradeOverlayBar.rectTransform.sizeDelta = new Vector2(totalBarWidth, upgradeOverlayBar.rectTransform.sizeDelta.y);
        barStore = totalBarWidth;

        //Update stat number text with preview
        finalStat = Mathf.RoundToInt(currentValue + (selectedCount * statValuePerUpgrade));
        if (selectedCount > 0)
            statNumberText.text = $"{currentValue} -> {finalStat}";
        else
            statNumberText.text = $"{currentValue}";
    }


    public int GetUpgradeCount()
    {
        return selectedCount;
    }

    public IEnumerator AnimateBarFill(float targetWidth)
    {
        float currentWidth = baseBar.rectTransform.sizeDelta.x;

        while (currentWidth < targetWidth)
        {
            currentWidth += fillSpeed * Time.unscaledDeltaTime;
            if (currentWidth > targetWidth) currentWidth = targetWidth;

            baseBar.rectTransform.sizeDelta = new Vector2(currentWidth, baseBar.rectTransform.sizeDelta.y);
            yield return null;
        }

        acquiredBarWidth = targetWidth; 
    }


    public void CommitUpgrades()
    {
        acquiredBarWidth = barStore;
        StartCoroutine(AnimateBarFill(acquiredBarWidth));
        currentValue = finalStat;
        selectedCount = 0;
        UpdateBars();
    }

}