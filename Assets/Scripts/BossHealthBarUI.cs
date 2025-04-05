using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public Slider healthSlider;
    public GameObject bossBarRoot; //The whole UI bar root (to toggle on/off)


    public void ShowBar()
    {
        bossBarRoot.SetActive(true);
        healthSlider.value = 1;
       // print("Attempted to show");
    }

    public void HideBar()
    {
        bossBarRoot.SetActive(false);
    }

    public void SetHealth(float current, float max)
    {
        healthSlider.value = current / max;
    }
}
