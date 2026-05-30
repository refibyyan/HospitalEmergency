using System.Collections;
using UnityEngine;

public class OpeningAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip codeBlue;
    public AudioClip monitorJantung;

    IEnumerator Start()
    {
        if (codeBlue != null)
        {
            audioSource.PlayOneShot(codeBlue);

            yield return new WaitForSeconds(codeBlue.length);
        }

        if (monitorJantung != null)
        {
            audioSource.clip = monitorJantung;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}