using System.Collections;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [Header("UI Elemen")]
    public GameObject dialogBox;
    public TextMeshProUGUI textMeshPro;
    public float typingSpeed = 0.05f;

    [Header("Waktu Jeda Awal (Detik)")]
    public float startDelay = 2.0f; // <--- Kamu bisa ganti angka jedanya di Inspector nanti!

    [Header("Isi Dialog")]
    [TextArea(3, 5)]
    public string[] dialogLines;
    private int index;

    void Start()
    {
        // Gak langsung jalan, tapi kita suruh Unity nunggu dulu sesuai startDelay
        StartCoroutine(WaitBeforeStart());
    }

    IEnumerator WaitBeforeStart()
    {
        // Sembunyikan dulu kotak dialog selama jeda
        dialogBox.SetActive(false);

        // Tunggu selama 2 detik (atau sesuai angka startDelay)
        yield return new WaitForSeconds(startDelay);

        // Setelah nunggu, baru jalankan dialognya!
        StartDialog();
    }

    public void StartDialog()
    {
        dialogBox.SetActive(true);
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        textMeshPro.text = "";
        foreach (char c in dialogLines[index].ToCharArray())
        {
            textMeshPro.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void Update()
    {
        // Sekarang tombolnya mendeteksi Enter (Return) atau Klik Kiri Mouse
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (textMeshPro.text == dialogLines[index])
            {
                NextLine();
            }
            else
            {
                // Kalau lagi ngetik terus ditekan Enter, langsung munculin semua hurufnya
                StopAllCoroutines();
                textMeshPro.text = dialogLines[index];
            }
        }
    }

    void NextLine()
    {
        if (index < dialogLines.Length - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            dialogBox.SetActive(false);
            textMeshPro.text = "";
            Debug.Log("Dialog Selesai! Pemain sekarang bisa jalan.");
        }
    }
}