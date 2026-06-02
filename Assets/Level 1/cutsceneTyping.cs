using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CutsceneTyping : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini di Inspector

    [Header("UI")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Dialog")]
    public string[] dialogLines;
    public float typingSpeed = 0.1f;

    [Header("Typing SFX")]
    public AudioSource typingSource;
    public AudioClip typingSFX;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeSpeed = 1f;

    private int indexDialog = 0;
    private bool typing = false;
    private bool done = false;
    private Coroutine typingCoroutine;
    private TriggerDecisionLv1 trigger;

    void Start()
    {
        // Otomatis mencari script ESP32Input di hierarki jika belum di-drag manual
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);

        SetFade(0f);
    }

    // ================= PLAY CUTSCENE =================
    public void PlayCutscene(TriggerDecisionLv1 t)
    {
        trigger = t;
        cutscenePanel.SetActive(true);
        indexDialog = 0;
        StopAllCoroutines();
        StartCoroutine(CutsceneFlow());
    }

    IEnumerator CutsceneFlow()
    {
        yield return StartCoroutine(FadeIn());
        typingCoroutine = StartCoroutine(TypeDialog());
    }

    void Update()
    {
        if (cutscenePanel == null || !cutscenePanel.activeSelf) return;

        // 🔥 DETEKSI INPUT HYBRID (Keyboard Enter OR Tombol Select Hardware ESP32)
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return) ||
                                (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed);

        if (isConfirmPressed)
        {
            // Jika text sedang mengetik dan belum beres -> Skip tampilkan langsung semua text
            if (typing && !done)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                dialogueText.text = dialogLines[indexDialog];

                if (typingSource != null)
                {
                    typingSource.Stop();
                }

                typing = false;
                done = true;
            }
            // Jika text sudah selesai diketik seluruhnya -> Lanjut dialog berikutnya
            else if (done)
            {
                NextDialog();
            }
        }
    }

    IEnumerator TypeDialog()
    {
        typing = true;
        done = false;
        dialogueText.text = "";

        if (typingSource != null && typingSFX != null)
        {
            typingSource.clip = typingSFX;
            typingSource.loop = true;
            typingSource.Play();
        }

        foreach (char c in dialogLines[indexDialog])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (typingSource != null)
        {
            typingSource.loop = false;
            typingSource.Stop();
        }

        typing = false;
        done = true;
    }

    void NextDialog()
    {
        indexDialog++;
        if (indexDialog < dialogLines.Length)
        {
            if (typingSource != null)
            {
                typingSource.Stop();
            }
            typingCoroutine = StartCoroutine(TypeDialog());
        }
        else
        {
            EndCutscene();
        }
    }

    void EndCutscene()
    {
        StartCoroutine(EndFlow());
    }

    IEnumerator EndFlow()
    {
        yield return StartCoroutine(FadeOut());
        cutscenePanel.SetActive(false);
        if (trigger != null)
        {
            trigger.ShowDecision();
        }
    }

    // ================= FADE =================
    IEnumerator FadeIn()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * fadeSpeed;
            SetFade(t);
            yield return null;
        }
        SetFade(0f);
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            SetFade(t);
            yield return null;
        }
        SetFade(1f);
    }

    void SetFade(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}