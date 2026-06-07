using UnityEngine;
using UnityEngine.UI;
using TMP_Text = TMPro.TMP_Text;
using UnityEngine.SceneManagement;
using System.Collections;

public class MekanikDecision : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; 

    [Header("--- CUTSCENE REFERENCE ---")]
    public CutsceneTyping cutsceneManager;

    [Header("Object UI Decision")]
    [Tooltip("Drag objek kosong pembungkus kartu & timer ke sini (bukan win panel/fade image)")]
    public GameObject kontenDecisionPanel; 
    public Image cardKiriUI;
    public Image cardKananUI;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip chooseClick;
    public AudioClip berhasilLevel;

    [Header("Sprite Card Kiri")]
    public Sprite gambarKiriIjo;
    public Sprite gambarKiriPolos;

    [Header("Sprite Card Kanan")]
    public Sprite gambarKananIjo;
    public Sprite gambarKananPolos;

    [Header("Timer")]
    public Image barMerahTimer;
    public TMP_Text teksCountdown;
    public float waktuMaksimal = 15f;

    private float waktuBerjalan;

    [Header("Popup")]
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("Fade Background Effect")]
    [Tooltip("Drag UI FadeImage yang punya komponen CanvasGroup ke sini")]
    public CanvasGroup fadeImageCanvasGroup; 

    [Header("Button Win")]
    public Button proceedButton;

    [Header("Button Game Over")]
    public Image buttonRestartUI;
    public Image buttonExitUI;
    public Button restartButton;
    public Button exitButton;
    public Sprite restartIjo;
    public Sprite restartPolos;
    public Sprite exitIjo;
    public Sprite exitPolos;

    [Header("Text Game Over")]
    public TMP_Text restartText;
    public TMP_Text exitText;

    private bool pilihKiri = true;
    private bool gameSelesai = false;
    private bool isGameOverActive = false;
    private bool pilihRestart = true;

    [Header("Timer Audio")]
    public AudioSource timerSource;
    public AudioClip timerSFX;

    [Header("Game Over Audio")]
    public AudioSource gameOverSource;
    public AudioClip hentiJantungSFX;

    [Header("Monitor Jantung")]
    public AudioSource monitorSource;
    public AudioClip monitorJantung;

    private bool espLeftHoldLastFrame = false;
    private bool espRightHoldLastFrame = false;
    private bool espLeftThumbPressed = false;
    private bool espRightThumbPressed = false;

    void Start()
    {
        waktuBerjalan = waktuMaksimal;
        pilihKiri = true;
        gameSelesai = false;
        isGameOverActive = false;
        pilihRestart = true;

        Time.timeScale = 1f;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (fadeImageCanvasGroup != null)
        {
            fadeImageCanvasGroup.gameObject.SetActive(true);
            
            // 🛡️ PROTEKSI OTOMATIS: Ambil komponen Image dan paksa warnanya jadi Hitam Pekat (Alpha 255)
            Image imgKomponen = fadeImageCanvasGroup.GetComponent<Image>();
            if (imgKomponen != null)
            {
                imgKomponen.color = new Color(0f, 0f, 0f, 1f); // Mencegah Alpha Image bernilai 0 di Inspector
            }

            // Atur transparansi utama lewat CanvasGroup (set ke 0 dulu biar tidak nutupin gameplay awal)
            fadeImageCanvasGroup.alpha = 0f;
            fadeImageCanvasGroup.blocksRaycasts = false; 
        }

        if (proceedButton != null)
            proceedButton.onClick.AddListener(LanjutKeCutscene);

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });
        }

        if (timerSource != null && timerSFX != null)
        {
            timerSource.clip = timerSFX;
            timerSource.loop = true;
            timerSource.Play();
        }

        if (esp32Input == null)
        {
            esp32Input = FindAnyObjectByType<ESP32Input>();
        }

        if (cutsceneManager == null)
        {
            cutsceneManager = FindAnyObjectByType<CutsceneTyping>();
        }

        UpdatePilihanCard();
        UpdateGameOverButton();
    }

    void Update()
    {
        HandleESP32JoystickThresholds();

        if (isGameOverActive)
        {
            NavigasiGameOver();
            return;
        }

        if (gameSelesai)
        {
            HandleWinPanelInput();
            return;
        }

        UpdateTimer();
        InputPilihan();
    }

    void UpdateTimer()
    {
        if (waktuBerjalan > 0)
        {
            waktuBerjalan -= Time.deltaTime;

            if (waktuBerjalan < 0)
                waktuBerjalan = 0;

            if (barMerahTimer != null)
                barMerahTimer.fillAmount = waktuBerjalan / waktuMaksimal;

            if (teksCountdown != null)
                teksCountdown.text = Mathf.CeilToInt(waktuBerjalan).ToString();
        }
        else
        {
            TriggerGameOver();
        }
    }

    void InputPilihan()
    {
        if (espLeftThumbPressed)
        {
            pilihKiri = true;
            UpdatePilihanCard();

            if (audioSource != null && chooseClick != null)
                audioSource.PlayOneShot(chooseClick);
        }

        if (espRightThumbPressed)
        {
            pilihKiri = false;
            UpdatePilihanCard();

            if (audioSource != null && chooseClick != null)
                audioSource.PlayOneShot(chooseClick);
        }

        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) ||
                                Input.GetKeyDown(KeyCode.Space) ||
                                (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed);

        if (isConfirmPressed)
        {
            if (pilihKiri)
                Menang();
            else
                TriggerGameOver();
        }
    }

    void UpdatePilihanCard()
    {
        if (pilihKiri)
        {
            if (cardKiriUI != null) cardKiriUI.sprite = gambarKiriIjo;
            if (cardKananUI != null) cardKananUI.sprite = gambarKananPolos;
        }
        else
        {
            if (cardKananUI != null) cardKananUI.sprite = gambarKananIjo;
            if (cardKiriUI != null) cardKiriUI.sprite = gambarKiriPolos;
        }
    }

    public void Menang()
    {
        if (gameSelesai) return;
        gameSelesai = true;

        if (audioSource != null && berhasilLevel != null)
            audioSource.PlayOneShot(berhasilLevel);

        Debug.Log("MENANG! Membuka Win Panel & Set Latar Belakang Hitam.");

        // Langsung paksa CanvasGroup naik ke Alpha 240 (0.94f)
        if (fadeImageCanvasGroup != null)
        {
            fadeImageCanvasGroup.blocksRaycasts = true;
            fadeImageCanvasGroup.alpha = 0.94f; 
        }

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void LanjutKeCutscene()
    {
        if (audioSource != null && chooseClick != null)
            audioSource.PlayOneShot(chooseClick);

        if (winPanel != null)
            winPanel.SetActive(false); 

        if (kontenDecisionPanel != null)
        {
            kontenDecisionPanel.SetActive(false);
        }
        else
        {
            if (cardKiriUI != null) cardKiriUI.gameObject.SetActive(false);
            if (cardKananUI != null) cardKananUI.gameObject.SetActive(false);
            if (barMerahTimer != null) barMerahTimer.transform.parent.gameObject.SetActive(false);
        }

        if (cutsceneManager != null)
        {
            cutsceneManager.PlayCutscene(null);
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Loading 1 to 2");
        }
    }

    void HandleWinPanelInput()
    {
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) ||
                                Input.GetKeyDown(KeyCode.Space) ||
                                (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed);

        if (isConfirmPressed && winPanel != null && winPanel.activeSelf)
        {
            LanjutKeCutscene();
        }
    }

    void TriggerGameOver()
    {
        if (isGameOverActive) return;
        isGameOverActive = true;

        if (timerSource != null) timerSource.Stop();
        if (monitorSource != null) monitorSource.Stop();

        if (gameOverSource != null && hentiJantungSFX != null)
        {
            gameOverSource.PlayOneShot(hentiJantungSFX);
        }

        Debug.Log("GAME OVER!");

        // Langsung paksa CanvasGroup naik ke Alpha 240 (0.94f)
        if (fadeImageCanvasGroup != null)
        {
            fadeImageCanvasGroup.blocksRaycasts = true;
            fadeImageCanvasGroup.alpha = 0.94f; 
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        UpdateGameOverButton();
    }

    public void MulaiMonitorJantung()
    {
        if (monitorSource != null && monitorJantung != null)
        {
            monitorSource.clip = monitorJantung;
            monitorSource.loop = true;
            monitorSource.Play();
        }
    }

    void UpdateGameOverButton()
    {
        if (buttonRestartUI == null || buttonExitUI == null) return;

        if (pilihRestart)
        {
            buttonRestartUI.sprite = restartIjo;
            buttonExitUI.sprite = exitPolos;

            if (restartText != null) restartText.color = new Color32(125, 185, 171, 255);
            if (exitText != null) exitText.color = new Color32(120, 120, 120, 255);
        }
        else
        {
            buttonRestartUI.sprite = restartPolos;
            buttonExitUI.sprite = exitIjo;

            if (restartText != null) restartText.color = new Color32(120, 120, 120, 255);
            if (exitText != null) exitText.color = new Color32(125, 185, 171, 255);
        }
    }

    void NavigasiGameOver()
    {
        if (espLeftThumbPressed)
        {
            pilihRestart = true;
            UpdateGameOverButton();
        }

        if (espRightThumbPressed)
        {
            pilihRestart = false;
            UpdateGameOverButton();
        }

        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) ||
                                Input.GetKeyDown(KeyCode.Space) ||
                                (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed);

        if (isConfirmPressed)
        {
            if (pilihRestart)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    void HandleESP32JoystickThresholds()
    {
        espLeftThumbPressed = false;
        espRightThumbPressed = false;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float espHorizontal = 0f;

        if (esp32Input != null && esp32Input.isConnected)
        {
            var type = esp32Input.GetType();
            var fieldHorizontal = type.GetField("horizontalValue") ?? type.GetField("joystickX") ?? type.GetField("horizontal");

            if (fieldHorizontal != null)
            {
                espHorizontal = (float)fieldHorizontal.GetValue(esp32Input);
            }
            else
            {
                espHorizontal = horizontalInput;
            }
        }

        bool leftRaw = (horizontalInput < -0.5f) || (espHorizontal < -0.5f) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightRaw = (horizontalInput > 0.5f) || (espHorizontal > 0.5f) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        if (leftRaw)
        {
            if (!espLeftHoldLastFrame)
            {
                espLeftThumbPressed = true;
                espLeftHoldLastFrame = true;
            }
        }
        else
        {
            espLeftHoldLastFrame = false;
        }

        if (rightRaw)
        {
            if (!espRightHoldLastFrame)
            {
                espRightThumbPressed = true;
                espRightHoldLastFrame = true;
            }
        }
        else
        {
            espRightHoldLastFrame = false;
        }
    }
}