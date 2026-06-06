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
    private bool isJoystickVerticalInUse = false;

    void Start()
    {
        if (kiroButton != null) kiroButton.Select();

        // FIX BARIS 52: Menggunakan FindAnyObjectByType untuk menghilangkan warning OBSOLETE
        if (esp32Input == null)
        {
            esp32Input = FindAnyObjectByType<ESP32Input>();
        }
    }

    void Update()
    {
        // -----------------------------------------------------------------
        // SINKRONISASI TOTAL: Ambil langsung dari indeks CharacterSelector
        // -----------------------------------------------------------------
        if (!isPopUpActive)
        {
            if (CharacterSelector.publicCurrentIndex == 0)
            {
                activeCharacter = "Kiro";
            }
            else
            {
                activeCharacter = "Lyra";
            }
        }

        // -----------------------------------------------------------------
        // AMBIL INPUT HYBRID (KEYBOARD & ESP32)
        // -----------------------------------------------------------------
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (esp32Input != null && esp32Input.isConnected)
        {
            if (Mathf.Abs(esp32Input.vertical) > 0.5f) verticalInput = esp32Input.vertical;
        }

        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
        if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
        {
            isConfirmPressed = true;
        }

        // -----------------------------------------------------------------
        // PROSES ONE-SHOT NAVIGASI POP-UP (VERTIKAL)
        // -----------------------------------------------------------------
        bool navigateUp = false;
        bool navigateDown = false;

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

        // Tambahan input tombol keyboard cadangan (W/S)
        if (Input.GetKeyDown(KeyCode.W)) navigateUp = true;
        if (Input.GetKeyDown(KeyCode.S)) navigateDown = true;

        // -----------------------------------------------------------------
        // LOGIKA SELEKSI POP-UP
        // -----------------------------------------------------------------
        if (!isPopUpActive)
        {
            if (isConfirmPressed)
            {
                // Eksekusi pop-up berdasarkan data statis yang 100% valid dan anti-tertukar
                if (activeCharacter == "Kiro")
                {
                    PlaySFX(pressedSound);
                    OpenPopUp(kiroPopUpObj);
                }
                else if (activeCharacter == "Lyra")
                {
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

        // Fokus dikembalikan pasif mengikuti karakter aktif saat ini
        if (activeCharacter == "Kiro") kiroButton.Select();
        else lyraButton.Select();
    }
}