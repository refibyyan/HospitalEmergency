using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup fadeOverlay;
    public TextMeshProUGUI introText;

    [Header("Duration Settings")]
    public float fadeInDuration = 1.0f;
    public float displayDuration = 5.0f;
    public float fadeOutDuration = 1.0f;

    [Tooltip("Akan otomatis disesuaikan berdasarkan pilihan karakter (Lyra -> Level 2 Lyra, Kiro -> Level 2)")]
    public string nextSceneName = "Level 2 Lyra";

    [Header("Typewriter Settings")]
    public string fullText = "Moving to Level 2...";
    public float typingSpeed = 0.05f;

    private AudioSource audioSource;
    public AudioClip typewriterSoundClip;

    private void Start()
    {
        // PENGECEKAN KONDISI YANG SUDAH DIPERBAIKI:
        // Jika sebelumnya memilih Lyra -> Maka lanjut ke "Level 2 Lyra"
        if (Select.selectedCharacter == "Lyra")
        {
            nextSceneName = "Level 2 Lyra";
            Debug.Log("[SceneController] Karakter Lyra terdeteksi. Target Scene berikutnya: " + nextSceneName);
        }
        // Jika sebelumnya memilih Kiro -> Maka lanjut ke "Level 2"
        else if (Select.selectedCharacter == "Kiro")
        {
            nextSceneName = "Level 2";
            Debug.Log("[SceneController] Karakter Kiro terdeteksi. Target Scene berikutnya: " + nextSceneName);
        }

        // Setup AudioSource secara otomatis lewat code
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = typewriterSoundClip;

        // Pastikan overlay menutupi layar di awal
        fadeOverlay.alpha = 1;
        introText.text = "";
        StartCoroutine(SceneSequence());
    }

    IEnumerator SceneSequence()
    {
        // 1. Fade In (Layar Gelap ke Transparan)
        yield return StartCoroutine(Fade(1, 0, fadeInDuration));

        // 2. Typewriter Effect & SFX
        yield return StartCoroutine(TypeText());

        // 3. Menunggu sisa durasi scene (Total 5 detik dari awal)
        yield return new WaitForSeconds(displayDuration);

        // 4. Fade Out (Layar Transparan ke Gelap)
        yield return StartCoroutine(Fade(0, 1, fadeOutDuration));

        // 5. Pindah ke Level berikutnya sesuai dengan logika di atas
        Debug.Log("Memuat Scene: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        fadeOverlay.alpha = end;
    }

    IEnumerator TypeText()
    {
        foreach (char c in fullText)
        {
            introText.text += c;

            if (audioSource != null && typewriterSoundClip != null)
            {
                audioSource.Stop();
                audioSource.Play();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}