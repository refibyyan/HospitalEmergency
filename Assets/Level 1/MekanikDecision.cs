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

    [Header("Button Game Over")]
    public Sprite restartIjo;
    public Sprite restartPolos;
    public Sprite exitIjo;
    public Sprite exitPolos;

    [Header("Text Game Over")]
    public TMP_Text restartText;
    public TMP_Text exitText;

    private Image buttonRestartUI;
    private Image buttonExitUI;

    private bool pilihKiri = true;
    private bool gameSelesai = false;
    private bool isGameOverActive = false;
    private bool pilihRestart = true;

    void Start()
    {
        waktuBerjalan = waktuMaksimal;
        pilihKiri = true;
        gameSelesai = false;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (barMerahTimer != null)
            barMerahTimer.fillAmount = 1f;

        UpdatePilihanCard();
    }

    void Update()
    {
        // kalau game over aktif
        if (isGameOverActive)
        {
            NavigasiGameOver();
            return;
        }

        // kalau game selesai stop
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

            // biar ga minus
            if (waktuBerjalan < 0)
                waktuBerjalan = 0;

            // update timer bar
            if (barMerahTimer != null)
                barMerahTimer.fillAmount = waktuBerjalan / waktuMaksimal;

            // update countdown text
            if (teksCountdown != null)
                teksCountdown.text = Mathf.CeilToInt(waktuBerjalan).ToString();
        }
        else
        {
            // langsung game over otomatis
            if (!isGameOverActive)
            {
                WaktuHabis();
            }
        }
    }

    void InputPilihan()
    {
        // pindah kiri
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihKiri = true;
            UpdatePilihanCard();
        }

        // pindah kanan
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihKiri = false;
            UpdatePilihanCard();
        }

        // enter = menang
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            Menang();
        }
    }

    void UpdatePilihanCard()
    {
        if (pilihKiri)
        {
            cardKiriUI.sprite = gambarKiriIjo;
            cardKananUI.sprite = gambarKananPolos;
        }
        else
        {
            cardKiriUI.sprite = gambarKiriPolos;
            cardKananUI.sprite = gambarKananIjo;
        }
    }

    void Menang()
    {
        // biar ga dipanggil berkali-kali
        if (gameSelesai)
            return;

        gameSelesai = true;

        // munculin popup menang
        if (winPanel != null)
            winPanel.SetActive(true);

        // pause game
        Time.timeScale = 0f;
    }

    void WaktuHabis()
    {
        TriggerGameOver();
    }

    void TriggerGameOver()
    {
        // biar ga dipanggil berkali-kali
        if (isGameOverActive)
            return;

        isGameOverActive = true;

        // munculin popup
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // cari button
        Transform restart = gameOverPanel.transform.Find("restart");
        Transform exit = gameOverPanel.transform.Find("exit");

        // ambil image
        if (restart != null)
            buttonRestartUI = restart.GetComponent<Image>();

        if (exit != null)
            buttonExitUI = exit.GetComponent<Image>();

        // default pilih restart
        pilihRestart = true;

        UpdateGameOverButton();

        // pause game
        Time.timeScale = 0f;
    }

    void NavigasiGameOver()
    {
        // pilih restart
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihRestart = true;
            UpdateGameOverButton();
        }

        // pilih exit
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihRestart = false;
            UpdateGameOverButton();
        }

        // enter / spasi
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

    void UpdateGameOverButton()
    {
        if (buttonRestartUI == null || buttonExitUI == null)
            return;

        if (pilihRestart)
        {
            // border ijo restart
            buttonRestartUI.sprite = restartIjo;
            buttonExitUI.sprite = exitPolos;

            // warna text
            restartText.color = new Color32(125, 185, 171, 255);
            exitText.color = new Color32(120, 120, 120, 255);
        }
        else
        {
            // border ijo exit
            buttonRestartUI.sprite = restartPolos;
            buttonExitUI.sprite = exitIjo;

            // warna text
            restartText.color = new Color32(120, 120, 120, 255);
            exitText.color = new Color32(125, 185, 171, 255);
        }
    }
}