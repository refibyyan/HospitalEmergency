using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class QuickDecisionManager : MonoBehaviour
{
    [Header("MAIN UI")]
    public GameObject quickDecisionUI;
    public CanvasGroup quickDecisionCanvas;

    [Header("FADE IMAGE")]
    public Image fadeImage;

    [Range(0f, 1f)]
    public float fadeDarkAlpha = 0.6f;

    [Header("CHOICES")]
    public Image choice1;
    public Image choice2;
    public Image choice3;

    [Header("CHOICE SPRITES")]
    public Sprite choice1OFF;
    public Sprite choice1ON;

    public Sprite choice2OFF;
    public Sprite choice2ON;

    public Sprite choice3OFF;
    public Sprite choice3ON;

    [Header("TIMER")]
    public Image timerFill;
    public float timerDuration = 15f;

    [Header("POPUPS")]
    public GameObject popupChoice1;
    public GameObject popupChoice2;
    public GameObject popupChoice3;

    [Header("PLAYER")]
    public MonoBehaviour playerMovement;

    [Header("AUDIO")]
    public AudioSource audioSource;

    public AudioClip selectSFX;
    public AudioClip confirmSFX;
    public AudioClip timerSFX;

    [Header("CHOICE 1 BUTTONS")]
    public Image restartButton1;
    public Image exitButton1;

    [Header("CHOICE 3 BUTTONS")]
    public Image restartButton3;
    public Image exitButton3;

    [Header("BUTTON SPRITES")]
    public Sprite restartOFF;
    public Sprite restartON;

    public Sprite exitOFF;
    public Sprite exitON;

    bool uiActive = false;
    bool popupOpened = false;

    int currentChoice = 0;

    int currentPopupButton = 0;
    int currentPopupType = 0;

    float currentTimer;

    void Start()
    {
        quickDecisionUI.SetActive(false);

        popupChoice1.SetActive(false);
        popupChoice2.SetActive(false);
        popupChoice3.SetActive(false);

        quickDecisionCanvas.alpha = 0;

        // FADE IMAGE
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;

            fadeImage.gameObject.SetActive(false);
        }

        UpdateChoiceVisual();
    }

    void Update()
    {
        if (!uiActive) return;

        HandleInput();

        HandleTimer();
    }

    // =========================================
    // START DECISION
    // =========================================

    public void StartDecision()
    {
        if (uiActive) return;

        StartCoroutine(StartDecisionRoutine());
    }

    IEnumerator StartDecisionRoutine()
    {
        uiActive = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        quickDecisionUI.SetActive(true);

        // FADE BACKGROUND ON
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);

            StartCoroutine(FadeBackground(0, fadeDarkAlpha, 1f));
        }

        currentChoice = 0;

        currentTimer = 0;

        timerFill.fillAmount = 0;

        UpdateChoiceVisual();

        PlaySFX(timerSFX);

        yield return StartCoroutine(FadeCanvas(0, 1, 1f));
    }

    // =========================================
    // TIMER
    // =========================================

    void HandleTimer()
    {
        if (popupOpened) return;

        currentTimer += Time.deltaTime;

        timerFill.fillAmount = currentTimer / timerDuration;

        if (currentTimer >= timerDuration)
        {
            SceneManager.LoadScene("Level2");
        }
    }

    // =========================================
    // INPUT
    // =========================================

    void HandleInput()
    {
        // =====================================
        // POPUP INPUT
        // =====================================

        if (popupOpened)
        {
            // CHOICE 2
            if (currentPopupType == 2)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    PlaySFX(confirmSFX);

                    Debug.Log("PINDAH SCENE NANTI");
                }

                return;
            }

            // RIGHT
            if (Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentPopupButton++;

                if (currentPopupButton > 1)
                    currentPopupButton = 0;

                UpdatePopupVisual();

                PlaySFX(selectSFX);
            }

            // LEFT
            if (Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentPopupButton--;

                if (currentPopupButton < 0)
                    currentPopupButton = 1;

                UpdatePopupVisual();

                PlaySFX(selectSFX);
            }

            // ENTER
            if (Input.GetKeyDown(KeyCode.Return))
            {
                PlaySFX(confirmSFX);

                // POPUP 1
                if (currentPopupType == 1)
                {
                    if (currentPopupButton == 0)
                    {
                        RestartLevel();
                    }
                    else
                    {
                        ExitGame();
                    }
                }

                // POPUP 3
                if (currentPopupType == 3)
                {
                    if (currentPopupButton == 0)
                    {
                        RestartLevel();
                    }
                    else
                    {
                        ExitGame();
                    }
                }
            }

            return;
        }

        // =====================================
        // MAIN CHOICE INPUT
        // =====================================

        // RIGHT
        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentChoice++;

            if (currentChoice > 2)
                currentChoice = 0;

            UpdateChoiceVisual();

            PlaySFX(selectSFX);
        }

        // LEFT
        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentChoice--;

            if (currentChoice < 0)
                currentChoice = 2;

            UpdateChoiceVisual();

            PlaySFX(selectSFX);
        }

        // ENTER
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaySFX(confirmSFX);

            popupOpened = true;

            currentPopupButton = 0;

            // CHOICE 1
            if (currentChoice == 0)
            {
                popupChoice1.SetActive(true);

                currentPopupType = 1;

                UpdatePopupVisual();
            }

            // CHOICE 2
            if (currentChoice == 1)
            {
                popupChoice2.SetActive(true);

                currentPopupType = 2;
            }

            // CHOICE 3
            if (currentChoice == 2)
            {
                popupChoice3.SetActive(true);

                currentPopupType = 3;

                UpdatePopupVisual();
            }
        }
    }

    // =========================================
    // CHOICE VISUAL
    // =========================================

    void UpdateChoiceVisual()
    {
        choice1.sprite = choice1OFF;
        choice2.sprite = choice2OFF;
        choice3.sprite = choice3OFF;

        if (currentChoice == 0)
        {
            choice1.sprite = choice1ON;
        }

        if (currentChoice == 1)
        {
            choice2.sprite = choice2ON;
        }

        if (currentChoice == 2)
        {
            choice3.sprite = choice3ON;
        }
    }

    // =========================================
    // POPUP BUTTON VISUAL
    // =========================================

    void UpdatePopupVisual()
    {
        restartButton1.sprite = restartOFF;
        exitButton1.sprite = exitOFF;

        restartButton3.sprite = restartOFF;
        exitButton3.sprite = exitOFF;

        // POPUP 1
        if (currentPopupType == 1)
        {
            if (currentPopupButton == 0)
            {
                restartButton1.sprite = restartON;
            }
            else
            {
                exitButton1.sprite = exitON;
            }
        }

        // POPUP 3
        if (currentPopupType == 3)
        {
            if (currentPopupButton == 0)
            {
                restartButton3.sprite = restartON;
            }
            else
            {
                exitButton3.sprite = exitON;
            }
        }
    }

    // =========================================
    // FADE CANVAS
    // =========================================

    IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            quickDecisionCanvas.alpha =
                Mathf.Lerp(start, end, t / duration);

            yield return null;
        }

        quickDecisionCanvas.alpha = end;
    }

    // =========================================
    // FADE BACKGROUND
    // =========================================

    IEnumerator FadeBackground(float start, float end, float duration)
    {
        float t = 0;

        Color c = fadeImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(start, end, t / duration);

            fadeImage.color = c;

            yield return null;
        }

        c.a = end;

        fadeImage.color = c;
    }

    // =========================================
    // AUDIO
    // =========================================

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // =========================================
    // BUTTONS
    // =========================================

    public void RestartLevel()
    {
        SceneManager.LoadScene("Level2");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}