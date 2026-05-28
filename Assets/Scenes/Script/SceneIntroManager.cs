using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;

public class SceneIntroManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueData
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

    [Header("Fade")]
    public Image fadeImage;

    public float fadeDuration = 1f;

    [Header("Dialogue")]
    public GameObject dialogueBox;

    public TMP_Text dialogueTMP;

    [Header("Player Name Text")]
    public TMP_Text playerNameTMP;

    [Header("Single Intro Dialogue")]
    public DialogueData[] introDialogues;

    [Header("Lights")]
    public Light2D globalLight;

    public Light2D characterLight;

    [Header("Player Movement Script")]
    public PlayerMovement playerMovementScript;

    [Header("Popup")]
    public GameObject popupSprite;

    private AudioSource dialogueAudioSource;

    // =========================================
    // AWAKE
    // =========================================

    private void Awake()
    {
        dialogueAudioSource =
            gameObject.AddComponent<AudioSource>();
    }

    // =========================================
    // START
    // =========================================

    IEnumerator Start()
    {
        // popup hide
        if (popupSprite != null)
        {
            popupSprite.SetActive(false);
        }

        // hide dialogue
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // freeze movement
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = false;
        }

        // global light
        if (globalLight != null)
        {
            globalLight.intensity = 1f;
        }

        // character light off
        if (characterLight != null)
        {
            characterLight.enabled = false;
        }

        // =====================================
        // FORCE BLACK SCREEN
        // =====================================

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);

            // biar fade image paling depan
            fadeImage.transform.SetAsLastSibling();

            Color color = fadeImage.color;

            color.r = 0f;
            color.g = 0f;
            color.b = 0f;
            color.a = 1f;

            fadeImage.color = color;
        }

        // tunggu 1 frame
        yield return null;

        yield return new WaitForSeconds(0.5f);

        // =====================================
        // FADE OUT
        // =====================================

        yield return StartCoroutine(FadeOut());

        // =====================================
        // PLAY DIALOGUE
        // =====================================

        yield return StartCoroutine(
            PlayDialogueSequence(introDialogues)
        );

        // =====================================
        // PLAYER MOVE ON
        // =====================================

        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = true;
        }

        // =====================================
        // POPUP SHOW
        // =====================================

        if (popupSprite != null)
        {
            popupSprite.SetActive(true);
        }
    }

    // =========================================
    // PLAY DIALOGUE SEQUENCE
    // =========================================

    public IEnumerator PlayDialogueSequence(
        DialogueData[] dialogues)
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        foreach (DialogueData dialogue in dialogues)
        {
            yield return StartCoroutine(
                TypeWriter(dialogue)
            );

            yield return new WaitForSeconds(
                dialogue.textDelay
            );
        }

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }

    // =========================================
    // TYPEWRITER
    // =========================================

    public IEnumerator TypeWriter(DialogueData data)
    {
        if (dialogueTMP == null)
            yield break;

        // reset text
        dialogueTMP.text = string.Empty;

        // player name
        if (playerNameTMP != null)
        {
            playerNameTMP.text = data.playerName;
        }

        dialogueTMP.ForceMeshUpdate();

        // clean text
        string cleanText =
            data.dialogueText
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim();

        // =====================================
        // PLAY AUDIO
        // =====================================

        if (data.dialogueSFX != null)
        {
            dialogueAudioSource.Stop();

            dialogueAudioSource.clip =
                data.dialogueSFX;

            dialogueAudioSource.loop = true;

            dialogueAudioSource.Play();
        }

        // =====================================
        // TYPEWRITER EFFECT
        // =====================================

        foreach (char c in cleanText)
        {
            dialogueTMP.text += c;

            dialogueTMP.ForceMeshUpdate();

            Canvas.ForceUpdateCanvases();

            yield return new WaitForSeconds(
                data.textSpeed
            );
        }

        // =====================================
        // STOP AUDIO
        // =====================================

        dialogueAudioSource.Stop();
    }

    // =========================================
    // FADE OUT
    // =========================================

    IEnumerator FadeOut()
    {
        if (fadeImage == null)
            yield break;

        float time = 0f;

        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    time / fadeDuration
                );

            color.a = alpha;

            fadeImage.color = color;

            yield return null;
        }

        color.a = 0f;

        fadeImage.color = color;
    }
}