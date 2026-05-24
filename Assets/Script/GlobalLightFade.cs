using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class GlobalLightFade : MonoBehaviour
{
    [Header("Global Light")]
    public Light2D globalLight;

    [Header("Timing")]
    public float startAtSecond = 5f;

    [Header("Fade")]
    public float fadeDuration = 2f;

    [Header("Intensity")]
    public float startIntensity = 1f;
    public float targetIntensity = 0.1f;

    void Start()
    {
        // set terang awal
        globalLight.intensity = startIntensity;

        // mulai coroutine
        StartCoroutine(FadeGlobalLight());
    }

    IEnumerator FadeGlobalLight()
    {
        // tunggu sesuai detik
        yield return new WaitForSeconds(startAtSecond);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            globalLight.intensity = Mathf.Lerp(
                startIntensity,
                targetIntensity,
                timer / fadeDuration
            );

            yield return null;
        }

        // pastikan intensity final tepat
        globalLight.intensity = targetIntensity;
    }
}