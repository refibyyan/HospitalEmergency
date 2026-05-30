using System.Collections;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
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

    void Start()
    {
        StartCoroutine(WaitBeforeStart());
    }

    IEnumerator WaitBeforeStart()
    {
        dialogBox.SetActive(false);

        yield return new WaitForSeconds(startDelay);

        StartDialog();
    }

    public void StartDialog()
    {
        dialogBox.SetActive(true);
        index = 0;

        typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        textMeshPro.text = "";

        foreach (char c in dialogLines[index].ToCharArray())
        {
            textMeshPro.text += c;

            if (typingSource != null &&
                typingSFX != null &&
                c != ' ' &&
                !typingSource.isPlaying)
            {
                typingSource.PlayOneShot(typingSFX);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (textMeshPro.text == dialogLines[index])
            {
                NextLine();
            }
            else
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }

                textMeshPro.text = dialogLines[index];

                if (typingSource != null)
                {
                    typingSource.Stop();
                }
            }
        }
    }

    void NextLine()
    {
        if (index < dialogLines.Length - 1)
        {
            index++;

            if (typingSource != null)
            {
                typingSource.Stop();
            }

            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            if (typingSource != null)
            {
                typingSource.Stop();
            }

            dialogBox.SetActive(false);
            textMeshPro.text = "";

            Debug.Log("Dialog Selesai! Pemain sekarang bisa jalan.");
        }
    }
}