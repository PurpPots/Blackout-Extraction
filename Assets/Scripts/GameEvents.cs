using System;
using UnityEngine;
using System.Collections.Generic;

public static class GameEvents
{
    public static Action OnLevelUpItemCollected;
    public static Action<GameObject> OnCollectiblePickedUp;
    public static Action OnMiniBossDefeated;
    public static Action<List<UpgradeOptionData>> OnUpgradesCompleted;

    public static Action OnFinalBossDefeated;

    public static void TriggerLevelUpItemCollected()
    {
        OnLevelUpItemCollected?.Invoke();
    }

    public static void TriggerCollectiblePickedUp(GameObject collectible)
    {
        OnCollectiblePickedUp?.Invoke(collectible);
    }

    public static void TriggerMiniBossDefeated()
    {
        Debug.Log("Mini-Boss Defeated Event");
        OnMiniBossDefeated?.Invoke();
    }

    public static void TriggerUpgradesCompleted(List<UpgradeOptionData> upgrades)
    {
        OnUpgradesCompleted?.Invoke(upgrades);
    }

    // ✅ NEW
    public static void TriggerFinalBossDefeated()
    {
        Debug.Log("Final Boss Defeated Event");
        OnFinalBossDefeated?.Invoke();
    }
}
