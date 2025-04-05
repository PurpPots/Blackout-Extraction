using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimerUI : MonoBehaviour
{
    public static LevelTimerUI Instance { get; private set; }

    public TextMeshProUGUI timerDisplay; // Assign in Inspector
    public TextMeshProUGUI objectiveDisplay; // Assign in Inspector
    public TextMeshProUGUI bonusEXPText; // Displays "Bonus EXP Time:"
    public Slider bonusEXPSlider; // Displays remaining bonus EXP time

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

        bonusEXPText.gameObject.SetActive(false);
        bonusEXPSlider.gameObject.SetActive(false);
    }

    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerDisplay.text = $"Time: {minutes:00}:{seconds:00}"; // Format MM:SS
    }

    public void SetTimerToReady()
    {
        objectiveDisplay.text = "Level Up Item: READY!"; // Show when item is available
    }

    public void UpdateObjective(string objectiveText)
    {
        objectiveDisplay.text = objectiveText;
    }

    public void StartBonusEXPCountdown(float duration)
    {
      //  Debug.Log("Bonus EXP Countdown Started!");
        bonusEXPText.gameObject.SetActive(true);
        bonusEXPSlider.gameObject.SetActive(true);
        StartCoroutine(BonusEXPTimer(duration));
    }

    private IEnumerator BonusEXPTimer(float duration)
    {
     //   print($"time left {duration}");
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            bonusEXPSlider.value = timeLeft / duration; // Update slider progress
//            print($"slider val {bonusEXPSlider.value}");
            yield return null;
        }

        bonusEXPText.gameObject.SetActive(false);
        bonusEXPSlider.gameObject.SetActive(false);
    }
    public void ClearTimer()
    {
        timerDisplay.text = ""; // Hide timer display
    }

    public void GotBonusEXPUIHandler()
    {
        bonusEXPText.gameObject.SetActive(false);
        bonusEXPSlider.gameObject.SetActive(false);
    }
}
