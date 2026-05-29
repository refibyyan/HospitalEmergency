using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelGameplayManager : MonoBehaviour
{
    [Header("--- FADE SYSTEM ---")]
    [Tooltip("Masukkan UI Image (FadeImage) yang menutup layar di sini")]
    public CanvasGroup fadeCanvasGroup;
    [Tooltip("Durasi berapa detik fade out berlangsung di awal game")]
    public float fadeDuration = 3.0f;

    [Header("--- POP-UP & TIMER SYSTEM ---")]
    [Tooltip("Masukkan Canvas Group dari PopUpObject di sini")]
    public CanvasGroup popUpCanvasGroup;
    [Tooltip("Masukkan TextMeshPro dari TimerText")]
    public TMPro.TextMeshProUGUI timerText;
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

    [Header("--- BAD ENDING UI ---")]
    [Tooltip("Masukkan UI Image tempat menampilkan pop up Bad Ending")]
    public Image badEndingPopupDisplay;
    [Tooltip("Sprite PopUp saat mendeteksi pilihan RESTART")]
    public Sprite spritePopupBadRestart;
    [Tooltip("Sprite PopUp saat mendeteksi pilihan EXIT")]
    public Sprite spritePopupBadExit;

    [Header("--- GOOD ENDING UI ---")]
    [Tooltip("Masukkan UI Image tempat menampilkan pop up awal Success")]
    public Image successPopupDisplay;
    [Tooltip("Masukkan UI Image Stretch Full Layar khusus tempat slide cerita Good Ending")]
    public Image goodEndingSlideDisplay;
    [Tooltip("Masukkan Tombol Transparan Full Layar untuk mendeteksi klik lanjut slide")]
    public Button slideClickButton;

    [Header("--- GOOD ENDING SLIDES DATA ---")]
    [Tooltip("Klik '+' untuk menambah urutan gambar full-screen setelah pop-up sukses diklik")]
    public List<Sprite> goodEndingSlides = new List<Sprite>();
    [Tooltip("Durasi diam (dalam detik) untuk setiap sprite/slide cerita sebelum memudar")]
    public float slideDisplayDuration = 3.0f;

    // Variabel Kontrol Internal
    private bool isCutscenePlaying = false;
    private bool isBadEndingActive = false;
    private bool isSelectingRestart = true; // True = Restart, False = Exit
    private int currentGoodSlideIndex = -1; // -1 berarti sedang di PopUpSuccess awal
    private bool isTransitioningSlide = false; // Pengaman agar tidak bisa spam klik saat fade

    void Start()
    {
        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;
        DeactivateAllEndingUI();

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

        if (isBadEndingActive)
        {
            HandleBadEndingInput();
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void FreezePlayer()
    {
        GameObject player = GameObject.Find("lyra_depan");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this) script.enabled = false;
            }
        }
    }

    // ==========================================
    // LOGIC: BAD ENDING SYSTEM (TIMER ABIS)
    // ==========================================
    void TimerGameOver()
    {
        isTimerRunning = false;
        isCutscenePlaying = true;

        if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
        if (sfxTimerEnd != null) audioSource.PlayOneShot(sfxTimerEnd);

        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;
        FreezePlayer();

        StartCoroutine(BadEndingRoutine());
    }

    IEnumerator BadEndingRoutine()
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, 1.0f));

        if (badEndingPopupDisplay != null)
        {
            badEndingPopupDisplay.gameObject.SetActive(true);
            isSelectingRestart = true;
            badEndingPopupDisplay.sprite = spritePopupBadRestart;
        }

        yield return StartCoroutine(FadeScreen(1f, 0f, 1.0f));
        isBadEndingActive = true;
    }

    void HandleBadEndingInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isSelectingRestart = true;
            if (badEndingPopupDisplay != null) badEndingPopupDisplay.sprite = spritePopupBadRestart;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            isSelectingRestart = false;
            if (badEndingPopupDisplay != null) badEndingPopupDisplay.sprite = spritePopupBadExit;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            isBadEndingActive = false;
            if (isSelectingRestart) SceneManager.LoadScene("Level 3");
            else Application.Quit();
        }
    }

    // ==========================================
    // LOGIC: GOOD ENDING SYSTEM (PINTU EXIT)
    // ==========================================
    public void PlayerReachedExit()
    {
        if (isTimerRunning && !isCutscenePlaying)
        {
            isTimerRunning = false;
            isCutscenePlaying = true;

            if (audioSource != null) { audioSource.Stop(); audioSource.loop = false; }
            if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;
            FreezePlayer();

            StartCoroutine(GoodEndingRoutine());
        }
    }

    IEnumerator GoodEndingRoutine()
    {
        // 1. Tutup layar ke hitam penuh
        yield return StartCoroutine(FadeScreen(0f, 1f, 1.0f));

        // 2. Siapkan UI awal, aktifkan tombol klik rahasia agar bisa diklik/di-enter
        if (successPopupDisplay != null) successPopupDisplay.gameObject.SetActive(true);
        if (goodEndingSlideDisplay != null) goodEndingSlideDisplay.gameObject.SetActive(false);
        if (slideClickButton != null) slideClickButton.gameObject.SetActive(true);

        currentGoodSlideIndex = -1; // Set ke status papan piala sukses
        isTransitioningSlide = false;

        // 3. Membuka layar hitam untuk memperlihatkan papan sukses
        yield return StartCoroutine(FadeScreen(1f, 0f, 1.0f));
    }

    // Fungsi Pintar Utama: Dipanggil saat menekan tombol transparan layar ATAU menekan Enter/Space
    public void AdvanceToNextGoodSlide()
    {
        // Cegah spam klik jika animasi transisinya belum beres
        if (isTransitioningSlide) return;

        StartCoroutine(PlayGoodSlidesRoutine());
    }

    IEnumerator PlayGoodSlidesRoutine()
    {
        isTransitioningSlide = true;
        currentGoodSlideIndex++;

        // 1. FADE OUT: Layar pelan-pelan menutup hitam penuh
        yield return StartCoroutine(FadeScreen(fadeCanvasGroup.alpha, 1f, 0.8f));

        // Matikan pop up sukses awal jika sudah lewat langkah pertama
        if (successPopupDisplay != null) successPopupDisplay.gameObject.SetActive(false);

        // 2. Periksa apakah gambar sprite cerita berikutnya tersedia di daftar list
        if (goodEndingSlides != null && currentGoodSlideIndex < goodEndingSlides.Count)
        {
            if (goodEndingSlideDisplay != null)
            {
                goodEndingSlideDisplay.gameObject.SetActive(true);
                // Pasang sprite baru ke bingkai layar
                goodEndingSlideDisplay.sprite = goodEndingSlides[currentGoodSlideIndex];
            }

            // FADE IN: Layar hitam membuka perlahan menampilkan sprite cerita baru
            yield return StartCoroutine(FadeScreen(1f, 0f, 0.8f));
            isTransitioningSlide = false;

            // DURASI DIAM: Tunggu beberapa detik sesuai durasi yang kamu tentukan di Inspector
            float elapsed = 0f;
            while (elapsed < slideDisplayDuration)
            {
                // Jika di tengah-tengah durasi player kebelet mencet Enter/Klik, langsung potong durasi diamnya
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // Jika semua sprite gambar cerita di list sudah habis digunakan, pindah ke MainMenu
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                // Pengaman jika kolom nama scene lupa diisi agar tidak nge-hang hitam
                yield return StartCoroutine(FadeScreen(1f, 0f, 0.5f));
                DeactivateAllEndingUI();
                isTransitioningSlide = false;
            }
        }
    }

    // ==========================================
    // UTILITY UTILS
    // ==========================================
    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;
        float timer = 0f;
        fadeCanvasGroup.alpha = startAlpha;
        fadeCanvasGroup.blocksRaycasts = (endAlpha == 1f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        fadeCanvasGroup.alpha = endAlpha;
    }

    void DeactivateAllEndingUI()
    {
        if (badEndingPopupDisplay != null) badEndingPopupDisplay.gameObject.SetActive(false);
        if (successPopupDisplay != null) successPopupDisplay.gameObject.SetActive(false);
        if (goodEndingSlideDisplay != null) goodEndingSlideDisplay.gameObject.SetActive(false);
        if (slideClickButton != null) slideClickButton.gameObject.SetActive(false);
    }
}