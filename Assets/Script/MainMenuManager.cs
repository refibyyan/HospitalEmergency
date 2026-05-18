using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
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
    public AudioClip selectingSound; // Drag sound 'Selecting' ke sini
    public AudioClip pressedSound;  // Drag sound 'Pressed Confirm' ke sini

    private bool isExitPopUpActive = false;
    private bool isExitYesSelected = false; 

    void Start() {
        if (startButton != null) startButton.Select();
    }

    void Update() {
        if (!isExitPopUpActive) {
            // Deteksi navigasi arrow di menu utama untuk suara Selecting
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
                PlaySFX(selectingSound);
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                GameObject current = EventSystem.current.currentSelectedGameObject;

                if (current == startButton.gameObject) {
                    PlaySFX(pressedSound); // Suara saat klik Start
                    StartGame();
                } 
                else if (current == exitButton.gameObject) {
                    PlaySFX(pressedSound); // Suara saat klik Exit (buka pop-up)
                    OpenExitPopUp();
                }
                // Tambahkan check untuk tutorialButton di sini jika sudah ada
            }
        } 
        else {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
                isExitYesSelected = !isExitYesSelected;
                PlaySFX(selectingSound); // Suara saat pindah antara Yes/No di Pop-up
                RefreshExitSprite();
            }

            if (Input.GetKeyDown(KeyCode.Return)) {
                PlaySFX(pressedSound); // Suara saat konfirmasi di dalam pop-up
                if (isExitYesSelected) {
                    Debug.Log("Game Quit");
                    Application.Quit(); 
                } else {
                    CloseExitPopUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                PlaySFX(selectingSound); // Opsional: suara saat batal/escape
                CloseExitPopUp();
            }
        }
    }

    // Fungsi bantuan untuk memutar SFX
    void PlaySFX(AudioClip clip) {
        if (sfxSource != null && clip != null) {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    public void OpenExitPopUp() {
        isExitPopUpActive = true;
        if (exitPopUpObj != null) exitPopUpObj.SetActive(true);
        if (blurPanel != null) blurPanel.SetActive(true);
        
        isExitYesSelected = false; 
        RefreshExitSprite();

        EventSystem.current.SetSelectedGameObject(null);
    }

    void RefreshExitSprite() {
        if (exitImage != null) {
            exitImage.sprite = isExitYesSelected ? exitYesSprite : exitNoSprite;
        }
    }

    void CloseExitPopUp() {
        isExitPopUpActive = false;
        if (exitPopUpObj != null) exitPopUpObj.SetActive(false);
        if (blurPanel != null) blurPanel.SetActive(false);
        
        if (exitButton != null) exitButton.Select();
    }
}