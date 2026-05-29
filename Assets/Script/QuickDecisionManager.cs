using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using TMPro;

public class QuickDecisionManager : MonoBehaviour
{
    [Header("MAIN UI")]
    public GameObject quickDecisionTemplate;

    public GameObject popupChoice1;
    public GameObject popupChoice2;
    public GameObject popupChoice3;

    public GameObject popupDarkOverlay;

    [Header("TOP LEFT POPUP OBJ")]
    public GameObject popupObjRestart;
    public GameObject popupObjGenerator;

    [Header("CHOICE IMAGE")]
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

    [Header("CHOICE 1 BUTTON")]
    public Image restartButton1;
    public Image exitButton1;

    [Header("CHOICE 2 BUTTON")]
    public Image continueButton2;

    [Header("CHOICE 3 BUTTON")]
    public Image restartButton3;
    public Image exitButton3;

    [Header("BUTTON SPRITES")]
    public Sprite restartOFF;
    public Sprite restartON;

    public Sprite exitOFF;
    public Sprite exitON;

    public Sprite continueOFF;
    public Sprite continueON;

    [Header("PLAYER")]
    public PlayerMovement playerMovement;

    [Header("GLOBAL LIGHT")]
    public Light2D globalLight;

    [Header("MAIN TIMER")]
    public Image timerFill;

    public float timerDuration = 15f;

    [Header("MISSION TIMER")]
    public GameObject timerTemplate;

    public TMP_Text timerText;

    public float missionTimerDuration = 20f;

    [Header("AUDIO")]
    public AudioSource audioSource;

    public AudioClip selectSFX;
    public AudioClip confirmSFX;
    public AudioClip timerSFX;

    [Header("MISSION TIMER AUDIO")]
    public AudioSource missionTimerAudioSource;

    public AudioClip missionTimerSFX;

    // =========================================
    // STATE
    // =========================================

    bool waitingRestartTrigger = false;

    bool waitingGeneratorTrigger = false;

    bool uiActive = false;

    bool popupOpened = false;

    bool eventStarted = false;

    bool triggerUsed = false;

    bool missionTimerRunning = false;

    int currentChoice = 0;

    int currentPopupButton = 0;

    int currentPopupType = 0;
    // 1 = popup1
    // 2 = popup2
    // 3 = popup3

    float currentTimer;

    float currentMissionTimer;

    // =========================================
    // START
    // =========================================

    void Start()
    {
        if (quickDecisionTemplate != null)
        {
            quickDecisionTemplate.SetActive(false);
        }

        if (popupChoice1 != null)
        {
            popupChoice1.SetActive(false);
        }

        if (popupChoice2 != null)
        {
            popupChoice2.SetActive(false);
        }

        if (popupChoice3 != null)
        {
            popupChoice3.SetActive(false);
        }

        if (popupDarkOverlay != null)
        {
            popupDarkOverlay.SetActive(false);
        }

        if (popupObjRestart != null)
        {
            popupObjRestart.SetActive(false);
        }

        if (popupObjGenerator != null)
        {
            popupObjGenerator.SetActive(false);
        }

        // TIMER TEMPLATE OFF
        if (timerTemplate != null)
        {
            timerTemplate.SetActive(false);
        }

        UpdateChoiceVisual();

        UpdatePopupVisual();
    }

    // =========================================
    // UPDATE
    // =========================================

    void Update()
    {
        if (uiActive && !popupOpened)
        {
            HandleMainInput();

            HandleTimer();
        }

        if (popupOpened)
        {
            HandlePopupInput();
        }

        // MISSION TIMER
        if (missionTimerRunning)
        {
            HandleMissionTimer();
        }
    }

    // =========================================
    // START DECISION
    // =========================================

    public void StartDecision()
    {
        // trigger hanya sekali
        if (triggerUsed)
            return;

        if (uiActive)
            return;

        triggerUsed = true;

        uiActive = true;

        currentChoice = 0;

        currentTimer = 0f;

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
        }

        if (quickDecisionTemplate != null)
        {
            quickDecisionTemplate.SetActive(true);
        }

        // overlay MATI saat quick decision
        if (popupDarkOverlay != null)
        {
            popupDarkOverlay.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        UpdateChoiceVisual();

        // TIMER SFX
        if (audioSource != null &&
            timerSFX != null)
        {
            audioSource.clip = timerSFX;

            audioSource.loop = true;

            audioSource.Play();
        }
    }

    // =========================================
    // TIMER
    // =========================================

    void HandleTimer()
    {
        currentTimer += Time.deltaTime;

        if (timerFill != null)
        {
            timerFill.fillAmount =
                currentTimer / timerDuration;
        }

        if (currentTimer >= timerDuration)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }
    }

    // =========================================
    // MISSION TIMER
    // =========================================

    void StartMissionTimer()
    {
        missionTimerRunning = true;

        currentMissionTimer = missionTimerDuration;

        if (timerTemplate != null)
        {
            timerTemplate.SetActive(true);
        }

        UpdateMissionTimerUI();

        // TIMER SFX LOOP
        if (missionTimerAudioSource != null &&
            missionTimerSFX != null)
        {
            missionTimerAudioSource.clip = missionTimerSFX;

            missionTimerAudioSource.loop = true;

            missionTimerAudioSource.Play();
        }
    }

    void HandleMissionTimer()
    {
        currentMissionTimer -= Time.deltaTime;

        UpdateMissionTimerUI();

        if (currentMissionTimer <= 0f)
        {
            currentMissionTimer = 0f;

            missionTimerRunning = false;

            StopMissionTimerSFX();

            SceneManager.LoadScene("level 2");
        }
    }

    void UpdateMissionTimerUI()
    {
        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(currentMissionTimer).ToString();
        }
    }

    void StopMissionTimer()
    {
        missionTimerRunning = false;

        if (timerTemplate != null)
        {
            timerTemplate.SetActive(false);
        }

        StopMissionTimerSFX();
    }

    void StopMissionTimerSFX()
    {
        if (missionTimerAudioSource != null)
        {
            missionTimerAudioSource.Stop();
        }
    }

    // =========================================
    // MAIN INPUT
    // =========================================

    void HandleMainInput()
    {
        // RIGHT
        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentChoice++;

            if (currentChoice > 2)
            {
                currentChoice = 0;
            }

            UpdateChoiceVisual();

            PlaySFX(selectSFX);
        }

        // LEFT
        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentChoice--;

            if (currentChoice < 0)
            {
                currentChoice = 2;
            }

            UpdateChoiceVisual();

            PlaySFX(selectSFX);
        }

        // ENTER
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaySFX(confirmSFX);

            // =====================================
            // CHOICE 1
            // =====================================

            if (currentChoice == 0)
            {
                waitingRestartTrigger = true;

                HideAllDecisionUI();

                if (popupObjRestart != null)
                {
                    popupObjRestart.SetActive(true);
                }

                // global light GELAP
                if (globalLight != null)
                {
                    globalLight.intensity = 0.01f;
                }

                if (playerMovement != null)
                {
                    playerMovement.canMove = true;
                }

                // START TIMER
                StartMissionTimer();
            }

            // =====================================
            // CHOICE 2
            // =====================================

            if (currentChoice == 1)
            {
                waitingGeneratorTrigger = true;

                HideAllDecisionUI();

                if (popupObjGenerator != null)
                {
                    popupObjGenerator.SetActive(true);
                }

                // SAMA kayak choice1
                if (globalLight != null)
                {
                    globalLight.intensity = 0.01f;
                }

                if (playerMovement != null)
                {
                    playerMovement.canMove = true;
                }

                // START TIMER
                StartMissionTimer();
            }

            // =====================================
            // CHOICE 3
            // =====================================

            if (currentChoice == 2)
            {
                popupOpened = true;

                currentPopupType = 3;

                currentPopupButton = 0;

                if (popupDarkOverlay != null)
                {
                    popupDarkOverlay.SetActive(true);
                }

                if (popupChoice3 != null)
                {
                    popupChoice3.SetActive(true);
                }

                UpdatePopupVisual();
            }
        }
    }

    // =========================================
    // POPUP INPUT
    // =========================================

    void HandlePopupInput()
    {
        // =====================================
        // POPUP CHOICE 2
        // =====================================

        if (currentPopupType == 2)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                PlaySFX(confirmSFX);

                popupChoice2.SetActive(false);

                popupDarkOverlay.SetActive(false);

                popupOpened = false;

                if (playerMovement != null)
                {
                    playerMovement.canMove = true;
                }
            }

            return;
        }

        // =====================================
        // POPUP 1 & 3
        // =====================================

        if (Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPopupButton++;

            if (currentPopupButton > 1)
            {
                currentPopupButton = 0;
            }

            UpdatePopupVisual();

            PlaySFX(selectSFX);
        }

        if (Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentPopupButton--;

            if (currentPopupButton < 0)
            {
                currentPopupButton = 1;
            }

            UpdatePopupVisual();

            PlaySFX(selectSFX);
        }

        // ENTER
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaySFX(confirmSFX);

            // RESTART
            if (currentPopupButton == 0)
            {
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().name
                );
            }

            // EXIT
            else
            {
                Application.Quit();
            }
        }
    }

    // =========================================
    // CHOICE VISUAL
    // =========================================

    void UpdateChoiceVisual()
    {
        if (choice1 != null)
        {
            choice1.sprite = choice1OFF;
        }

        if (choice2 != null)
        {
            choice2.sprite = choice2OFF;
        }

        if (choice3 != null)
        {
            choice3.sprite = choice3OFF;
        }

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
    // POPUP VISUAL
    // =========================================

    void UpdatePopupVisual()
    {
        // RESET POPUP 1
        if (restartButton1 != null)
        {
            restartButton1.sprite = restartOFF;
        }

        if (exitButton1 != null)
        {
            exitButton1.sprite = exitOFF;
        }

        // RESET POPUP 2
        if (continueButton2 != null)
        {
            continueButton2.sprite = continueOFF;
        }

        // RESET POPUP 3
        if (restartButton3 != null)
        {
            restartButton3.sprite = restartOFF;
        }

        if (exitButton3 != null)
        {
            exitButton3.sprite = exitOFF;
        }

        // =====================================
        // POPUP 1
        // =====================================

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

        // =====================================
        // POPUP 2
        // =====================================

        if (currentPopupType == 2)
        {
            continueButton2.sprite = continueON;
        }

        // =====================================
        // POPUP 3
        // =====================================

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
    // HIDE UI
    // =========================================

    void HideAllDecisionUI()
    {
        if (quickDecisionTemplate != null)
        {
            quickDecisionTemplate.SetActive(false);
        }

        if (popupChoice1 != null)
        {
            popupChoice1.SetActive(false);
        }

        if (popupChoice2 != null)
        {
            popupChoice2.SetActive(false);
        }

        if (popupChoice3 != null)
        {
            popupChoice3.SetActive(false);
        }

        if (popupDarkOverlay != null)
        {
            popupDarkOverlay.SetActive(false);
        }

        // STOP TIMER AUDIO
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        uiActive = false;

        popupOpened = false;
    }

    // =========================================
    // PUBLIC TRIGGER CALL
    // =========================================

    public void TriggerRestartEvent()
    {
        if (eventStarted)
            return;

        if (!waitingRestartTrigger)
            return;

        // STOP TIMER
        StopMissionTimer();

        StartCoroutine(RestartLightEvent());
    }

    public void TriggerGeneratorEvent()
    {
        if (eventStarted)
            return;

        if (!waitingGeneratorTrigger)
            return;

        // STOP TIMER
        StopMissionTimer();

        StartCoroutine(GeneratorEvent());
    }

    // =========================================
    // RESTART EVENT
    // =========================================

    IEnumerator RestartLightEvent()
    {
        eventStarted = true;

        waitingRestartTrigger = false;

        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        if (popupObjRestart != null)
        {
            popupObjRestart.SetActive(false);
        }

        float timer = 0f;

        while (timer < 2.5f)
        {
            timer += 0.2f;

            globalLight.intensity = 1f;

            yield return new WaitForSeconds(0.1f);

            globalLight.intensity = 0.2f;

            yield return new WaitForSeconds(0.1f);
        }

        popupOpened = true;

        currentPopupType = 1;

        currentPopupButton = 0;

        popupDarkOverlay.SetActive(true);

        popupChoice1.SetActive(true);

        UpdatePopupVisual();
    }

    // =========================================
    // GENERATOR EVENT
    // =========================================

    IEnumerator GeneratorEvent()
    {
        eventStarted = true;

        waitingGeneratorTrigger = false;

        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }

        if (popupObjGenerator != null)
        {
            popupObjGenerator.SetActive(false);
        }

        // baru nyala pas nyentuh generator
        if (globalLight != null)
        {
            globalLight.intensity = 1f;
        }

        yield return new WaitForSeconds(0.5f);

        popupOpened = true;

        currentPopupType = 2;

        popupDarkOverlay.SetActive(true);

        popupChoice2.SetActive(true);

        UpdatePopupVisual();
    }

    // =========================================
    // AUDIO
    // =========================================

    void PlaySFX(AudioClip clip)
    {
        if (clip != null &&
            audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}