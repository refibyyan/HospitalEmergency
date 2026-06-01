using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

[System.Serializable]
public class PotDialogue
{
    [Header("Player Name")]
    public string playerName;

    [TextArea]
    public string dialogueText;

    [Header("Typing")]
    public float textSpeed = 0.03f;

    [Header("Delay After Dialogue")]
    public float textDelay = 0.5f;

    [Header("Dialogue Audio")]
    public AudioClip dialogueSFX;
}

public class PotFall : MonoBehaviour
{
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
    public List<PotDialogue> potDialogues =
        new List<PotDialogue>();

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

    // Berlaku untuk semua pot
    private bool eventAlreadyTriggered = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        audioSource =
            GetComponent<AudioSource>();

        dialogueAudioSource =
            gameObject.AddComponent<AudioSource>();

        if (sr != null &&
            standingSprite != null)
        {
            sr.sprite = standingSprite;
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Lampu player awal OFF
        if (playerLight != null)
        {
            playerLight.enabled = false;
        }
    }

    void Update()
    {
        if (hasFallen)
            return;

        if (player == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distance <= triggerDistance)
        {
            FallPot();
        }
    }

    void FallPot()
    {
        hasFallen = true;

        // Ganti sprite
        if (sr != null &&
            fallenSprite != null)
        {
            sr.sprite = fallenSprite;
        }

        // Rotasi jatuh
        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                -90f
            );

        // SFX pot
        if (audioSource != null &&
            breakSound != null)
        {
            audioSource.PlayOneShot(
                breakSound
            );
        }

        // Event utama hanya sekali
        if (eventAlreadyTriggered)
            return;

        eventAlreadyTriggered = true;

        StartCoroutine(
            StorySequence()
        );
    }

    IEnumerator StorySequence()
    {
        // Dunia gelap
        if (globalLight != null)
        {
            globalLight.intensity =
                targetIntensity;
        }

        // Lampu Lyra nyala
        if (playerLight != null)
        {
            playerLight.enabled = true;
        }

        // Tampilkan dialog
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        foreach (PotDialogue dialogue in potDialogues)
        {
            yield return StartCoroutine(
                TypeWriter(dialogue)
            );

            yield return new WaitForSeconds(
                dialogue.textDelay
            );
        }

        // Tutup dialog
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Mulai timer
        if (gameplayManager != null)
        {
            gameplayManager
                .StartTimerAfterPotDialogue();
        }
    }

    IEnumerator TypeWriter(
        PotDialogue data)
    {
        if (dialogueTMP == null)
            yield break;

        dialogueTMP.text = "";

        if (playerNameTMP != null)
        {
            playerNameTMP.text =
                data.playerName;
        }

        string cleanText =
            data.dialogueText
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim();

        // Audio dialog
        if (data.dialogueSFX != null)
        {
            dialogueAudioSource.Stop();

            dialogueAudioSource.clip =
                data.dialogueSFX;

            dialogueAudioSource.loop = true;

            dialogueAudioSource.Play();
        }

        foreach (char c in cleanText)
        {
            dialogueTMP.text += c;

            dialogueTMP.ForceMeshUpdate();

            Canvas.ForceUpdateCanvases();

            yield return new WaitForSeconds(
                data.textSpeed
            );
        }

        dialogueAudioSource.Stop();
    }
}