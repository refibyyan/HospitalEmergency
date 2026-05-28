using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Tambahkan ini untuk pindah scene

public class StorySequenceManager : MonoBehaviour
{
    [Header("UI Components")]
    public Image backgroundImage;
    public TextMeshProUGUI storyText;
    public CanvasGroup fadeOverlay;

    [Header("Story Assets")]
    // ... (Part 1 & 2 tetap sama)
    public Sprite bgSprite1;
    [TextArea(3, 5)] public string textContent1;
    public AudioClip audioPart1;

    public AudioClip audioPart2;

    public Sprite bgSprite3;
    [TextArea(3, 5)] public string textContent3;
    public AudioClip audioPart3;

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    public float postAudioDelay = 1.0f;
    public float typingSpeed = 0.05f;
    
    [Header("Exit Settings")]
    public string nextSceneName = "MainMenu"; // Nama scene tujuan
    public float part3TotalDuration = 7.0f;   // Durasi paksa Part 3

    private AudioSource audioSource;
    private TypewriterEffectLoading typewriter;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (storyText != null)
            typewriter = storyText.GetComponent<TypewriterEffectLoading>();

        fadeOverlay.alpha = 1;
        StartCoroutine(RunStorySequence());
    }

    IEnumerator RunStorySequence()
    {
        // ================================================================
        // SEKUENS 1 & 2 (Tetap sama seperti kode sebelumnya)
        // ================================================================
        // ... (Kode Sekuens 1)
        backgroundImage.sprite = bgSprite1;
        backgroundImage.gameObject.SetActive(true);
        storyText.text = "";
        yield return StartCoroutine(Fade(1, 0));
        if (audioPart1 != null) {
            audioSource.clip = audioPart1;
            audioSource.Play();
            if (typewriter != null) typewriter.SetupAndPlay(textContent1, typingSpeed);
            while (typewriter != null && typewriter.IsTyping) yield return null;
            yield return new WaitForSeconds(postAudioDelay);
        }
        yield return StartCoroutine(Fade(0, 1));

        // ... (Kode Sekuens 2)
        backgroundImage.gameObject.SetActive(false);
        storyText.gameObject.SetActive(false);
        if (audioPart2 != null) {
            audioSource.clip = audioPart2;
            audioSource.Play();
            yield return new WaitForSeconds(audioPart2.length);
        }

        // ================================================================
        // SEKUENS 3: Dibatasi 7 Detik
        // ================================================================
        backgroundImage.sprite = bgSprite3;
        backgroundImage.gameObject.SetActive(true);
        storyText.gameObject.SetActive(true);
        storyText.text = ""; 

        yield return StartCoroutine(Fade(1, 0));

        if (audioPart3 != null)
        {
            audioSource.clip = audioPart3;
            audioSource.Play();
            if (typewriter != null) typewriter.SetupAndPlay(textContent3, typingSpeed);
        }

        // TUNGGU TEPAT 7 DETIK (Terlepas dari panjang teks/audio)
        yield return new WaitForSeconds(part3TotalDuration);

        // FADE OUT TERAKHIR SEBELUM PINDAH
        yield return StartCoroutine(Fade(0, 1));
        
        Debug.Log("Pindah ke Scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = endAlpha;
    }
}