using UnityEngine;
using UnityEngine.SceneManagement; // PENTING: Untuk mengatur pindah scene/level

public class ExitDoor : MonoBehaviour
{
    [Header("--- Pengaturan Pindah Level ---")]
    [SerializeField] private string namaSceneBerikutnya; // Ketik nama scene tujuan di Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek apakah yang menyentuh pintu adalah Player (sesuaikan Tag-nya, misal "Player")
        if (collision.CompareTag("Player"))
        {
            // Panggil fungsi SelesaiLevel yang ada di LevelManager
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.SelesaiLevel();
            }

            // Tunggu sebentar agar suara selesai/menang sempat berbunyi sebelum pindah level
            Invoke("PindahLevel", 1.5f);
        }
    }

    private void PindahLevel()
    {
        if (!string.IsNullOrEmpty(namaSceneBerikutnya))
        {
            SceneManager.LoadScene(namaSceneBerikutnya);
        }
        else
        {
            Debug.LogWarning("Nama scene berikutnya belum diisi di Inspector Pintu Exit!");
        }
    }
}