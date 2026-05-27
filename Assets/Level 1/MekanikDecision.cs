using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MekanikDecision : MonoBehaviour
{
    [Header("Object UI Decision")]
    public Image cardKiriUI;
    public Image cardKananUI;

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

    void Start()
    {
        waktuBerjalan = waktuMaksimal;

        pilihKiri = true;
        gameSelesai = false;
        isGameOverActive = false;

        Time.timeScale = 1f;

        // Hide popup awal
        if (winPanel != null)
            winPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Timer full
        if (barMerahTimer != null)
            barMerahTimer.fillAmount = 1f;

        // Tombol proceed
        if (proceedButton != null)
            proceedButton.onClick.AddListener(LanjutLevel2);

        UpdatePilihanCard();
        UpdateGameOverButton();
    }

    void Update()
    {
        // Kalau game over aktif
        if (isGameOverActive)
        {
            NavigasiGameOver();
            return;
        }

        // Kalau udah menang
        if (gameSelesai)
            return;

        UpdateTimer();
        InputPilihan();
    }

    // ================= TIMER =================
    void UpdateTimer()
    {
        if (waktuBerjalan > 0)
        {
            waktuBerjalan -= Time.deltaTime;

            if (waktuBerjalan < 0)
                waktuBerjalan = 0;

            // Update fill timer
            if (barMerahTimer != null)
                barMerahTimer.fillAmount = waktuBerjalan / waktuMaksimal;

            // Update text countdown
            if (teksCountdown != null)
                teksCountdown.text = Mathf.CeilToInt(waktuBerjalan).ToString();
        }
        else
        {
            TriggerGameOver();
        }
    }

    // ================= INPUT =================
    void InputPilihan()
    {
        // Pilih kiri
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihKiri = true;
            UpdatePilihanCard();
        }

        // Pilih kanan
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihKiri = false;
            UpdatePilihanCard();
        }

        // Confirm pilihan
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Menang();
        }
    }

    // ================= UPDATE CARD =================
    void UpdatePilihanCard()
    {
        if (pilihKiri)
        {
            if (cardKiriUI != null)
                cardKiriUI.sprite = gambarKiriIjo;

            if (cardKananUI != null)
                cardKananUI.sprite = gambarKananPolos;
        }
        else
        {
            if (cardKiriUI != null)
                cardKiriUI.sprite = gambarKiriPolos;

            if (cardKananUI != null)
                cardKananUI.sprite = gambarKananIjo;
        }
    }

    // ================= WIN =================
    public void Menang()
    {
        Debug.Log("MENANG KEPAKE");

        if (winPanel == null)
        {
            Debug.Log("WIN PANEL NULL");
            return;
        }

        winPanel.SetActive(true);
        Debug.Log("WIN PANEL SET ACTIVE");
    }

    void LanjutLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }

    // ================= GAME OVER =================
    void TriggerGameOver()
    {
        if (isGameOverActive)
            return;

        isGameOverActive = true;

        Debug.Log("GAME OVER");

        // Pause game
        Time.timeScale = 0f;

        // Munculin popup game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        UpdateGameOverButton();
    }

    // ================= NAVIGASI GAME OVER =================
    void NavigasiGameOver()
    {
        // Pilih restart
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihRestart = true;
            UpdateGameOverButton();
        }

        // Pilih exit
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihRestart = false;
            UpdateGameOverButton();
        }

        // Confirm
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
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

    // ================= UPDATE BUTTON GAME OVER =================
    void UpdateGameOverButton()
    {
        if (buttonRestartUI == null || buttonExitUI == null)
            return;

        if (pilihRestart)
        {
            buttonRestartUI.sprite = restartIjo;
            buttonExitUI.sprite = exitPolos;

            if (restartText != null)
                restartText.color = new Color32(125, 185, 171, 255);

            if (exitText != null)
                exitText.color = new Color32(120, 120, 120, 255);
        }
        else
        {
            buttonRestartUI.sprite = restartPolos;
            buttonExitUI.sprite = exitIjo;

            if (restartText != null)
                restartText.color = new Color32(120, 120, 120, 255);

            if (exitText != null)
                exitText.color = new Color32(125, 185, 171, 255);
        }
    }
}