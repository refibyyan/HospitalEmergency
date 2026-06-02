using UnityEngine;
using System.Collections;

public partial class TutorialManager : MonoBehaviour
{
    [Header("ESP32 Input Reference")]
    public ESP32Input esp32Input; // Referensi untuk mendeteksi tombol ESP32

    [Header("UI Elements")]
    public GameObject blurPanel;
    public GameObject choicePopUp;
    public GameObject movementPopUp;

    [Header("Text Elements")]
    public GameObject popupTutorialText;
    public GameObject learnHowToText;

    private bool isTutorialActive = false;

    void Start()
    {
        // Otomatis cari ESP32Input jika lupa di-drag
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }
    }

    public void ShowTutorial()
    {
        // Cegah duplikasi coroutine jika tutorial sudah terbuka
        if (!isTutorialActive)
        {
            StartCoroutine(TutorialSequence());
        }
    }

    IEnumerator TutorialSequence()
    {
        isTutorialActive = true;

        // 1. Aktifkan semua elemen dasar
        blurPanel.SetActive(true);
        choicePopUp.SetActive(true);
        movementPopUp.SetActive(true);

        // 2. Aktifkan teks
        popupTutorialText.SetActive(true);
        learnHowToText.SetActive(true);

        // Tunggu 1 frame dulu agar input "Enter" dari menu utama tidak langsung memicu penutupan tutorial
        yield return null;

        // 3. MENUNGGU INPUT (Bukan menunggu 10 detik lagi)
        // Loop ini akan terus berjalan SEBELUM user menekan Enter di keyboard ATAU tombol di ESP32
        bool hasConfirmed = false;
        while (!hasConfirmed)
        {
            // Cek Keyboard Enter
            if (Input.GetKeyDown(KeyCode.Return))
            {
                hasConfirmed = true;
            }

            // Cek ESP32 Button
            if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
            {
                hasConfirmed = true;
            }

            yield return null; // Tunggu frame berikutnya sebelum cek input lagi
        }

        // 4. Hilangkan semua elemen setelah tombol ditekan
        blurPanel.SetActive(false);
        choicePopUp.SetActive(false);
        movementPopUp.SetActive(false);
        popupTutorialText.SetActive(false);
        learnHowToText.SetActive(false);

        isTutorialActive = false;

        // Beritahu MainMenuManager kalau tutorial sudah ditutup
        MainMenuManager mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.CloseTutorialPopUp();
        }
    }
}