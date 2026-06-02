using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MekanikDecision : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini di Inspector

    [Header("Object UI Decision")]
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

    // Tracker State internal untuk simulasi GetKeyDown (Mencegah spam menu/card)
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

        if (barMerahTimer != null)
            barMerahTimer.fillAmount = 1f;

        if (proceedButton != null)
            proceedButton.onClick.AddListener(LanjutLevel2);

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

        // Otomatis mencari script ESP32Input di hierarki jika belum di-drag manual
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }

        UpdatePilihanCard();
        UpdateGameOverButton();
    }

    void Update()
    {
        // Jalankan pemrosesan threshold joystick & WASD secara hybrid
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
        // Input KIRI Hybrid
        if (espLeftThumbPressed)
        {
            pilihKiri = true;
            UpdatePilihanCard();

            if (audioSource != null && chooseClick != null)
                audioSource.PlayOneShot(chooseClick);
        }

        // Input KANAN Hybrid
        if (espRightThumbPressed)
        {
            pilihKiri = false;
            UpdatePilihanCard();

            if (audioSource != null && chooseClick != null)
                audioSource.PlayOneShot(chooseClick);
        }

        // Input KONFIRMASI Hybrid
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
        // FIX: Sekarang visual card kanan/kiri berubah dinamis mengikuti nilai boolean 'pilihKiri'
        if (pilihKiri)
        {
            if (cardKiriUI != null) cardKiriUI.sprite = gambarKiriIjo;
            if (cardKananUI != null) cardKananUI.sprite = gambarKananPolos;
        }
        else
        {
            if (cardKiriUI != null) cardKiriUI.sprite = gambarKiriPolos;
            if (cardKananUI != null) cardKananUI.sprite = gambarKananIjo;
        }
    }

    public void Menang()
    {
        if (gameSelesai) return;

        gameSelesai = true;

        if (audioSource != null && berhasilLevel != null)
            audioSource.PlayOneShot(berhasilLevel);

        Debug.Log("MENANG!");

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void LanjutLevel2()
    {
        if (audioSource != null && chooseClick != null)
            audioSource.PlayOneShot(chooseClick);

        Time.timeScale = 1f;
        SceneManager.LoadScene("Loading 1 to 2");
    }

    void HandleWinPanelInput()
    {
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) ||
                                Input.GetKeyDown(KeyCode.Space) ||
                                (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed);

        if (isConfirmPressed && winPanel != null && winPanel.activeSelf)
        {
            LanjutLevel2();
        }
    }

    void TriggerGameOver()
    {
        if (isGameOverActive) return;

        isGameOverActive = true;

        if (timerSource != null)
            timerSource.Stop();

        if (monitorSource != null)
            monitorSource.Stop();

        if (gameOverSource != null && hentiJantungSFX != null)
        {
            gameOverSource.PlayOneShot(hentiJantungSFX);
        }

        Debug.Log("GAME OVER!");

        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

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

        // 1. Ambil input keyboard/WASD standar Unity
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Ambil nilai joystick dari script ESP32Input
        float espHorizontal = 0f;
        if (esp32Input != null && esp32Input.isConnected)
        {
            // 💡 FIX: Menggunakan refleksi otomatis untuk mendeteksi variabel joystick apa pun di ESP32Input.cs Anda
            // (Mencari otomatis jika namanya: horizontalValue, joystickX, atau horizontal)
            var type = esp32Input.GetType();
            var fieldHorizontal = type.GetField("horizontalValue") ?? type.GetField("joystickX") ?? type.GetField("horizontal");

            if (fieldHorizontal != null)
            {
                espHorizontal = (float)fieldHorizontal.GetValue(esp32Input);
            }
            else
            {
                // Jika sistem otomatis tidak mendeteksi nama variabel di script Anda, ganti manual baris di bawah ini:
                espHorizontal = horizontalInput;
            }
        }

        // 3. Gabungkan deteksi mentah (Raw Input)
        bool leftRaw = (horizontalInput < -0.5f) || (espHorizontal < -0.5f) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightRaw = (horizontalInput > 0.5f) || (espHorizontal > 0.5f) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        // --- Edge-Detection (Anti-Spam / Simulasi GetKeyDown) ---
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