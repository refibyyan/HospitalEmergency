using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelGameplayManager : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini di Inspector

    [Header("--- FADE SYSTEM ---")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 3f;

    [Header("--- POPUP & TIMER SYSTEM ---")]
    public CanvasGroup popUpCanvasGroup;
    public TMPro.TextMeshProUGUI timerText;
    public float startingTime = 20f;
    private float currentTime;
    private bool isTimerRunning = false;

    [HideInInspector]
    public bool timerHasStarted = false;

    [Header("--- AUDIO SYSTEM ---")]
    public AudioSource audioSource;
    public AudioClip sfxTimerStart;
    public AudioClip sfxTimerEnd;
    public AudioClip sfxSuccessPopUp;
    public AudioClip sfxSuccessConfirm;
    public AudioClip sfxSelecting; // Opsional: Tambahkan sfx pindah menu bad ending jika ada

    [Header("--- SCENE / EXIT SYSTEM ---")]
    public string nextSceneName = "Main Menu";

    [Header("--- BAD ENDING UI ---")]
    public GameObject badEndingPanelObject;
    public Image badEndingPopupDisplay;
    public Sprite spritePopupBadRestart;
    public Sprite spritePopupBadExit;

    [Header("--- SUCCESS POPUP ---")]
    public Image successPopupDisplay;
    public CanvasGroup successPopupBackground;

    [Header("--- ENDING DISPLAY ---")]
    public Image endingDisplay;
    [Tooltip("Masukkan CanvasGroup dari Image Hitam baru khusus untuk transisi ending slide")]
    public CanvasGroup endingFadeCanvasGroup;
    public List<Sprite> endingSlides = new List<Sprite>();

    [Tooltip("DURASI TIAP SLIDE ENDING")]
    public float slideDisplayDuration = 3f;

    // =====================================
    // KONSTANTA ALPHA TARGET FADEIMAGE
    // =====================================
    private const float FADE_IMAGE_TARGET_ALPHA = 250f / 255f;

    // =====================================
    // INTERNAL
    // =====================================
    private bool isCutscenePlaying = false;
    private bool isBadEndingActive = false;
    private bool isSelectingRestart = true;
    private bool waitingSuccessInput = false;

    // Kunci Anti-Spam Joystick
    private bool isJoystickHorizontalInUse = false;

    void Start()
    {
        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 0f;
        }

        DeactivateAllEndingUI();

        // Otomatis mencari script ESP32Input jika lupa di-drag di Inspector
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }

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
        if (isCutscenePlaying)
            return;

        if (timerText != null)
        {
            timerText.text = string.Format(
                "{0:00}:{1:00}",
                Mathf.FloorToInt(startingTime / 60),
                Mathf.FloorToInt(startingTime % 60)
            );
        }
    }

    void Update()
    {
        // =====================================
        // TIMER
        // =====================================
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

        // =====================================
        // BAD ENDING INPUT (HYBRID)
        // =====================================
        if (isBadEndingActive)
        {
            HandleBadEndingInputHybrid();
        }

        // =====================================
        // PAKSA ALPHA FADEIMAGE SELAMA SUCCESS POPUP AKTIF
        // =====================================
        if (waitingSuccessInput && successPopupBackground != null)
        {
            successPopupBackground.alpha = FADE_IMAGE_TARGET_ALPHA;
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void FreezePlayer()
    {
        GameObject player = GameObject.Find("lyra_depan");

        if (player != null)
        {
            FreezePlayerDynamic(player);
        }
        else
        {
            Debug.LogWarning("[LevelGameplayManager] FreezePlayer: 'lyra_depan' tidak ditemukan otomatis!");
        }
    }

    void FreezePlayerDynamic(GameObject player)
    {
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            AudioSource[] playerAudioSources = player.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource pAudio in playerAudioSources)
            {
                if (pAudio != null && pAudio.isPlaying)
                {
                    pAudio.Stop();
                }
            }

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script != this) script.enabled = false;
            }
            Debug.Log("[LevelGameplayManager] " + player.name + " & SFX Langkah Kakinya berhasil dihentikan!");
        }
    }

    // =====================================
    // BAD ENDING MANAGEMENT
    // =====================================
    void TimerGameOver()
    {
        isTimerRunning = false;
        isCutscenePlaying = true;

        Debug.Log("[LevelGameplayManager] TimerGameOver dipanggil!");

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;

        if (audioSource != null && sfxTimerEnd != null)
            audioSource.PlayOneShot(sfxTimerEnd);

        FreezePlayer();
        StartCoroutine(BadEndingRoutine());
    }

    IEnumerator BadEndingRoutine()
    {
        Debug.Log("[LevelGameplayManager] BadEndingRoutine dimulai...");

        yield return StartCoroutine(FadeScreen(0f, 1f, 1f));

        if (badEndingPanelObject != null)
        {
            badEndingPanelObject.SetActive(true);
        }

        if (badEndingPopupDisplay != null)
        {
            badEndingPopupDisplay.gameObject.SetActive(true);
            badEndingPopupDisplay.enabled = true;
            badEndingPopupDisplay.color = Color.white;

            isSelectingRestart = true;
            SetBadEndingSprite(true);
        }
        else
        {
            Debug.LogError("[LevelGameplayManager] badEndingPopupDisplay BELUM di-assign di Inspector!");
        }

        yield return StartCoroutine(FadeScreen(1f, 0f, 1f));

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isBadEndingActive = true;
    }

    void SetBadEndingSprite(bool selectRestart)
    {
        if (badEndingPopupDisplay == null) return;

        if (selectRestart)
        {
            if (spritePopupBadRestart != null)
            {
                badEndingPopupDisplay.sprite = spritePopupBadRestart;
            }
        }
        else
        {
            if (spritePopupBadExit != null)
            {
                badEndingPopupDisplay.sprite = spritePopupBadExit;
            }
        }

        if (badEndingPopupDisplay.canvas != null)
            Canvas.ForceUpdateCanvases();
    }

    // Penanganan Input Hybrid Khusus Bad Ending (Keyboard + Joystick ESP32)
    void HandleBadEndingInputHybrid()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (esp32Input != null && esp32Input.isConnected)
        {
            if (Mathf.Abs(esp32Input.horizontal) > 0.5f)
            {
                horizontalInput = esp32Input.horizontal;
            }
        }

        bool navigateLeft = false;
        bool navigateRight = false;

        // One-Shot behavior untuk navigasi horizontal stik/tombol arah
        if (horizontalInput != 0)
        {
            if (!isJoystickHorizontalInUse)
            {
                if (horizontalInput < -0.3f) navigateLeft = true;
                if (horizontalInput > 0.3f) navigateRight = true;
                isJoystickHorizontalInUse = true;
            }
        }
        else
        {
            isJoystickHorizontalInUse = false;
        }

        // Tambahan tombol keyboard A/D bawaan
        if (Input.GetKeyDown(KeyCode.A)) navigateLeft = true;
        if (Input.GetKeyDown(KeyCode.D)) navigateRight = true;

        bool changed = false;

        if (navigateLeft && !isSelectingRestart)
        {
            isSelectingRestart = true;
            changed = true;
            if (sfxSelecting != null && audioSource != null) audioSource.PlayOneShot(sfxSelecting);
            Debug.Log("[LevelGameplayManager] Pilihan berubah ke: RESTART");
        }
        else if (navigateRight && isSelectingRestart)
        {
            isSelectingRestart = false;
            changed = true;
            if (sfxSelecting != null && audioSource != null) audioSource.PlayOneShot(sfxSelecting);
            Debug.Log("[LevelGameplayManager] Pilihan berubah ke: EXIT");
        }

        if (changed)
        {
            SetBadEndingSprite(isSelectingRestart);
        }

        // Deteksi tombol Enter/Space pada keyboard ATAU tombol select ESP32
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
        {
            isConfirmPressed = true;
        }

        if (isConfirmPressed)
        {
            isBadEndingActive = false;

            if (isSelectingRestart)
            {
                string activeSceneName = SceneManager.GetActiveScene().name;
                Debug.Log("[LevelGameplayManager] Memuat ulang scene aktif saat ini: " + activeSceneName);
                SceneManager.LoadScene(activeSceneName);
            }
            else
            {
                Debug.Log("[LevelGameplayManager] Kembali ke: Main Menu");
                SceneManager.LoadScene("Main Menu");
            }
        }
    }

    // =====================================
    // GOOD ENDING MANAGEMENT
    // =====================================
    public void PlayerReachedExit(GameObject playerObject)
    {
        if (!isTimerRunning) return;
        if (isCutscenePlaying) return;

        isTimerRunning = false;
        isCutscenePlaying = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 0f;

        FreezePlayerDynamic(playerObject);
        StartCoroutine(GoodEndingRoutine());
    }

    IEnumerator GoodEndingRoutine()
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, 1f));

        if (successPopupDisplay != null)
        {
            successPopupDisplay.gameObject.SetActive(true);
            successPopupDisplay.transform.SetAsLastSibling();
        }

        if (successPopupBackground != null)
        {
            successPopupBackground.gameObject.SetActive(true);
            successPopupBackground.alpha = 0f;
            successPopupBackground.blocksRaycasts = true;
            successPopupBackground.transform.SetSiblingIndex(
                successPopupDisplay.transform.GetSiblingIndex() - 1
            );
        }

        if (audioSource != null && sfxSuccessPopUp != null)
            audioSource.PlayOneShot(sfxSuccessPopUp);

        yield return StartCoroutine(FadeScreenAndBackground(
            screenStart: 1f,
            screenEnd: 0f,
            bgStart: 0f,
            bgEnd: FADE_IMAGE_TARGET_ALPHA,
            duration: 1f,
            bgDuration: 0.2f
        ));

        if (successPopupBackground != null)
            successPopupBackground.alpha = FADE_IMAGE_TARGET_ALPHA;

        waitingSuccessInput = true;

        // Loop menunggu konfirmasi input Hybrid (Keyboard, Mouse Klik, ESP32 Button)
        while (waitingSuccessInput)
        {
            bool isSuccessConfirmed = Input.GetKeyDown(KeyCode.Return) ||
                                     Input.GetKeyDown(KeyCode.Space) ||
                                     Input.GetMouseButtonDown(0);

            if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
            {
                isSuccessConfirmed = true;
            }

            if (isSuccessConfirmed)
            {
                if (audioSource != null && sfxSuccessConfirm != null)
                    audioSource.PlayOneShot(sfxSuccessConfirm);

                waitingSuccessInput = false;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        if (successPopupDisplay != null)
            successPopupDisplay.gameObject.SetActive(false);

        if (successPopupBackground != null)
        {
            successPopupBackground.alpha = 0f;
            successPopupBackground.blocksRaycasts = false;
            successPopupBackground.gameObject.SetActive(false);
        }

        StartCoroutine(PlayEndingSlides());
    }

    IEnumerator PlayEndingSlides()
    {
        if (endingDisplay == null) yield break;

        if (endingFadeCanvasGroup != null)
        {
            endingFadeCanvasGroup.gameObject.SetActive(true);
            endingFadeCanvasGroup.alpha = 1f;
            endingFadeCanvasGroup.blocksRaycasts = true;
        }

        endingDisplay.gameObject.SetActive(true);

        for (int i = 0; i < endingSlides.Count; i++)
        {
            endingDisplay.sprite = endingSlides[i];

            if (endingFadeCanvasGroup != null)
                yield return StartCoroutine(FadeCustomCanvasGroup(endingFadeCanvasGroup, 1f, 0f, 0.7f));

            yield return new WaitForSeconds(slideDisplayDuration);

            if (endingFadeCanvasGroup != null)
                yield return StartCoroutine(FadeCustomCanvasGroup(endingFadeCanvasGroup, 0f, 1f, 0.7f));

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("[LevelGameplayManager] Selesai memutar slide dengan FadeImage baru. Memuat Main Menu...");
        SceneManager.LoadScene("Main Menu");
    }

    IEnumerator FadeCustomCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        cg.alpha = startAlpha;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }

    // =====================================
    // FADE FUNCTIONS
    // =====================================
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

    IEnumerator FadeScreenAndBackground(float screenStart, float screenEnd,
                                         float bgStart, float bgEnd,
                                         float duration, float bgDuration)
    {
        float timer = 0f;

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = screenStart;
        if (successPopupBackground != null) successPopupBackground.alpha = bgStart;

        while (timer < duration || timer < bgDuration)
        {
            timer += Time.deltaTime;

            if (fadeCanvasGroup != null && timer <= duration)
                fadeCanvasGroup.alpha = Mathf.Lerp(screenStart, screenEnd, timer / duration);

            if (successPopupBackground != null && timer <= bgDuration)
                successPopupBackground.alpha = Mathf.Lerp(bgStart, bgEnd, timer / bgDuration);

            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = screenEnd;
            fadeCanvasGroup.blocksRaycasts = (screenEnd == 1f);
        }

        if (successPopupBackground != null)
            successPopupBackground.alpha = bgEnd;
    }

    void DeactivateAllEndingUI()
    {
        if (badEndingPanelObject != null)
            badEndingPanelObject.SetActive(false);

        if (badEndingPopupDisplay != null)
            badEndingPopupDisplay.gameObject.SetActive(false);

        if (successPopupDisplay != null)
            successPopupDisplay.gameObject.SetActive(false);

        if (successPopupBackground != null)
        {
            successPopupBackground.gameObject.SetActive(false);
            successPopupBackground.alpha = 0f;
            successPopupBackground.blocksRaycasts = false;
        }

        if (endingDisplay != null)
            endingDisplay.gameObject.SetActive(false);

        if (endingFadeCanvasGroup != null)
            endingFadeCanvasGroup.gameObject.SetActive(false);
    }

    public void StartTimerAfterPotDialogue()
    {
        if (timerHasStarted) return;

        timerHasStarted = true;
        currentTime = startingTime;

        if (popUpCanvasGroup != null) popUpCanvasGroup.alpha = 1f;

        isTimerRunning = true;

        if (audioSource != null && sfxTimerStart != null)
        {
            audioSource.clip = sfxTimerStart;
            audioSource.loop = true;
            audioSource.Play();
        }

        UpdateTimerDisplay(currentTime);
    }
}