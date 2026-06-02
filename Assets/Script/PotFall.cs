using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

[System.Serializable]
public class PotDialogue
{
    public string playerName;

    [TextArea]
    public string dialogueText;

    public float textSpeed = 0.03f;
    public float textDelay = 0.5f;

    public AudioClip dialogueSFX;
}

public class PotFall : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini di Inspector

    [Header("Sprite")]
    public Sprite standingSprite;
    public Sprite fallenSprite;

    [Header("Player")]
    public Transform player;
    public float triggerDistance = 2f;

    [Header("Pot SFX")]
    public AudioClip breakSound;

    [Header("Dialogue")]
    public GameObject dialogueBox;
    public TMP_Text dialogueTMP;
    public TMP_Text playerNameTMP;

    [Header("Pot Dialogues")]
    public List<PotDialogue> potDialogues = new List<PotDialogue>();

    [Header("Global Light")]
    public Light2D globalLight;
    public float targetIntensity = 0.02f;

    [Header("Character Light")]
    public Light2D playerLight;

    [Header("Gameplay Manager")]
    public LevelGameplayManager gameplayManager;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private AudioSource dialogueAudioSource;

    private bool hasFallen = false;
    private bool eventAlreadyTriggered = false;

    // 🔥 CONTROL SKIP
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isDone = false;
    private Coroutine typingCoroutine;
    private bool isDialoguePlaying = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();

        if (sr != null && standingSprite != null)
            sr.sprite = standingSprite;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (playerLight != null)
            playerLight.enabled = false;

        // Otomatis mencari script ESP32Input jika lupa di-drag di Inspector
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }
    }

    void Update()
    {
        if (!hasFallen && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= triggerDistance)
            {
                FallPot();
            }
        }

        // 🔥 HANDLE SKIP HYBRID
        if (isDialoguePlaying)
        {
            // Ambil input konfirmasi secara Hybrid (Keyboard Enter ATAU ESP32 Select)
            bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return);

            if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
            {
                isConfirmPressed = true;
            }

            if (isConfirmPressed)
            {
                // Kalau sedang mengetik -> langsung tampilkan semua teks utuh
                if (isTyping && !isDone)
                {
                    if (typingCoroutine != null)
                        StopCoroutine(typingCoroutine);

                    ShowFullText(potDialogues[currentIndex]);

                    if (dialogueAudioSource != null)
                        dialogueAudioSource.Stop();

                    isTyping = false;
                    isDone = true;
                }
                // Kalau sudah selesai ngetik -> lanjut ke baris dialog berikutnya
                else if (isDone)
                {
                    NextDialogue();
                }
            }
        }
    }

    void FallPot()
    {
        hasFallen = true;

        if (sr != null && fallenSprite != null)
            sr.sprite = fallenSprite;

        transform.rotation = Quaternion.Euler(0f, 0f, -90f);

        if (audioSource != null && breakSound != null)
            audioSource.PlayOneShot(breakSound);

        if (eventAlreadyTriggered) return;

        eventAlreadyTriggered = true;

        StartCoroutine(StorySequence());
    }

    IEnumerator StorySequence()
    {
        if (globalLight != null)
            globalLight.intensity = targetIntensity;

        if (playerLight != null)
            playerLight.enabled = true;

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        // mulai sistem dialog baru
        isDialoguePlaying = true;
        currentIndex = 0;

        typingCoroutine = StartCoroutine(TypeWriter(potDialogues[currentIndex]));

        yield return new WaitUntil(() => !isDialoguePlaying);

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        if (gameplayManager != null)
            gameplayManager.StartTimerAfterPotDialogue();
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex < potDialogues.Count)
        {
            typingCoroutine = StartCoroutine(TypeWriter(potDialogues[currentIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialoguePlaying = false;
    }

    IEnumerator TypeWriter(PotDialogue data)
    {
        isTyping = true;
        isDone = false;

        dialogueTMP.text = "";

        if (playerNameTMP != null)
            playerNameTMP.text = data.playerName;

        string cleanText = data.dialogueText.Replace("\n", "").Replace("\r", "").Trim();

        // 🔊 PLAY SFX LOOP
        if (data.dialogueSFX != null)
        {
            dialogueAudioSource.Stop();
            dialogueAudioSource.clip = data.dialogueSFX;
            dialogueAudioSource.loop = true;
            dialogueAudioSource.Play();
        }

        foreach (char c in cleanText)
        {
            dialogueTMP.text += c;

            dialogueTMP.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();

            yield return new WaitForSeconds(data.textSpeed);
        }

        // 🔊 STOP SFX
        dialogueAudioSource.Stop();

        isTyping = false;
        isDone = true;
    }

    void ShowFullText(PotDialogue data)
    {
        dialogueTMP.text = data.dialogueText;

        if (playerNameTMP != null)
            playerNameTMP.text = data.playerName;
    }
}