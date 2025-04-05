using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [SerializeField] public float healthAmount = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.Heal(healthAmount);
                Destroy(gameObject);
            }
        }
    }
}
