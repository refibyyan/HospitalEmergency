using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SceneIntroManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueData
    {
        public string playerName;

        [TextArea]
        public string dialogueText;

        public float textSpeed = 0.03f;
        public float textDelay = 0.5f;

        public AudioClip dialogueSFX;
    }

    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini di Inspector

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Dialogue")]
    public GameObject dialogueBox;
    public TMP_Text dialogueTMP;
    public TMP_Text playerNameTMP;

    [Header("Intro Dialogue")]
    public DialogueData[] introDialogues;

    [Header("Lights")]
    public Light2D globalLight;
    public Light2D characterLight;

    [Header("Player")]
    public PlayerMovement playerMovementScript;

    [Header("Popup")]
    public GameObject popupSprite;

    [Header("Ambient")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientLoopSFX;
    public float ambientVolume = 1f;
    public float ambientDelay = 5f;

    private AudioSource dialogueAudioSource;
    private Coroutine ambientCoroutine;

    // 🔥 TAMBAHAN CONTROL
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isDone = false;
    private Coroutine typingCoroutine;
    private bool isDialoguePlaying = false;

    private void Awake()
    {
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // Otomatis mencari script ESP32Input jika lupa di-drag di Inspector
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }

        StartCoroutine(IntroSequence());
    }

    private void Update()
    {
        if (!isDialoguePlaying) return;

        // 🔥 DETEKSI INPUT HYBRID (Keyboard Enter ATAU ESP32 Select Button)
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return);

        if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
        {
            isConfirmPressed = true;
        }

        if (isConfirmPressed)
        {
            // Jika teks sedang diketik -> potong langsung tampilkan full
            if (isTyping && !isDone)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                ShowFullText(introDialogues[currentIndex]);

                // 🔊 STOP SFX saat skip
                if (dialogueAudioSource != null)
                    dialogueAudioSource.Stop();

                isTyping = false;
                isDone = true;
            }
            // Jika teks sudah selesai tampil -> lanjut ke dialog berikutnya
            else if (isDone)
            {
                NextDialogue();
            }
        }
    }

    IEnumerator IntroSequence()
    {
        if (popupSprite != null) popupSprite.SetActive(false);
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (playerMovementScript != null) playerMovementScript.canMove = false;

        if (globalLight != null) globalLight.intensity = 0.2f;
        if (characterLight != null) characterLight.enabled = false;

        ambientCoroutine = StartCoroutine(AmbientLoopRoutine());

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOut());

        // 🔥 mulai sistem dialog baru
        StartDialogue();

        yield return new WaitUntil(() => !isDialoguePlaying);

        if (characterLight != null) characterLight.enabled = true;
        if (globalLight != null) globalLight.intensity = 1f;
        if (playerMovementScript != null) playerMovementScript.canMove = true;
        if (popupSprite != null) popupSprite.SetActive(true);

        if (fadeImage != null) fadeImage.gameObject.SetActive(false);
    }

    void StartDialogue()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        isDialoguePlaying = true;
        currentIndex = 0;

        typingCoroutine = StartCoroutine(TypeWriter(introDialogues[currentIndex]));
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex < introDialogues.Length)
        {
            typingCoroutine = StartCoroutine(TypeWriter(introDialogues[currentIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        isDialoguePlaying = false;
    }

    IEnumerator AmbientLoopRoutine()
    {
        while (true)
        {
            if (ambientAudioSource != null && ambientLoopSFX != null)
            {
                ambientAudioSource.clip = ambientLoopSFX;
                ambientAudioSource.volume = ambientVolume;
                ambientAudioSource.loop = true;
                ambientAudioSource.Play();

                yield return new WaitForSeconds(5f);
                ambientAudioSource.Stop();
            }

            yield return new WaitForSeconds(ambientDelay);
        }
    }

    public void StopAmbientLoop()
    {
        if (ambientCoroutine != null)
            StopCoroutine(ambientCoroutine);

        if (ambientAudioSource != null)
            ambientAudioSource.Stop();
    }

    IEnumerator TypeWriter(DialogueData data)
    {
        isTyping = true;
        isDone = false;

        dialogueTMP.text = "";

        if (playerNameTMP != null)
            playerNameTMP.text = data.playerName;

        // 🔊 PLAY SFX LOOP
        if (data.dialogueSFX != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
            dialogueAudioSource.clip = data.dialogueSFX;
            dialogueAudioSource.loop = true;
            dialogueAudioSource.Play();
        }

        foreach (char c in data.dialogueText)
        {
            dialogueTMP.text += c;
            yield return new WaitForSeconds(data.textSpeed);
        }

        // 🔊 STOP SFX
        if (dialogueAudioSource != null)
            dialogueAudioSource.Stop();

        isTyping = false;
        isDone = true;
    }

    void ShowFullText(DialogueData data)
    {
        dialogueTMP.text = data.dialogueText;

        if (playerNameTMP != null)
            playerNameTMP.text = data.playerName;
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;
    }
}