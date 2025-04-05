using TMPro;
using UnityEngine;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance { get; set; }

    public TextMeshProUGUI ammoDisplay;
    public TextMeshProUGUI totalAmmoDisplay;
    public int totalAmmo = 20;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        UpdateUI();
    }

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        UpdateUI();
    }

    public void UseAmmo(int amount)
    {
        totalAmmo -= amount;
        if (totalAmmo < 0) totalAmmo = 0;  //Prevent negative ammo
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (totalAmmoDisplay != null)
        {
            totalAmmoDisplay.text = $"{totalAmmo}";  //Update total ammo UI
        }
    }
}
