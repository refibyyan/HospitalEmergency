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

    [Header("AMBIENT LOOP SFX")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientLoopSFX;
    [Range(0f, 1f)] public float ambientVolume = 1f;
    public float ambientDelay = 5f;

    private AudioSource dialogueAudioSource;
    private Coroutine ambientCoroutine;
    private bool isIntroRunning = false;

    // =========================================
    // AWAKE
    // =========================================
    private void Awake()
    {
        // Pastikan AudioSource selalu dibuat fresh
        dialogueAudioSource = gameObject.AddComponent<AudioSource>();
    }

    // =========================================
    // ON ENABLE - Subscribe scene loaded event
    // =========================================
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =========================================
    // ON SCENE LOADED - Dipanggil setiap kali scene di-load/restart
    // =========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-validasi semua referensi setelah scene reload
        RevalidateReferences();
    }

    // =========================================
    // RE-VALIDASI REFERENSI
    // Cari ulang object jika referensi null setelah reload
    // =========================================
    private void RevalidateReferences()
    {
        if (playerMovementScript == null)
            playerMovementScript = FindObjectOfType<PlayerMovement>();

        if (globalLight == null)
        {
            Light2D[] allLights = FindObjectsOfType<Light2D>();
            foreach (var light in allLights)
            {
                if (light.name.ToLower().Contains("global"))
                {
                    globalLight = light;
                    break;
                }
            }
        }

        if (characterLight == null)
        {
            Light2D[] allLights = FindObjectsOfType<Light2D>();
            foreach (var light in allLights)
            {
                if (light.name.ToLower().Contains("character") || light.name.ToLower().Contains("player"))
                {
                    characterLight = light;
                    break;
                }
            }
        }
    }

    // =========================================
    // START
    // =========================================
    private void Start()
    {
        if (!isIntroRunning)
        {
            StartCoroutine(IntroSequence());
        }
    }

    // =========================================
    // INTRO SEQUENCE (Semua logic dipindah ke sini)
    // =========================================
    IEnumerator IntroSequence()
    {
        isIntroRunning = true;

        // Validasi ulang referensi di awal
        RevalidateReferences();

        // 1. Sembunyikan popup di awal
        if (popupSprite != null)
            popupSprite.SetActive(false);

        // 2. Sembunyikan dialogue box di awal
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        // 3. Matikan pergerakan player selama intro
        if (playerMovementScript != null)
            playerMovementScript.canMove = false;

        // 4. Setup awal Lampu
        if (globalLight != null)
            globalLight.intensity = 0.2f;

        // 5. Matikan lampu karakter di awal intro
        if (characterLight != null)
            characterLight.enabled = false;

        // =====================================
        // START AMBIENT LOOP
        // =====================================
        if (ambientCoroutine != null)
            StopCoroutine(ambientCoroutine);

        ambientCoroutine = StartCoroutine(AmbientLoopRoutine());

        // =====================================
        // FORCE BLACK SCREEN
        // =====================================
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.transform.SetAsLastSibling();

            Color color = fadeImage.color;
            color.r = 0f;
            color.g = 0f;
            color.b = 0f;
            color.a = 1f;
            fadeImage.color = color;
        }

        yield return null;
        yield return new WaitForSeconds(0.5f);

        // =====================================
        // FADE OUT
        // =====================================
        yield return StartCoroutine(FadeOut());

        // =====================================
        // PLAY DIALOGUE
        // =====================================
        yield return StartCoroutine(PlayDialogueSequence(introDialogues));

        // =====================================
        // POST-INTRO RESET
        // =====================================

        // 1. Nyalakan kembali lampu karakter
        if (characterLight != null)
            characterLight.enabled = true;

        // 2. Kembalikan intensitas lampu global
        if (globalLight != null)
            globalLight.intensity = 1.0f;

        // 3. Izinkan player bergerak
        if (playerMovementScript != null)
            playerMovementScript.canMove = true;

        // 4. Munculkan popup sprite
        if (popupSprite != null)
            popupSprite.SetActive(true);

        // 5. Matikan fade image
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(false);

        isIntroRunning = false;
    }

    // =========================================
    // AMBIENT LOOP
    // =========================================
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

    // =========================================
    // STOP AMBIENT
    // =========================================
    public void StopAmbientLoop()
    {
        if (ambientCoroutine != null)
        {
            StopCoroutine(ambientCoroutine);
            ambientCoroutine = null;
        }

        if (ambientAudioSource != null)
            ambientAudioSource.Stop();
    }

    // =========================================
    // PLAY DIALOGUE SEQUENCE
    // =========================================
    public IEnumerator PlayDialogueSequence(DialogueData[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0)
            yield break;

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        foreach (DialogueData dialogue in dialogues)
        {
            yield return StartCoroutine(TypeWriter(dialogue));
            yield return new WaitForSeconds(dialogue.textDelay);
        }

        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    // =========================================
    // TYPEWRITER
    // =========================================
    public IEnumerator TypeWriter(DialogueData data)
    {
        if (dialogueTMP == null)
            yield break;

        dialogueTMP.text = string.Empty;

        if (playerNameTMP != null)
            playerNameTMP.text = data.playerName;

        dialogueTMP.ForceMeshUpdate();

        string cleanText = data.dialogueText
            .Replace("\n", "")
            .Replace("\r", "")
            .Trim();

        if (data.dialogueSFX != null && dialogueAudioSource != null)
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

        if (dialogueAudioSource != null)
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
            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }

    // =========================================
    // ON DESTROY - Bersihkan semua coroutine
    // =========================================
    private void OnDestroy()
    {
        StopAllCoroutines();
        isIntroRunning = false;
    }
}