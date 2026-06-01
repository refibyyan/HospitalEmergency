using UnityEngine;

public class ExitTriggerHelper : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah objek yang menyentuh pintu punya Tag "Player"
        if (collision.CompareTag("Player"))
        {
            LevelGameplayManager manager = FindObjectOfType<LevelGameplayManager>();
            if (manager != null)
            {
                // Kirim GameObject player yang menabrak trigger ini ke manager
                manager.PlayerReachedExit(collision.gameObject);
            }
        }
    }
}