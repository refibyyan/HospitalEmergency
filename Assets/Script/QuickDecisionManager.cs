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

    [Header("TRIGGER DECISION")]
    public GameObject triggerDecision;

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
    public ESP32Input esp32Input;

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

    bool waitingRestartTrigger   = false;
    bool waitingGeneratorTrigger = false;

    bool uiActive        = false;
    bool popupOpened     = false;
    bool eventStarted    = false;
    bool triggerUsed     = false;
    bool missionTimerRunning = false;

    bool inputLocked = false;

    // Flag joystick khusus UI — pisah dari karakter
    bool uiJoyInUse = false;

    int currentChoice      = 0;
    int currentPopupButton = 0;
    int currentPopupType   = 0;

    float currentTimer;
    float currentMissionTimer;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (quickDecisionTemplate != null) quickDecisionTemplate.SetActive(false);
        if (popupChoice1 != null)          popupChoice1.SetActive(false);
        if (popupChoice2 != null)          popupChoice2.SetActive(false);
        if (popupChoice3 != null)          popupChoice3.SetActive(false);
        if (popupDarkOverlay != null)      popupDarkOverlay.SetActive(false);
        if (popupObjRestart != null)       popupObjRestart.SetActive(false);
        if (popupObjGenerator != null)     popupObjGenerator.SetActive(false);
        if (timerTemplate != null)         timerTemplate.SetActive(false);
        if (triggerDecision != null)       triggerDecision.SetActive(false);

        UpdateChoiceVisual();
        UpdatePopupVisual();
    }

    // =========================================================
    // UPDATE
    // =========================================================

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

        if (missionTimerRunning)
        {
            HandleMissionTimer();
        }
    }

    // =========================================================
    // ACTIVATE DECISION
    // =========================================================

    public void ActivateDecisionTrigger()
    {
        if (triggerDecision != null)
            triggerDecision.SetActive(true);
    }

    // =========================================================
    // START DECISION
    // =========================================================

    public void StartDecision()
    {
        if (triggerUsed) return;
        if (uiActive)    return;

        triggerUsed = true;

        if (triggerDecision != null)
            triggerDecision.SetActive(false);

        uiActive      = true;
        currentChoice = 0;
        currentTimer  = 0f;

        if (timerFill != null)
            timerFill.fillAmount = 0f;

        if (quickDecisionTemplate != null)
            quickDecisionTemplate.SetActive(true);

        if (playerMovement != null)
            playerMovement.canMove = false;

        UpdateChoiceVisual();

        if (audioSource != null && timerSFX != null)
        {
            audioSource.clip = timerSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    // =========================================================
    // BACA INPUT UI — kanan/kiri/select dari keyboard atau ESP32
    // =========================================================

    void ReadUIInput(out bool right, out bool left, out bool select)
    {
        right  = false;
        left   = false;
        select = false;

        bool useKeyboard = (esp32Input == null || !esp32Input.isConnected);

        if (useKeyboard)
        {
            // ---- KEYBOARD ----
            right  = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
            left   = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
            select = Input.GetKeyDown(KeyCode.Return);
        }
        else
        {
            // ---- ESP32 JOYSTICK ----
            float h = esp32Input.horizontal;
            float sensitivity = esp32Input.joystickSensitivity;

            // Joystick ke kanan — one-shot sampai balik ke tengah
            if (!uiJoyInUse && h >= sensitivity)
            {
                right      = true;
                uiJoyInUse = true;
            }
            // Joystick ke kiri — one-shot sampai balik ke tengah
            else if (!uiJoyInUse && h <= -sensitivity)
            {
                left       = true;
                uiJoyInUse = true;
            }

            // Reset flag jika joystick kembali mendekati titik tengah (dead zone)
            if (Mathf.Abs(h) < (sensitivity * 0.5f) || Mathf.Abs(h) < 0.15f)
            {
                uiJoyInUse = false;
            }

            // Tombol select dari ESP32
            select = esp32Input.selectPressed;
        }
    }

    // =========================================================
    // MAIN INPUT (pilih choice 1 / 2 / 3)
    // =========================================================

    void HandleMainInput()
    {
        if (inputLocked) return;

        ReadUIInput(out bool right, out bool left, out bool select);

        if (right)
        {
            currentChoice++;
            if (currentChoice > 2) currentChoice = 0;

            UpdateChoiceVisual();
            PlaySFX(selectSFX);
            StartCoroutine(InputDelay());
            return; 
        }

        if (left)
        {
            currentChoice--;
            if (currentChoice < 0) currentChoice = 2;

            UpdateChoiceVisual();
            PlaySFX(selectSFX);
            StartCoroutine(InputDelay());
            return;
        }

        if (select)
        {
            PlaySFX(confirmSFX);

            // CHOICE 1 — Restart mission
            if (currentChoice == 0)
            {
                waitingRestartTrigger = true;
                HideAllDecisionUI();

                if (popupObjRestart != null)
                    popupObjRestart.SetActive(true);

                if (globalLight != null)
                    globalLight.intensity = 0.01f;

                if (playerMovement != null)
                    playerMovement.canMove = true;

                StartMissionTimer();
            }
            // CHOICE 2 — Generator mission
            else if (currentChoice == 1)
            {
                waitingGeneratorTrigger = true;
                HideAllDecisionUI();

                if (popupObjGenerator != null)
                    popupObjGenerator.SetActive(true);

                if (globalLight != null)
                    globalLight.intensity = 0.01f;

                if (playerMovement != null)
                    playerMovement.canMove = true;

                StartMissionTimer();
            }
            // CHOICE 3 — Popup restart/exit
            else if (currentChoice == 2)
            {
                popupOpened        = true;
                currentPopupType   = 3;
                currentPopupButton = 0;

                if (popupDarkOverlay != null)
                    popupDarkOverlay.SetActive(true);

                if (popupChoice3 != null)
                    popupChoice3.SetActive(true);

                UpdatePopupVisual();
            }

            StartCoroutine(InputDelay());
        }
    }

    // =========================================================
    // POPUP INPUT (pilih tombol di dalam popup)
    // =========================================================

    void HandlePopupInput()
    {
        if (inputLocked) return;

        ReadUIInput(out bool right, out bool left, out bool select);

        // ---- POPUP TYPE 2 — hanya tombol continue ----
        if (currentPopupType == 2)
        {
            if (select)
            {
                PlaySFX(confirmSFX);

                if (popupChoice2 != null)
                    popupChoice2.SetActive(false);

                if (popupDarkOverlay != null)
                    popupDarkOverlay.SetActive(false);

                popupOpened = false;

                if (playerMovement != null)
                    playerMovement.canMove = true;

                StartCoroutine(InputDelay());
            }
            return;
        }

        // ---- POPUP TYPE 1 & 3 — restart / exit ----

        if (right)
        {
            currentPopupButton++;
            if (currentPopupButton > 1) currentPopupButton = 0;

            UpdatePopupVisual();
            PlaySFX(selectSFX);
            StartCoroutine(InputDelay());
            return;
        }

        if (left)
        {
            currentPopupButton--;
            if (currentPopupButton < 0) currentPopupButton = 1;

            UpdatePopupVisual();
            PlaySFX(selectSFX);
            StartCoroutine(InputDelay());
            return;
        }

        if (select)
        {
            PlaySFX(confirmSFX);

            if (currentPopupButton == 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Application.Quit();
            }

            StartCoroutine(InputDelay());
        }
    }

    // =========================================================
    // VISUAL UPDATE (Sistem ON / OFF Sprite Lempar Di Sini)
    // =========================================================

    void UpdateChoiceVisual()
    {
        // 1. Kembalikan semua pilihan ke sprite OFF terlebih dahulu
        if (choice1 != null) choice1.sprite = choice1OFF;
        if (choice2 != null) choice2.sprite = choice2OFF;
        if (choice3 != null) choice3.sprite = choice3OFF;

        // 2. Pasangkan sprite ON pada index pilihan yang sedang aktif (0, 1, atau 2)
        switch (currentChoice)
        {
            case 0:
                if (choice1 != null) choice1.sprite = choice1ON;
                break;
            case 1:
                if (choice2 != null) choice2.sprite = choice2ON;
                break;
            case 2:
                if (choice3 != null) choice3.sprite = choice3ON;
                break;
        }
    }

    void UpdatePopupVisual()
    {
        if (restartButton1 != null)  restartButton1.sprite  = restartOFF;
        if (exitButton1 != null)     exitButton1.sprite     = exitOFF;
        if (continueButton2 != null) continueButton2.sprite = continueOFF;
        if (restartButton3 != null)  restartButton3.sprite  = restartOFF;
        if (exitButton3 != null)     exitButton3.sprite     = exitOFF;

        if (currentPopupType == 1)
        {
            if (currentPopupButton == 0)
            {
                if (restartButton1 != null) restartButton1.sprite = restartON;
            }
            else
            {
                if (exitButton1 != null) exitButton1.sprite = exitON;
            }
        }

        if (currentPopupType == 2)
        {
            if (continueButton2 != null) continueButton2.sprite = continueON;
        }

        if (currentPopupType == 3)
        {
            if (currentPopupButton == 0)
            {
                if (restartButton3 != null) restartButton3.sprite = restartON;
            }
            else
            {
                if (exitButton3 != null) exitButton3.sprite = exitON;
            }
        }
    }

    // =========================================================
    // TIMER & EVENTS (Tetap Aman Sesuai Logika Awal)
    // =========================================================

    void HandleTimer()
    {
        currentTimer += Time.deltaTime;

        if (timerFill != null)
            timerFill.fillAmount = currentTimer / timerDuration;

        if (currentTimer >= timerDuration)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void StartMissionTimer()
    {
        missionTimerRunning  = true;
        currentMissionTimer  = missionTimerDuration;

        if (timerTemplate != null)
            timerTemplate.SetActive(true);

        UpdateMissionTimerUI();

        if (missionTimerAudioSource != null && missionTimerSFX != null)
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
            currentMissionTimer  = 0f;
            missionTimerRunning  = false;
            StopMissionTimerSFX();
            SceneManager.LoadScene("level 2");
        }
    }

    void UpdateMissionTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(currentMissionTimer).ToString();
    }

    void StopMissionTimer()
    {
        missionTimerRunning = false;

        if (timerTemplate != null)
            timerTemplate.SetActive(false);

        StopMissionTimerSFX();
    }

    void StopMissionTimerSFX()
    {
        if (missionTimerAudioSource != null)
            missionTimerAudioSource.Stop();
    }

    public void TriggerRestartEvent()
    {
        if (eventStarted)            return;
        if (!waitingRestartTrigger)  return;

        StopMissionTimer();
        StartCoroutine(RestartLightEvent());
    }

    public void TriggerGeneratorEvent()
    {
        if (eventStarted)              return;
        if (!waitingGeneratorTrigger)  return;

        StopMissionTimer();
        StartCoroutine(GeneratorEvent());
    }

    IEnumerator RestartLightEvent()
    {
        eventStarted          = true;
        waitingRestartTrigger = false;

        if (playerMovement != null)
            playerMovement.canMove = false;

        if (popupObjRestart != null)
            popupObjRestart.SetActive(false);

        float timer = 0f;

        while (timer < 2.5f)
        {
            timer += 0.2f;

            if (globalLight != null) globalLight.intensity = 1f;
            yield return new WaitForSeconds(0.1f);

            if (globalLight != null) globalLight.intensity = 0.2f;
            yield return new WaitForSeconds(0.1f);
        }

        popupOpened        = true;
        currentPopupType   = 1;
        currentPopupButton = 0;

        if (popupDarkOverlay != null) popupDarkOverlay.SetActive(true);
        if (popupChoice1 != null)     popupChoice1.SetActive(true);

        UpdatePopupVisual();
    }

    IEnumerator GeneratorEvent()
    {
        eventStarted             = true;
        waitingGeneratorTrigger  = false;

        if (playerMovement != null)
            playerMovement.canMove = false;

        if (popupObjGenerator != null)
            popupObjGenerator.SetActive(false);

        if (globalLight != null)
            globalLight.intensity = 1f;

        yield return new WaitForSeconds(0.5f);

        popupOpened      = true;
        currentPopupType = 2;

        if (popupDarkOverlay != null) popupDarkOverlay.SetActive(true);
        if (popupChoice2 != null)     popupChoice2.SetActive(true);

        UpdatePopupVisual();
    }

    void HideAllDecisionUI()
    {
        if (quickDecisionTemplate != null) quickDecisionTemplate.SetActive(false);
        if (popupChoice1 != null)          popupChoice1.SetActive(false);
        if (popupChoice2 != null)          popupChoice2.SetActive(false);
        if (popupChoice3 != null)          popupChoice3.SetActive(false);
        if (popupDarkOverlay != null)      popupDarkOverlay.SetActive(false);

        if (audioSource != null)
            audioSource.Stop();

        uiActive     = false;
        popupOpened  = false;
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    IEnumerator InputDelay()
    {
        inputLocked = true;
        yield return new WaitForSeconds(0.2f);
        inputLocked = false;
    }
}