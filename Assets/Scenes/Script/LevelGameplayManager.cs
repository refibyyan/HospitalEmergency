using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelGameplayManager : MonoBehaviour
{
    // Struktur data agar Gambar, Teks, dan Durasi Tampil bisa ditambah dinamis di Inspector
    [System.Serializable]
    public struct EndingSlide
    {
        public Sprite imageSprite;       // Gambar untuk slide ini
        [TextArea(3, 5)]
        public string textContent;       // Teks narasi
        public float displayDuration;    // Berapa lama slide ditumpuk diam setelah teks selesai diketik
    }

    [Header("--- FADE SYSTEM ---")]
    [Tooltip("Masukkan UI Image (FadeImage) yang menutup layar di sini")]
    public CanvasGroup fadeCanvasGroup;
    [Tooltip("Durasi berapa detik fade out berlangsung")]
    public float fadeDuration = 3.0f;

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
    [Tooltip("Masukkan file (.mp3/.wav) suara pendek detak jam (Otomatis Looping)")]
    public AudioClip sfxTimerStart;
    [Tooltip("Masukkan file (.mp3/.wav) suara saat waktu habis / game over")]
    public AudioClip sfxTimerEnd;

    [Header("--- SCENE / EXIT SYSTEM ---")]
    [Tooltip("Tuliskan NAMA SCENE tujuan berikutnya (harus sama persis huruf besar kecilnya)")]
    public string nextSceneName;

    [Header("--- CUTSCENE ENDING UI SYSTEM ---")]
    [Tooltip("Masukkan UI Image khusus tempat menampilkan gambar ending (Stretch Full Layar)")]
    public Image endingImageDisplay;
    [Tooltip("Masukkan TextMeshPro khusus tempat menampilkan teks ending")]
    public TextMeshProUGUI endingTextDisplay;
    [Tooltip("Kecepatan ketik animasi typewriter (makin kecil makin cepat, misal 0.05)")]
    public float typingSpeed = 0.05f;
    [Tooltip("Audio klip suara ketikan keyboard/mesin tik")]
    public AudioClip sfxTyping;

    [Header("--- GOOD ENDING SLIDES ---")]
    [Tooltip("Klik '+' untuk menambah urutan gambar dan teks saat masuk Pintu Exit")]
    public List<EndingSlide> goodEndingSlides = new List<EndingSlide>();

    [Header("--- BAD ENDING SLIDES ---")]
    [Tooltip("Klik '+' untuk menambah urutan gambar dan teks saat Timer Habis (00:00)")]
    public List<EndingSlide> badEndingSlides = new List<EndingSlide>();

    private bool isCutscenePlaying = false;

    void Start()
    {
        // Awal game, sembunyikan Pop-up Timer dan UI Ending
        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;
        ResetEndingUI();

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

        OnFadeOutComplete();
    }

    void OnFadeOutComplete()
    {
        if (isCutscenePlaying) return;

        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 1f;

        if (audioSource != null && sfxTimerStart != null)
        {
            audioSource.clip = sfxTimerStart;
            audioSource.loop = true;
            audioSource.Play();
        }

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

    // TRIGGER BAD ENDING (Waktu Habis)
    void TimerGameOver()
    {
        isTimerRunning = false;
        isCutscenePlaying = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        PlaySound(sfxTimerEnd);
        Debug.Log("Waktu habis! Memulai Bad Ending...");

        // Sembunyikan timer saat cutscene berjalan
        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;

        // Jalankan rangkaian cutscene Bad Ending, lalu restart Level 3
        StartCoroutine(PlayEndingCutsceneRoutine(badEndingSlides, "Level 3"));
    }

    // TRIGGER GOOD ENDING (Sentuh Pintu Exit)
    public void PlayerReachedExit()
    {
        if (isTimerRunning && !isCutscenePlaying)
        {
            isTimerRunning = false;
            isCutscenePlaying = true;

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
            }

            Debug.Log("Lyra keluar! Memulai Good Ending...");

            if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;

            // Jalankan rangkaian cutscene Good Ending, lalu pergi ke MainMenu
            StartCoroutine(PlayEndingCutsceneRoutine(goodEndingSlides, nextSceneName));
        }
    }

    // CORE ENGINE: Mengatur jalannya gabungan slide gambar, text typewriter, sfx typing, dan fade
    IEnumerator PlayEndingCutsceneRoutine(List<EndingSlide> slides, string targetScene)
    {
        // 1. Fade In Layar Hitam Sebelum Gambar Pertama Masuk
        yield return StartCoroutine(FadeScreen(0f, 1f));
        ResetEndingUI();

        // 2. Mainkan satu per satu slide yang sudah didaftarkan di Inspector
        foreach (EndingSlide slide in slides)
        {
            // Pasang gambar dan kosongkan teks awal
            if (endingImageDisplay != null) endingImageDisplay.sprite = slide.imageSprite;
            if (endingTextDisplay != null) endingTextDisplay.text = "";

            // Layar hitam memudar terbuka (Memperlihatkan Gambar dan Kotak Teks)
            yield return StartCoroutine(FadeScreen(1f, 0f));

            // Jalankan animasi mengetik teks + SFX Typing
            yield return StartCoroutine(TypeTextRoutine(slide.textContent));

            // Diam beberapa saat sesuai kustom durasi yang kamu tentukan di Inspector
            yield return new WaitForSeconds(slide.displayDuration);

            // Layar menutup hitam kembali sebelum ganti ke slide berikutnya
            yield return StartCoroutine(FadeScreen(0f, 1f));
        }

        // 3. Setelah semua slide habis berjalan, bersihkan UI lalu muat Scene tujuan
        ResetEndingUI();
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("Nama Scene tujuan kosong!");
            // Pengaman jika lupa isi nama scene, kembalikan ke layar normal
            yield return StartCoroutine(FadeScreen(1f, 0f));
            isCutscenePlaying = false;
        }
    }

    // Animasi Typewriter + Kontrol SFX Typing Mulai & Berhenti otomatis
    IEnumerator TypeTextRoutine(string fullText)
    {
        endingTextDisplay.text = "";

        // Buat AudioSource memutar SFX mengetik secara looping sewaktu teks diketik
        if (audioSource != null && sfxTyping != null)
        {
            audioSource.clip = sfxTyping;
            audioSource.loop = true;
            audioSource.Play();
        }

        foreach (char letter in fullText.ToCharArray())
        {
            endingTextDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Teks selesai diketik penuh -> Segera MATIKAN suara sfx ketikan
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    // Fungsi pembantu untuk memudarkan layar hitam (Fade In / Fade Out)
    IEnumerator FadeScreen(float startAlpha, float endAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float timer = 0f;
        fadeCanvasGroup.alpha = startAlpha;

        while (timer < 1f) // Durasi perpindahan slide sengaja dikunci 1 detik agar pas
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / 1f);
            yield return null;
        }
        fadeCanvasGroup.alpha = endAlpha;
    }

    void ResetEndingUI()
    {
        if (endingImageDisplay != null) endingImageDisplay.sprite = null;
        if (endingTextDisplay != null) endingTextDisplay.text = "";
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}