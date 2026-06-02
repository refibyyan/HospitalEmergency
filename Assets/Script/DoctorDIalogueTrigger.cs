using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoctorDialogueTrigger : MonoBehaviour
{
    [Header("--- ESP32 INPUT REFERENCE ---")]
    public ESP32Input esp32Input;

    public SceneIntroManager sceneIntroManager;
    public SceneIntroManager.DialogueData[] doctorDialogues;

    [Header("Quick Decision")]
    public QuickDecisionManager quickDecisionManager;

    [Header("Player")]
    public PlayerMovement playerMovementScript;

    [Header("Lights")]
    public Light2D globalLight;
    public Light2D characterLight;

    [Header("Popup")]
    public GameObject popupSprite;

    [Header("Dark SFX")]
    public AudioSource darkSFXAudioSource;
    public AudioClip darkSFX;
    public float darkSFXVolume = 1f;

    private bool triggered = false;

    // 🔥 CONTROL SKIP
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isDone = false;
    private Coroutine typingCoroutine;

    // 🔊 TAMBAHAN AUDIO SOURCE KHUSUS DIALOGUE
    private AudioSource dialogueAudioSource;

    private void Start()
    {
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }

        // 🔊 init audio source untuk dialogue
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(DoctorSequence());
        }
    }

    private void Update()
    {
        if (!triggered) return;

        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return);

        if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
        {
            isConfirmPressed = true;
        }

        if (isConfirmPressed)
        {
            if (isTyping && !isDone)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                ShowFullText(doctorDialogues[currentIndex]);

                // 🔊 STOP SFX saat skip
                if (dialogueAudioSource != null)
                    dialogueAudioSource.Stop();

                isTyping = false;
                isDone = true;

                TriggerSpecialEventIfNeeded(currentIndex);
            }
            else if (isDone)
            {
                NextDialogue();
            }
        }
    }

    IEnumerator DoctorSequence()
    {
        if (playerMovementScript != null)
            playerMovementScript.canMove = false;

        if (popupSprite != null)
            popupSprite.SetActive(false);

        sceneIntroManager.dialogueBox.SetActive(true);

        currentIndex = 0;
        StartDialogue();
        yield return null;
    }

    void StartDialogue()
    {
        if (currentIndex < doctorDialogues.Length)
        {
            typingCoroutine = StartCoroutine(TypeDialog(doctorDialogues[currentIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeDialog(SceneIntroManager.DialogueData data)
    {
        isTyping = true;
        isDone = false;

        sceneIntroManager.dialogueTMP.text = "";

        if (sceneIntroManager.playerNameTMP != null)
            sceneIntroManager.playerNameTMP.text = data.playerName;

        // 🔊 PLAY SFX SAAT MULAI NGETIK
        if (data.dialogueSFX != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
            dialogueAudioSource.clip = data.dialogueSFX;
            dialogueAudioSource.loop = true;
            dialogueAudioSource.Play();
        }

        foreach (char c in data.dialogueText)
        {
            sceneIntroManager.dialogueTMP.text += c;
            yield return new WaitForSeconds(data.textSpeed);
        }

        // 🔊 STOP SFX SAAT SELESAI
        if (dialogueAudioSource != null)
            dialogueAudioSource.Stop();

        isTyping = false;
        isDone = true;

        TriggerSpecialEventIfNeeded(currentIndex);
    }

    void TriggerSpecialEventIfNeeded(int index)
    {
        if (index == 1)
        {
            sceneIntroManager.StopAmbientLoop();

            if (globalLight != null)
                globalLight.intensity = 0.02f;

            if (darkSFXAudioSource != null && darkSFX != null && !darkSFXAudioSource.isPlaying)
            {
                darkSFXAudioSource.volume = darkSFXVolume;
                darkSFXAudioSource.PlayOneShot(darkSFX);
            }
        }
    }

    void ShowFullText(SceneIntroManager.DialogueData data)
    {
        sceneIntroManager.dialogueTMP.text = data.dialogueText;

        if (sceneIntroManager.playerNameTMP != null)
            sceneIntroManager.playerNameTMP.text = data.playerName;
    }

    void NextDialogue()
    {
        currentIndex++;

        if (currentIndex < doctorDialogues.Length)
        {
            typingCoroutine = StartCoroutine(TypeDialog(doctorDialogues[currentIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        sceneIntroManager.dialogueBox.SetActive(false);

        if (characterLight != null)
            characterLight.enabled = true;

        if (playerMovementScript != null)
            playerMovementScript.canMove = true;

        if (quickDecisionManager != null)
            quickDecisionManager.ActivateDecisionTrigger();
    }
}