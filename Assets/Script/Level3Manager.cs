using UnityEngine;
using TMPro; // Wajib ada untuk mengatur TextMeshPro
using UnityEngine.SceneManagement; // Wajib ada untuk pindah scene

public class Level3Manager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // Tempat naruh teks timer nanti

    [Header("Settings")]
    public float timeLeft = 15f; // Waktu hitung mundur (15 detik)

    private bool isGameOver = false;

    void Update()
    {
        // Jika game over (waktu habis), stop script-nya
        if (isGameOver) return;

        // Jika waktu masih ada, kurangi terus setiap detik
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            DisplayTime(timeLeft);
        }
        else
        {
            timeLeft = 0;
            isGameOver = true;
            TriggerBadEnding();
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        // Membulatkan angka ke atas agar tidak ada koma desimal (misal: 14.2s jadi 15s)
        float seconds = Mathf.CeilToInt(timeToDisplay);
        timerText.text = "Time: " + seconds.ToString() + "s";
    }

    void TriggerBadEnding()
    {
        Debug.Log("Waktu Habis! Pindah ke Bad Ending.");

        // Mengisi nama scene Bad Ending kamu. 
        // Pastikan nanti kamu punya scene bernama "Bad Ending" atau sesuaikan namanya di sini
        SceneManager.LoadScene("Bad Ending");
    }
}