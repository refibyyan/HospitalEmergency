using UnityEngine;

public class ExitTriggerHelper : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Script ini akan mengecek: Apakah objek yang menyentuh pintu punya Tag "Player"?
        if (collision.CompareTag("Player"))
        {
            // Jika benar Player (Lyra), cari GameplayManager di scene dan panggil fungsi pindah scene
            LevelGameplayManager manager = FindObjectOfType<LevelGameplayManager>();
            if (manager != null)
            {
                manager.PlayerReachedExit();
            }
        }
    }
}