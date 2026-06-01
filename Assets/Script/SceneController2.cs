using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneController2 : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup fadeOverlay;
    public TextMeshProUGUI introText;

    [Header("Duration Settings")]
    public float fadeInDuration = 1.0f;
    public float displayDuration = 5.0f;
    public float fadeOutDuration = 1.0f;

    [Tooltip("Akan otomatis disesuaikan berdasarkan pilihan karakter (Lyra -> Level 3, Kiro -> Level 3 Kiro)")]
    public string nextSceneName = "Level 3";

    [Header("Typewriter Settings")]
    public string fullText = "Moving to Level 3...";
    public float typingSpeed = 0.05f;

    private AudioSource audioSource;
    public AudioClip typewriterSoundClip;

    private void Start()
    {
        // LOGIKA PENGECEKAN KARAKTER (DISESUAIKAN DENGAN PILIHAN PLAYER):
        // Jika sebelumnya memilih Lyra -> Maka lanjut ke "Level 3"
        if (Select.selectedCharacter == "Lyra")
        {
            nextSceneName = "Level 3";
            Debug.Log("[SceneController2] Karakter Lyra terdeteksi. Target Scene berikutnya: " + nextSceneName);
        }
        // Jika sebelumnya memilih Kiro -> Maka lanjut ke "Level 3 Kiro"
        else if (Select.selectedCharacter == "Kiro")
        {
            nextSceneName = "Level 3 Kiro";
            Debug.Log("[SceneController2] Karakter Kiro terdeteksi. Target Scene berikutnya: " + nextSceneName);
        }

        // Setup AudioSource secara otomatis
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        // PENTING: Set ke 2D Sound agar volume stabil 100%
        audioSource.spatialBlend = 0f;
        audioSource.clip = typewriterSoundClip;

        // Reset tampilan awal
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

        // 3. Menunggu sisa durasi display
        yield return new WaitForSeconds(displayDuration);

        // 4. Fade Out (Layar Transparan ke Gelap)
        yield return StartCoroutine(Fade(0, 1, fadeOutDuration));

        // 5. Pindah ke Scene target sesuai karakter yang dipilih
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
        // 1. MAINKAN AUDIO SECARA LOOP DI AWAL KETIKAN
        if (audioSource != null && typewriterSoundClip != null)
        {
            audioSource.loop = true; // Paksa audio untuk mengulang otomatis
            audioSource.Play();
        }

        // 2. PROSES KETIKAN BERJALAN
        foreach (char c in fullText)
        {
            introText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. SETELAH KETIKAN SELESAI, MATIKAN AUDIO TOTAL
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false; // Kembalikan settingan ke normal
        }
    }
}