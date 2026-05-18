using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorySequenceManager : MonoBehaviour
{ 
    [Header("UI Components")]
    public Image backgroundImage;
    public TextMeshProUGUI storyText; // Ini untuk aktif/nonaktifkan objek teks
    public CanvasGroup fadeOverlay; 

    // Tipenya sudah diganti ke TypewriterEffect agar sinkron
    private TypewriterEffectLoading typewriter; 

    [Header("Story Assets - Part 1 (5 Detik)")]
    public Sprite bgSprite1;
    [TextArea(3, 5)] public string textContent1;
    public AudioClip audioPart1;

    [Header("Story Assets - Part 2 (11 Detik - Blank)")]
    public AudioClip audioPart2;

    [Header("Story Assets - Part 3")]
    public Sprite bgSprite3;
    [TextArea(3, 5)] public string textContent3;
    public AudioClip audioPart3;

    [Header("Settings")]
    public float fadeDuration = 1.0f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        // OTOMATIS: Script akan mencari sendiri komponen TypewriterEffect yang ada di Story Text
        if (storyText != null)
        {
            typewriter = storyText.GetComponent<TypewriterEffectLoading>();
        }

        StartCoroutine(RunStorySequence());
    }

    IEnumerator RunStorySequence()
    {
        // ----------------------------------------------------------------
        // SEKUENS 1: BG 1 + Text 1 (Durasi Teks Selesai + Jeda 5 Detik)
        // ----------------------------------------------------------------
        fadeOverlay.alpha = 1; 
        backgroundImage.sprite = bgSprite1;
        backgroundImage.gameObject.SetActive(true);
        storyText.gameObject.SetActive(true);

        // 1. Fade In Mulai (Hitam ke Terang)
        yield return StartCoroutine(Fade(1, 0));

        if (audioPart1 != null) audioSource.PlayOneShot(audioPart1);
        
        // 2. Jalankan Typewriter
        if (typewriter != null) typewriter.SetupAndPlay(textContent1); 

        // 3. JANGAN LANGSUNG FADE. Kita tunggu dulu selama 5 detik penuh 
        // agar player bisa membaca teksnya setelah selesai diketik.
        yield return new WaitForSeconds(5f);

        // 4. Fade Out Mulai (Terang ke Hitam) - Kita pastikan transisinya berjalan
        Debug.Log("Mulai Fade Out ke Hitam...");
        yield return StartCoroutine(Fade(0, 1));
        Debug.Log("Selesai Fade Out. Layar sudah hitam.");

        // ----------------------------------------------------------------
        // SEKUENS 2: Blank Hitam + Audio Baru (Durasi 11 Detik)
        // ----------------------------------------------------------------
        backgroundImage.gameObject.SetActive(false);
        storyText.gameObject.SetActive(false);

        if (audioPart2 != null) audioSource.PlayOneShot(audioPart2);

        // Tunggu di dalam kegelapan selama 11 detik
        yield return new WaitForSeconds(11f);

        // ----------------------------------------------------------------
        // SEKUENS 3: BG 3 + Text 3
        // ----------------------------------------------------------------
        backgroundImage.sprite = bgSprite3;
        backgroundImage.gameObject.SetActive(true);
        storyText.gameObject.SetActive(true);

        // Fade In Kembali (Hitam ke Terang)
        yield return StartCoroutine(Fade(1, 0));

        if (audioPart3 != null) audioSource.PlayOneShot(audioPart3);
        
        if (typewriter != null) typewriter.SetupAndPlay(textContent3); 
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