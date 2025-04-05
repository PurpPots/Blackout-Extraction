using UnityEngine;

public class BulletDecal : MonoBehaviour
{
    public float decalLifetime = 2f; // Time before the decal is destroyed

    private void Start()
    {
        Destroy(gameObject, decalLifetime); // Destroy the decal after X seconds
    }
}
