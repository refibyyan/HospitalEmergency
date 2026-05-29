using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("UI Component")]
    public CanvasGroup fadeOverlay;

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    public string targetSceneName = "CharacterSelect";

    void Start()
    {
        // Di awal Main Menu, layar langsung normal dan transparan
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false; // Biar tombol bisa diklik
        }
    }

    public void ClickStartButton()
    {
        StartCoroutine(TransitionSequence());
    }

    IEnumerator TransitionSequence()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.blocksRaycasts = true; // Kunci tombol saat fade berjalan
            
            // Jalankan Fade Out (Terang ke Hitam penuh)
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        // Setelah benar-benar hitam, baru pindah scene
        SceneManager.LoadScene(targetSceneName);
    }
}