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

    [Header("--- AUDIO SYSTEM ---")]
    public AudioSource audioSource;

    public AudioClip sfxTimerStart;

    public AudioClip sfxTimerEnd;

    [Header("--- SCENE / EXIT SYSTEM ---")]
    public string nextSceneName;

    [Header("--- BAD ENDING UI ---")]
    public Image badEndingPopupDisplay;

    public Sprite spritePopupBadRestart;

    public Sprite spritePopupBadExit;

    [Header("--- GOOD ENDING UI ---")]
    [Tooltip("CUMA 1 IMAGE INI AJA YANG DIPAKE")]
    public Image successPopupDisplay;

    [Tooltip("ISI SEMUA SLIDE GOOD ENDING DISINI")]
    public List<Sprite> goodEndingSlides =
        new List<Sprite>();

    [Tooltip("DURASI TIAP SLIDE")]
    public float slideDisplayDuration = 3f;

    // =====================================
    // INTERNAL
    // =====================================

    private bool isCutscenePlaying = false;

    private bool isBadEndingActive = false;

    private bool isSelectingRestart = true;

    private bool isPlayingSlides = false;

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

            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    timer / fadeDuration
                );

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

        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 1f;
        }

        if (audioSource != null &&
            sfxTimerStart != null)
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
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }

        int minutes =
            Mathf.FloorToInt(timeToDisplay / 60);

        int seconds =
            Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text =
            string.Format(
                "{0:00}:{1:00}",
                minutes,
                seconds
            );
    }

    void FreezePlayer()
    {
        GameObject player =
            GameObject.Find("lyra_depan");

        if (player != null)
        {
            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            MonoBehaviour[] scripts =
                player.GetComponents<MonoBehaviour>();

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

        if (sfxTimerEnd != null)
        {
            audioSource.PlayOneShot(
                sfxTimerEnd
            );
        }

        if (popUpCanvasGroup != null)
        {
            popUpCanvasGroup.alpha = 0f;
        }

        FreezePlayer();

        StartCoroutine(BadEndingRoutine());
    }

    IEnumerator BadEndingRoutine()
    {
        yield return StartCoroutine(
            FadeScreen(0f, 1f, 1f)
        );

        if (badEndingPopupDisplay != null)
        {
            badEndingPopupDisplay
                .gameObject
                .SetActive(true);

            isSelectingRestart = true;

            badEndingPopupDisplay.sprite =
                spritePopupBadRestart;
        }

        yield return StartCoroutine(
            FadeScreen(1f, 0f, 1f)
        );

        isBadEndingActive = true;
    }

    void HandleBadEndingInput()
    {
        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isSelectingRestart = true;

            if (badEndingPopupDisplay != null)
            {
                badEndingPopupDisplay.sprite =
                    spritePopupBadRestart;
            }
        }

        else if (Input.GetKeyDown(KeyCode.D) ||
                 Input.GetKeyDown(KeyCode.RightArrow))
        {
            isSelectingRestart = false;

            if (badEndingPopupDisplay != null)
            {
                badEndingPopupDisplay.sprite =
                    spritePopupBadExit;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            isBadEndingActive = false;

            if (isSelectingRestart)
            {
                SceneManager.LoadScene(
                    "Level 3"
                );
            }
            else
            {
                Application.Quit();
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
        // FADE KE HITAM
        yield return StartCoroutine(
            FadeScreen(0f, 1f, 1f)
        );

        // TAMPILKAN IMAGE
        if (successPopupDisplay != null)
        {
            successPopupDisplay
                .gameObject
                .SetActive(true);
        }

        // FADE BUKA
        yield return StartCoroutine(
            FadeScreen(1f, 0f, 1f)
        );

        // AUTO MAINKAN SLIDE
        StartCoroutine(
            PlayGoodEndingSlides()
        );
    }

    IEnumerator PlayGoodEndingSlides()
    {
        if (isPlayingSlides)
            yield break;

        isPlayingSlides = true;

        for (int i = 0;
             i < goodEndingSlides.Count;
             i++)
        {
            // FADE TUTUP
            yield return StartCoroutine(
                FadeScreen(0f, 1f, 0.7f)
            );

            // GANTI SPRITE
            if (successPopupDisplay != null)
            {
                successPopupDisplay.sprite =
                    goodEndingSlides[i];
            }

            // FADE BUKA
            yield return StartCoroutine(
                FadeScreen(1f, 0f, 0.7f)
            );

            // TUNGGU OTOMATIS
            yield return new WaitForSeconds(
                slideDisplayDuration
            );
        }

        // PINDAH SCENE
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(
                nextSceneName
            );
        }
    }

    // =====================================
    // FADE
    // =====================================

    IEnumerator FadeScreen(
        float startAlpha,
        float endAlpha,
        float duration
    )
    {
        if (fadeCanvasGroup == null)
            yield break;

        float timer = 0f;

        fadeCanvasGroup.alpha =
            startAlpha;

        fadeCanvasGroup.blocksRaycasts =
            (endAlpha == 1f);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    timer / duration
                );

            yield return null;
        }

        fadeCanvasGroup.alpha =
            endAlpha;
    }

    void DeactivateAllEndingUI()
    {
        if (badEndingPopupDisplay != null)
        {
            badEndingPopupDisplay
                .gameObject
                .SetActive(false);
        }

        if (successPopupDisplay != null)
        {
            successPopupDisplay
                .gameObject
                .SetActive(false);
        }
    }
}