using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MekanikDecision : MonoBehaviour
{
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
            restartButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });

        if (exitButton != null)
            exitButton.onClick.AddListener(() =>
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            });

        if (timerSource != null && timerSFX != null)
        {
            timerSource.clip = timerSFX;
            timerSource.loop = true;
            timerSource.Play();
        }

        UpdatePilihanCard();
        UpdateGameOverButton();
    }

    void Update()
    {
        if (isGameOverActive)
        {
            NavigasiGameOver();
            return;
        }

        if (gameSelesai)
            return;

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
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihKiri = true;
            UpdatePilihanCard();

            if (audioSource != null && chooseClick != null)
                audioSource.PlayOneShot(chooseClick);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihKiri = false;
            UpdatePilihanCard();

            if (audioSource != null && chooseClick != null)
                audioSource.PlayOneShot(chooseClick);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
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

        // Memanggil scene transisi Loading 1 to 2
        SceneManager.LoadScene("Loading 1 to 2");
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
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihRestart = true;
            UpdateGameOverButton();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihRestart = false;
            UpdateGameOverButton();
        }

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
}