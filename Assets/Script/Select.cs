using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Select : MonoBehaviour
{
    // Variabel statis untuk menyimpan pilihan karakter agar bisa dibaca di scene Loading1
    public static string selectedCharacter = "Lyra";

    [Header("ESP32 Input Reference")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini

    [Header("Kiro Pop Up")]
    public GameObject kiroPopUpObj;
    public Image kiroImage;
    public Sprite kiroConfirmSprite;
    public Sprite kiroCancelSprite;

    [Header("Lyra Pop Up")]
    public GameObject lyraPopUpObj;
    public Image lyraImage;
    public Sprite lyraConfirmSprite;
    public Sprite lyraCancelSprite;

    [Header("General")]
    public Button kiroButton;
    public Button lyraButton;
    public GameObject blurPanel;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip selectingSound;
    public AudioClip pressedSound;

    private bool isPopUpActive = false;
    private bool isConfirmSelected = true;
    private string activeCharacter = "";

    // Kunci Anti-Spam Joystick
    private bool isJoystickHorizontalInUse = false;
    private bool isJoystickVerticalInUse = false;

    void Start()
    {
        if (kiroButton != null) kiroButton.Select();

        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }
    }

    void Update()
    {
        // -----------------------------------------------------------------
        // AMBIL INPUT HYBRID (KEYBOARD & ESP32)
        // -----------------------------------------------------------------
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (esp32Input != null && esp32Input.isConnected)
        {
            if (Mathf.Abs(esp32Input.horizontal) > 0.5f) horizontalInput = esp32Input.horizontal;
            if (Mathf.Abs(esp32Input.vertical) > 0.5f) verticalInput = esp32Input.vertical;
        }

        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
        {
            isConfirmPressed = true;
        }

        // -----------------------------------------------------------------
        // PROSES ONE-SHOT NAVIGASI
        // -----------------------------------------------------------------
        bool navigateLeft = false;
        bool navigateRight = false;
        bool navigateUp = false;
        bool navigateDown = false;

        // Cek Horizontal
        if (horizontalInput != 0)
        {
            if (!isJoystickHorizontalInUse)
            {
                if (horizontalInput < -0.3f) navigateLeft = true;
                if (horizontalInput > 0.3f) navigateRight = true;
                isJoystickHorizontalInUse = true;
            }
        }
        else
        {
            isJoystickHorizontalInUse = false;
        }

        // Cek Vertikal
        if (verticalInput != 0)
        {
            if (!isJoystickVerticalInUse)
            {
                if (verticalInput > 0.3f) navigateUp = true;
                if (verticalInput < -0.3f) navigateDown = true;
                isJoystickVerticalInUse = true;
            }
        }
        else
        {
            isJoystickVerticalInUse = false;
        }

        // Tambahan input tombol keyboard cadangan (A/D & W/S)
        if (Input.GetKeyDown(KeyCode.A)) navigateLeft = true;
        if (Input.GetKeyDown(KeyCode.D)) navigateRight = true;
        if (Input.GetKeyDown(KeyCode.W)) navigateUp = true;
        if (Input.GetKeyDown(KeyCode.S)) navigateDown = true;

        // -----------------------------------------------------------------
        // LOGIKA SELEKSI KARAKTER & POP-UP
        // -----------------------------------------------------------------
        if (!isPopUpActive)
        {
            // Navigasi Utama Kiri / Kanan (Looping & Menyambung Sempurna)
            if (navigateLeft || navigateRight)
            {
                PlaySFX(selectingSound);
                NavigateMainSelection(navigateLeft);
            }

            if (isConfirmPressed)
            {
                GameObject current = EventSystem.current.currentSelectedGameObject;

                // Memastikan pengecekan berdasarkan tombol yang saat ini AKTIF di-highlight secara presisi
                if (current == kiroButton.gameObject)
                {
                    activeCharacter = "Kiro";
                    PlaySFX(pressedSound);
                    OpenPopUp(kiroPopUpObj);
                }
                else if (current == lyraButton.gameObject)
                {
                    activeCharacter = "Lyra";
                    PlaySFX(pressedSound);
                    OpenPopUp(lyraPopUpObj);
                }
            }
        }
        else
        {
            // Di dalam Pop Up (Navigasi Atas / Bawah untuk Konfirmasi)
            if (navigateUp || navigateDown)
            {
                isConfirmSelected = !isConfirmSelected;
                PlaySFX(selectingSound);
                RefreshSprite();
            }

            if (isConfirmPressed)
            {
                PlaySFX(pressedSound);
                if (isConfirmSelected)
                {
                    Debug.Log("Game Start: " + activeCharacter);
                    selectedCharacter = activeCharacter;
                    SceneManager.LoadScene("Loading1");
                }
                else
                {
                    CloseAll();
                }
            }
        }
    }

    // Mengurus paksa focus UI Button secara looping melingkar tanpa putus
    private void NavigateMainSelection(bool left)
    {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null)
        {
            kiroButton.Select();
            return;
        }

        if (left)
        {
            // Jika di Kiro (paling kiri) dipaksa geser kiri, dia akan melompat secara mulus ke Lyra (paling kanan)
            if (currentSelected == kiroButton.gameObject)
            {
                lyraButton.Select();
            }
            else if (currentSelected == lyraButton.gameObject)
            {
                kiroButton.Select();
            }
        }
        else // Right
        {
            // Jika di Lyra (paling kanan) dipaksa geser kanan, dia akan berputar kembali ke Kiro (paling kiri)
            if (currentSelected == lyraButton.gameObject)
            {
                kiroButton.Select();
            }
            else if (currentSelected == kiroButton.gameObject)
            {
                lyraButton.Select();
            }
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void OpenPopUp(GameObject popUp)
    {
        isPopUpActive = true;
        popUp.SetActive(true);
        if (blurPanel != null) blurPanel.SetActive(true);
        isConfirmSelected = true;
        RefreshSprite();

        // Bersihkan fokus tombol utama agar tidak sengaja terpencet di belakang layar
        EventSystem.current.SetSelectedGameObject(null);
    }

    void RefreshSprite()
    {
        if (activeCharacter == "Kiro")
        {
            kiroImage.sprite = isConfirmSelected ? kiroConfirmSprite : kiroCancelSprite;
        }
        else if (activeCharacter == "Lyra")
        {
            lyraImage.sprite = isConfirmSelected ? lyraConfirmSprite : lyraCancelSprite;
        }
    }

    public void CloseAll()
    {
        isPopUpActive = false;
        kiroPopUpObj.SetActive(false);
        lyraPopUpObj.SetActive(false);
        if (blurPanel != null) blurPanel.SetActive(false);

        if (activeCharacter == "Kiro") kiroButton.Select();
        else lyraButton.Select();
    }
}