using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelGameplayManager : MonoBehaviour
{
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

    [Header("--- SCENE / EXIT SYSTEM ---")]
    public string nextSceneName;

    [Header("--- BAD ENDING UI ---")]
    public Image badEndingPopupDisplay;
    public Sprite spritePopupBadRestart;
    public Sprite spritePopupBadExit;

    [Header("--- SUCCESS POPUP ---")]
    public Image successPopupDisplay;
    public CanvasGroup successPopupBackground;

    [Header("--- ENDING DISPLAY ---")]
    public Image endingDisplay;
    public List<Sprite> endingSlides = new List<Sprite>();

    [Tooltip("DURASI TIAP SLIDE ENDING")]
    public float slideDisplayDuration = 3f;

    // =====================================
    // KONSTANTA ALPHA TARGET FADEIMAGE
    // Alpha 250 (skala 0-255) = 250f / 255f dalam Unity
    // =====================================
    private const float FADE_IMAGE_TARGET_ALPHA = 250f / 255f; // ~0.9804f

    // =====================================
    // INTERNAL
    // =====================================
    private bool isCutscenePlaying = false;
    private bool isBadEndingActive = false;
    private bool isSelectingRestart = true;
    private bool waitingSuccessInput = false;

    void Start()
    {
        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 0f;
        }

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
        // BAD ENDING INPUT
        // =====================================
        if (isBadEndingActive)
        {
            HandleBadEndingInput();
        }

        // =====================================
        // PAKSA ALPHA FADEIMAGE SELAMA SUCCESS POPUP AKTIF
        // Ini jaminan agar Unity tidak bisa mereset nilai alpha
        // =====================================
        if (waitingSuccessInput && successPopupBackground != null)
        {
            successPopupBackground.alpha = FADE_IMAGE_TARGET_ALPHA;
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void FreezePlayer()
    {
        GameObject player = GameObject.Find("lyra_depan");

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                if (script != this)
                {
                    script.enabled = false;
                }
            }
        }
    }

    // =====================================
    // BAD ENDING
    // =====================================
    void TimerGameOver()
    {
        isTimerRunning = false;
        isCutscenePlaying = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 0f;
        }

        if (audioSource != null && sfxTimerEnd != null)
        {
            audioSource.PlayOneShot(sfxTimerEnd);
        }

        FreezePlayer();
        StartCoroutine(BadEndingRoutine());
    }

    IEnumerator BadEndingRoutine()
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, 1f));

        if (badEndingPopupDisplay != null)
        {
            badEndingPopupDisplay.gameObject.SetActive(true);
            isSelectingRestart = true;
            badEndingPopupDisplay.sprite = spritePopupBadRestart;
        }

        yield return StartCoroutine(FadeScreen(1f, 0f, 1f));

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isBadEndingActive = true;
    }

    void HandleBadEndingInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isSelectingRestart = true;

            if (badEndingPopupDisplay != null && spritePopupBadRestart != null)
            {
                badEndingPopupDisplay.sprite = spritePopupBadRestart;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            isSelectingRestart = false;

            if (badEndingPopupDisplay != null && spritePopupBadExit != null)
            {
                badEndingPopupDisplay.sprite = spritePopupBadExit;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            isBadEndingActive = false;

            if (isSelectingRestart)
            {
                SceneManager.LoadScene("Level 3");
            }
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }

    // =====================================
    // GOOD ENDING
    // =====================================
    public void PlayerReachedExit()
    {
        if (!isTimerRunning)
            return;

        if (isCutscenePlaying)
            return;

        isTimerRunning = false;
        isCutscenePlaying = true;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 0f;
        }

        FreezePlayer();
        StartCoroutine(GoodEndingRoutine());
    }

    IEnumerator GoodEndingRoutine()
    {
        // 1. Layar menutup total ke hitam (Tirai Utama)
        yield return StartCoroutine(FadeScreen(0f, 1f, 1f));

        // 2. Aktifkan SuccessPopup teks (di depan)
        if (successPopupDisplay != null)
        {
            successPopupDisplay.gameObject.SetActive(true);
            successPopupDisplay.transform.SetAsLastSibling();
        }

        // 3. Paksa aktifkan SuccessFadeBackground Baru dan reset alpha ke 0
        if (successPopupBackground != null)
        {
            successPopupBackground.gameObject.SetActive(true);
            successPopupBackground.alpha = 0f;
            successPopupBackground.blocksRaycasts = true;
            successPopupBackground.transform.SetSiblingIndex(
                successPopupDisplay.transform.GetSiblingIndex() - 1
            );
        }

        // 4. Mainkan SFX muncul
        if (audioSource != null && sfxSuccessPopUp != null)
        {
            audioSource.PlayOneShot(sfxSuccessPopUp);
        }

        // 5. MODIFIKASI: Kecepatan tirai hitam membuka (1f -> 0f) diset 1 detik,
        //    Sedangkan background baru (0f -> 250) langsung ngebut selesai dalam 0.2 detik!
        yield return StartCoroutine(FadeScreenAndBackground(
            screenStart: 1f,
            screenEnd:   0f,
            bgStart:     0f,
            bgEnd:       FADE_IMAGE_TARGET_ALPHA,
            duration:    1f,
            bgDuration:  0.2f // <-- Parameter baru untuk kecepatan background hitam
        ));

        // 6. JAMINAN: ikat paksa alpha agar tidak bisa balik ke 0
        if (successPopupBackground != null)
        {
            successPopupBackground.alpha = FADE_IMAGE_TARGET_ALPHA;
        }

        // 7. Mulai menunggu input player
        waitingSuccessInput = true;

        while (waitingSuccessInput)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space)  ||
                Input.GetMouseButtonDown(0))
            {
                if (audioSource != null && sfxSuccessConfirm != null)
                {
                    audioSource.PlayOneShot(sfxSuccessConfirm);
                }
                waitingSuccessInput = false;
            }

            yield return null;
        }

        // 8. Jeda kecil agar SFX confirm sempat terdengar
        yield return new WaitForSeconds(0.2f);

        // 9. Sembunyikan sukses popup setelah input
        if (successPopupDisplay != null)
        {
            successPopupDisplay.gameObject.SetActive(false);
        }

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
        if (endingDisplay == null)
            yield break;

        endingDisplay.gameObject.SetActive(true);

        for (int i = 0; i < endingSlides.Count; i++)
        {
            yield return StartCoroutine(FadeScreen(0f, 1f, 0.7f));
            endingDisplay.sprite = endingSlides[i];
            yield return StartCoroutine(FadeScreen(1f, 0f, 0.7f));

            yield return new WaitForSeconds(slideDisplayDuration);
        }

        yield return StartCoroutine(FadeScreen(0f, 1f, 1f));

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // =====================================
    // FADE FUNCTIONS
    // =====================================
    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

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

    // Sistem Interpolasi Ganda dengan kontrol durasi terpisah
    IEnumerator FadeScreenAndBackground(float screenStart, float screenEnd,
                                         float bgStart,     float bgEnd,
                                         float duration,    float bgDuration)
    {
        float timer = 0f;

        if (fadeCanvasGroup != null)        fadeCanvasGroup.alpha        = screenStart;
        if (successPopupBackground != null) successPopupBackground.alpha = bgStart;

        while (timer < duration || timer < bgDuration)
        {
            timer += Time.deltaTime;

            // Transisi Tirai Utama
            if (fadeCanvasGroup != null && timer <= duration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(screenStart, screenEnd, timer / duration);
            }

            // Transisi Kilat Background Sukses (0.2 Detik!)
            if (successPopupBackground != null && timer <= bgDuration)
            {
                successPopupBackground.alpha = Mathf.Lerp(bgStart, bgEnd, timer / bgDuration);
            }

            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = screenEnd;
            fadeCanvasGroup.blocksRaycasts = (screenEnd == 1f);
        }

        if (successPopupBackground != null)
        {
            successPopupBackground.alpha = bgEnd;
        }
    }

    void DeactivateAllEndingUI()
    {
        if (badEndingPopupDisplay != null)
        {
            badEndingPopupDisplay.gameObject.SetActive(false);
        }

        if (successPopupDisplay != null)
        {
            successPopupDisplay.gameObject.SetActive(false);
        }

        if (successPopupBackground != null)
        {
            successPopupBackground.gameObject.SetActive(false);
            successPopupBackground.alpha = 0f;
            successPopupBackground.blocksRaycasts = false;
        }

        if (endingDisplay != null)
        {
            endingDisplay.gameObject.SetActive(false);
        }
    }

    public void StartTimerAfterPotDialogue()
    {
        if (timerHasStarted)
            return;

        timerHasStarted = true;
        currentTime = startingTime;

        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 1f;
        }

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