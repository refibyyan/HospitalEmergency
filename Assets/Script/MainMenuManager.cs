using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("ESP32 Input Reference")]
    public ESP32Input esp32Input;

    [Header("Tutorial Manager Reference")]
    public TutorialManager tutorialManager; // Drag GameObject TutorialManager ke sini

    [Header("Menu Buttons")]
    public Button startButton;
    public Button tutorialButton;
    public Button exitButton;

    [Header("Exit Pop Up UI")]
    public GameObject exitPopUpObj;
    public Image exitImage;
    public Sprite exitYesSprite;
    public Sprite exitNoSprite;
    public GameObject blurPanel;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip selectingSound;
    public AudioClip pressedSound;

    private bool isExitPopUpActive = false;
    private bool isTutorialActive = false; // Flag status tutorial
    private bool isExitYesSelected = false;

    private bool isJoystickAxisInUse = false;

    void Start()
    {
        if (startButton != null) startButton.Select();

        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }
        if (tutorialManager == null)
        {
            tutorialManager = FindFirstObjectByType<TutorialManager>();
        }
    }

    void Update()
    {
        // Jika tutorial sedang aktif, serahkan kendali penuh ke TutorialManager
        if (isTutorialActive) return;

        // 1. Ambil input navigasi vertikal (Hybrid)
        float verticalInput = Input.GetAxisRaw("Vertical");
        if (esp32Input != null && esp32Input.isConnected)
        {
            if (Mathf.Abs(esp32Input.vertical) > 0.5f)
            {
                verticalInput = esp32Input.vertical;
            }
        }

        // 2. Ambil input konfirmasi/Enter (Hybrid)
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return);
        if (esp32Input != null && esp32Input.isConnected)
        {
            if (esp32Input.selectPressed)
            {
                isConfirmPressed = true;
            }
        }

        // 3. Deteksi arah ketukan joystick
        bool navigateUp = false;
        bool navigateDown = false;
        if (verticalInput != 0)
        {
            if (!isJoystickAxisInUse)
            {
                if (verticalInput > 0.3f) navigateUp = true;
                if (verticalInput < -0.3f) navigateDown = true;
                isJoystickAxisInUse = true;
            }
        }
        else
        {
            isJoystickAxisInUse = false;
        }

        // ====================================================================
        // LOGIKAL NAVIGATION
        // ====================================================================

        if (!isExitPopUpActive)
        {
            if (navigateUp || navigateDown)
            {
                PlaySFX(selectingSound);
                NavigateMainMenuButtons(navigateUp);
            }

            if (isConfirmPressed)
            {
                GameObject current = EventSystem.current.currentSelectedGameObject;

                if (current == startButton.gameObject)
                {
                    PlaySFX(pressedSound);
                    StartGame();
                }
                else if (current == tutorialButton.gameObject)
                {
                    PlaySFX(pressedSound);
                    OpenTutorial(); // Membuka Tutorial Pop-Up
                }
                else if (current == exitButton.gameObject)
                {
                    PlaySFX(pressedSound);
                    OpenExitPopUp();
                }
            }
        }
        else
        {
            if (navigateUp || navigateDown)
            {
                isExitYesSelected = !isExitYesSelected;
                PlaySFX(selectingSound);
                RefreshExitSprite();
            }

            if (isConfirmPressed)
            {
                PlaySFX(pressedSound);
                if (isExitYesSelected)
                {
                    Debug.Log("Game Quit");
                    Application.Quit();
                }
                else
                {
                    CloseExitPopUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PlaySFX(selectingSound);
                CloseExitPopUp();
            }
        }
    }

    private void NavigateMainMenuButtons(bool up)
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected == null)
        {
            if (startButton != null) startButton.Select();
            return;
        }

        if (up)
        {
            if (currentSelected == tutorialButton.gameObject) startButton?.Select();
            else if (currentSelected == exitButton.gameObject) tutorialButton?.Select();
        }
        else
        {
            if (currentSelected == startButton.gameObject) tutorialButton?.Select();
            else if (currentSelected == tutorialButton.gameObject) exitButton?.Select();
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    // --- TUTORIAL HANDLING ---
    void OpenTutorial()
    {
        if (tutorialManager != null)
        {
            isTutorialActive = true;
            EventSystem.current.SetSelectedGameObject(null); // Clear fokus button belakang
            tutorialManager.ShowTutorial();
        }
    }

    public void CloseTutorialPopUp()
    {
        isTutorialActive = false;
        if (tutorialButton != null) tutorialButton.Select(); // Kembalikan fokus ke tombol tutorial
    }

    // --- EXIT POPUP HANDLING ---
    public void OpenExitPopUp()
    {
        isExitPopUpActive = true;
        if (exitPopUpObj != null) exitPopUpObj.SetActive(true);
        if (blurPanel != null) blurPanel.SetActive(true);
        isExitYesSelected = false;
        RefreshExitSprite();
        EventSystem.current.SetSelectedGameObject(null);
    }

    void RefreshExitSprite()
    {
        if (exitImage != null)
        {
            exitImage.sprite = isExitYesSelected ? exitYesSprite : exitNoSprite;
        }
    }

    void CloseExitPopUp()
    {
        isExitPopUpActive = false;
        if (exitPopUpObj != null) exitPopUpObj.SetActive(false);
        if (blurPanel != null) blurPanel.SetActive(false);
        if (exitButton != null) exitButton.Select();
    }
}