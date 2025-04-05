using System;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; set; }

    private GameObject hoveredCollectible = null;

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
    }

    private void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objectHitByRaycast = hit.transform.gameObject;

            if (objectHitByRaycast.CompareTag("Collectible"))
            {
                // Disable outline for previously hovered collectible
                if (hoveredCollectible && hoveredCollectible != objectHitByRaycast)
                {
                    hoveredCollectible.GetComponent<Outline>().enabled = false;
                }

                // Enable outline for the new collectible
                hoveredCollectible = objectHitByRaycast;
                hoveredCollectible.GetComponent<Outline>().enabled = true;

                if (Input.GetKeyDown(KeyCode.F))
                {
                    PickupCollectible(objectHitByRaycast);
                    Destroy(objectHitByRaycast);
                }
            }
            else
            {
                // Remove outline from previously hovered collectible if not looking at a collectible anymore
                if (hoveredCollectible)
                {
                    hoveredCollectible.GetComponent<Outline>().enabled = false;
                    hoveredCollectible = null;
                }
            }
        }
    }

    private void PickupCollectible(GameObject collectible)
    {
      //  print($"Picked up {collectible.name}");

        // Check if the collectible has a specific script for handling pickup behavior
        if (collectible.TryGetComponent<AmmoBox>(out AmmoBox ammoBox))
        {
            // Handle ammo pickup
            AmmoManager.Instance.AddAmmo(10);
            PlayerStats.Instance.addAmmo(10);
            print($"Collected Ammo: {ammoBox.ammoAmount}");
        }
        else if (collectible.TryGetComponent<LevelUpItem>(out LevelUpItem levelUpItem))
        {
            // Handle level-up item pickup
            print("Collected Level-Up Item!");
            GameEvents.OnLevelUpItemCollected?.Invoke(); // Trigger level-up event
        }
        else if (collectible.TryGetComponent<HealthPack>(out HealthPack healthPack))
        {
            print("Collected health pack");
            PlayerStats.Instance.Heal(healthPack.healthAmount);
        }
    }
}
