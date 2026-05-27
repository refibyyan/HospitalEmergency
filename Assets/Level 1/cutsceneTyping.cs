using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CutsceneTyping : MonoBehaviour
{
    [Header("UI")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Dialog")]
    public string[] dialogLines;
    public float typingSpeed = 0.05f;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeSpeed = 1f;

    private int indexDialog = 0;
    private bool typing = false;
    private bool done = false;

    private TriggerDecisionLv1 trigger;

    void Start()
    {
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
        yield return StartCoroutine(TypeDialog());
    }

    void Update()
    {
        if (!cutscenePanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (typing && !done)
            {
                StopAllCoroutines();
                dialogueText.text = dialogLines[indexDialog];
                typing = false;
                done = true;
            }
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

        foreach (char c in dialogLines[indexDialog])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        typing = false;
        done = true;
    }

    void NextDialog()
    {
        indexDialog++;

        if (indexDialog < dialogLines.Length)
        {
            StartCoroutine(TypeDialog());
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

        // 🔥 LANJUT KE DECISION
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