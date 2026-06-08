using System.Collections;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini di Inspector

    [Header("UI Elemen")]
    public GameObject dialogBox;
    public TextMeshProUGUI textMeshPro;
    public float typingSpeed = 0.05f;

    [Header("Typing SFX")]
    public AudioSource typingSource;
    public AudioClip typingSFX;

    [Header("Waktu Jeda Awal (Detik)")]
    public float startDelay = 2.0f;

    [Header("Isi Dialog")]
    [TextArea(3, 5)]
    public string[] dialogLines;

    private Coroutine typingCoroutine;
    private int index;
    private bool isDialogActive = false;

    void Start()
    {
        // Otomatis mencari script ESP32Input di hierarki jika belum di-drag manual
        if (esp32Input == null)
        {
            // Diubah ke FindAnyObjectByType untuk menghindari warning obsolete
            esp32Input = FindAnyObjectByType<ESP32Input>();
        }

        StartCoroutine(WaitBeforeStart());
    }

    IEnumerator WaitBeforeStart()
    {
        if (dialogBox != null) dialogBox.SetActive(false);

        yield return new WaitForSeconds(startDelay);

        StartDialog();
    }

    public void StartDialog()
    {
        if (dialogBox != null) dialogBox.SetActive(true);
        index = 0;
        isDialogActive = true;

        typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        textMeshPro.text = "";

        // 🔥 Mulai mainkan SFX secara looping selama text sedang mengetik
        if (typingSource != null && typingSFX != null)
        {
            typingSource.clip = typingSFX;
            typingSource.loop = true; // Set audio agar mengulang terus selama text jalan
            typingSource.Play();
        }

        foreach (char c in dialogLines[index].ToCharArray())
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 🔥 Matikan SFX tepat setelah text selesai beranimasi secara natural
        StopTypingSFX();
    }

    void Update()
    {
        if (!isDialogActive) return;

        // DETEKSI INPUT HYBRID (Keyboard Enter OR Mouse Click OR ESP32 Select Button)
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) ||
                                Input.GetMouseButtonDown(0) ||
                                (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed);

        if (isConfirmPressed)
        {
            // Jika teks sudah selesai diketik seluruhnya -> Lanjut baris berikutnya
            if (textMeshPro.text == dialogLines[index])
            {
                NextLine();
            }
            // Jika teks masih dalam proses mengetik -> Skip langsung tampilkan penuh
            else
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                textMeshPro.text = dialogLines[index];
                
                // 🔥 Matikan SFX langsung karena dialog di-skip oleh player
                StopTypingSFX();
            }
        }
    }

    void NextLine()
    {
        if (index < dialogLines.Length - 1)
        {
            index++;
            StopTypingSFX();
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            StopTypingSFX();

            if (dialogBox != null) dialogBox.SetActive(false);
            textMeshPro.text = "";
            isDialogActive = false;

            Debug.Log("Dialog Selesai! Pemain sekarang bisa jalan.");
        }
    }

    // 🔥 Fungsi pembantu untuk mematikan SFX typing dengan aman
    void StopTypingSFX()
    {
        if (typingSource != null && typingSource.isPlaying)
        {
            typingSource.Stop();
        }
    }
}