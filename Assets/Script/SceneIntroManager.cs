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

    [Header("Dialogue 1-3 (Masih Hitam)")]
    public DialogueData[] introDarkDialogues;

    [Header("Dialogue 4-6 (Setelah Fade Hilang)")]
    public DialogueData[] introLightDialogues;

    [Header("Lights")]
    public Light2D globalLight;

    public Light2D characterLight;

    [Header("Player Movement Script")]
    public PlayerMovement playerMovementScript;
    [Header("Popup")]
    public GameObject popupSprite;

    private AudioSource dialogueAudioSource;

    private void Awake()
    {
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
    }

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

        // awal terang
        if (globalLight != null)
        {
            globalLight.intensity = 1f;
        }

        // character light mati
        if (characterLight != null)
        {
            characterLight.enabled = false;
        }

        // layar hitam total
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1;
            fadeImage.color = color;
        }

        yield return new WaitForSeconds(0.5f);

        // dialogue 1-3
        yield return StartCoroutine(
            PlayDialogueSequence(introDarkDialogues)
        );

        // fade hilang
        yield return StartCoroutine(FadeOut());

        // dialogue 4-6
        yield return StartCoroutine(
            PlayDialogueSequence(introLightDialogues)
        );

        // player bisa gerak
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = true;
        }

        // popup muncul
        if (popupSprite != null)
        {
            popupSprite.SetActive(true);
        }
    }

    public IEnumerator PlayDialogueSequence(DialogueData[] dialogues)
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

    public IEnumerator TypeWriter(DialogueData data)
    {
        if (dialogueTMP == null)
            yield break;

        // reset text TOTAL
        dialogueTMP.text = string.Empty;

        // refresh TMP
        dialogueTMP.ForceMeshUpdate();

        // bersihin enter/spasi aneh
        string cleanText =
            data.dialogueText
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim();

        // PLAY AUDIO
        if (data.dialogueSFX != null)
        {
            dialogueAudioSource.Stop();

            dialogueAudioSource.clip = data.dialogueSFX;

            dialogueAudioSource.loop = true;

            dialogueAudioSource.Play();
        }

        // TYPEWRITER
        foreach (char c in cleanText)
        {
            dialogueTMP.text += c;

            // refresh canvas
            dialogueTMP.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();

            yield return new WaitForSeconds(
                data.textSpeed
            );
        }

        // STOP AUDIO
        dialogueAudioSource.Stop();
    }

    IEnumerator FadeOut()
    {
        if (fadeImage == null)
            yield break;

        float time = 0;

        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(
                1,
                0,
                time / fadeDuration
            );

            fadeImage.color = color;

            yield return null;
        }

        color.a = 0;

        fadeImage.color = color;
    }
}