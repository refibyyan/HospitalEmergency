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

    [Header("--- SCENE / EXIT SYSTEM ---")]
    public string nextSceneName;

    [Header("--- BAD ENDING UI ---")]
    public Image badEndingPopupDisplay;
    public Sprite spritePopupBadRestart;
    public Sprite spritePopupBadExit;

    [Header("--- SUCCESS POPUP ---")]
    public Image successPopupDisplay;

    [Header("--- ENDING DISPLAY ---")]
    public Image endingDisplay;
    public List<Sprite> endingSlides = new List<Sprite>();

    [Tooltip("DURASI TIAP SLIDE ENDING")]
    public float slideDisplayDuration = 3f;

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
        isBadEndingActive = true;
    }

    void HandleBadEndingInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isSelectingRestart = true;

            if (badEndingPopupDisplay != null)
            {
                badEndingPopupDisplay.sprite = spritePopupBadRestart;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            isSelectingRestart = false;

            if (badEndingPopupDisplay != null)
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
        yield return StartCoroutine(FadeScreen(0f, 1f, 1f));

        if (successPopupDisplay != null)
        {
            successPopupDisplay.gameObject.SetActive(true);
        }

        yield return StartCoroutine(FadeScreen(1f, 0f, 1f));
        waitingSuccessInput = true;

        while (waitingSuccessInput)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetMouseButtonDown(0))
            {
                waitingSuccessInput = false;
            }

            yield return null;
        }

        if (successPopupDisplay != null)
        {
            successPopupDisplay.gameObject.SetActive(false);
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
    // FADE
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