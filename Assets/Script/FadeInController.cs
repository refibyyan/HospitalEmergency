using System.Collections;
using UnityEngine;

public class FadeInController : MonoBehaviour
{
    [Header("UI Component")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1.0f;

    void Start()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f; // Mulai dari hitam pekat saat scene baru masuk
            fadeOverlay.blocksRaycasts = true;
            StartCoroutine(FadeInRoutine());
        }
    }

    IEnumerator FadeInRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false; // Buka kunci UI setelah terang
    }
}