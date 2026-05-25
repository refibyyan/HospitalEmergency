using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MekanikGameOver : MonoBehaviour
{
    [Header("Komponen UI Tombol")]
    public Image btnRestartUI;       // Tarik objek Image Restart ke sini
    public Image btnExitUI;          // Tarik objek Image Exit ke sini

    [Header("Aset Gambar Border Ijo")]
    public Sprite btnRestartIjo;     // Sprite Restart dengan border ijo
    public Sprite btnRestartPolos;   // Sprite Restart polos
    public Sprite btnExitIjo;        // Sprite Exit dengan border ijo
    public Sprite btnExitPolos;      // Sprite Exit polos

    private bool pilihRestart = true;

    // Fungsi otomatis berjalan saat panel Game Over ini aktif/muncul di layar
    void OnEnable()
    {
        pilihRestart = true;
        AturVisualGameOver();
    }

    void Update()
    {
        // Deteksi tombol keyboard A / D / Panah
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihRestart = true;
            AturVisualGameOver();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihRestart = false;
            AturVisualGameOver();
        }

        // Eksekusi pilihan tombol saat pencet Enter / Space
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (pilihRestart)
            {
                // Reload scene aktif saat ini (Restart)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                // Keluar dari game
                Debug.Log("Keluar dari Game...");
                Application.Quit();
            }
        }
    }

    void AturVisualGameOver()
    {
        if (pilihRestart == true)
        {
            if (btnRestartUI != null) btnRestartUI.sprite = btnRestartIjo;
            if (btnExitUI != null) btnExitUI.sprite = btnExitPolos;
        }
        else
        {
            if (btnRestartUI != null) btnRestartUI.sprite = btnRestartPolos;
            if (btnExitUI != null) btnExitUI.sprite = btnExitIjo;
        }
    }
}