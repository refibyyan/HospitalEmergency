using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Dibutuhkan untuk pindah scene/level

public class LevelGameplayManager : MonoBehaviour
{
    [Header("--- FADE SYSTEM ---")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 2.0f;

    [Header("--- POP-UP & TIMER SYSTEM ---")]
    [Tooltip("Masukkan Canvas Group dari PopUpObject di sini")]
    public CanvasGroup popUpCanvasGroup;
    [Tooltip("Masukkan TextMeshPro dari TimerText")]
    public TextMeshProUGUI timerText;
    [Tooltip("Durasi waktu mundur dalam detik")]
    public float startingTime = 20f;
    private float currentTime;
    private bool isTimerRunning = false;

    [Header("--- AUDIO SYSTEM ---")]
    [Tooltip("Tarik objek GameplayManager yang ada komponen Audio Source-nya ke sini")]
    public AudioSource audioSource;
    [Tooltip("Masukkan file (.mp3/.wav) untuk suara saat timer mulai")]
    public AudioClip sfxTimerStart;
    [Tooltip("Masukkan file (.mp3/.wav) untuk suara saat waktu habis")]
    public AudioClip sfxTimerEnd;

    [Header("--- SCENE / EXIT SYSTEM ---")]
    [Tooltip("Tuliskan NAMA SCENE tujuan berikutnya (harus sama persis huruf besar kecilnya)")]
    public string nextSceneName;

    void Start()
    {
        // Di awal game, sembunyikan pop-up timer
        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;

        // Memulai animasi layar hitam memudar
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(FadeOutRoutine());
        }
    }

    IEnumerator FadeOutRoutine()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        // Panggil fungsi ketika fade selesai
        OnFadeOutComplete();
    }

    void OnFadeOutComplete()
    {
        // 1. Munculkan Pop-up Timer
        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 1f;

        // 2. Putar SFX Start (Suara Timer Mulai)
        PlaySound(sfxTimerStart);

        // 3. Jalankan Waktu Mundur
        currentTime = startingTime;
        isTimerRunning = true;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay(currentTime);
            }
            else
            {
                currentTime = 0;
                UpdateTimerDisplay(currentTime);
                TimerGameOver();
            }
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TimerGameOver()
    {
        isTimerRunning = false;

        // Putar SFX End (Suara waktu habis)
        PlaySound(sfxTimerEnd);

        Debug.Log("Waktu habis! Lyra gagal keluar.");
        // Kamu bisa tambah logic Game Over di sini nanti jika mau
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // FUNGSI UTK DETEKSI LYRA NYENTUH PINTU EXIT
    // Fungsi ini otomatis berjalan saat objek dengan BoxCollider2D (Trigger) tersentuh player
    public void PlayerReachedExit()
    {
        if (isTimerRunning) // Player hanya bisa lolos kalau timer masih jalan
        {
            isTimerRunning = false;
            Debug.Log("Lyra berhasil kabur! Pindah ke scene berikutnya...");

            // Pindah ke scene/level berikutnya
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("Nama scene berikutnya belum kamu tulis di Inspector!");
            }
        }
    }
}