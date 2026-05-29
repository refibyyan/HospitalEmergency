using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoctorDialogueTrigger : MonoBehaviour
{
    public SceneIntroManager sceneIntroManager;

    public SceneIntroManager.DialogueData[] doctorDialogues;

    [Header("Player")]
    public PlayerMovement playerMovementScript;

    [Header("Lights")]
    public Light2D globalLight;

    public Light2D characterLight;

    [Header("Popup")]
    public GameObject popupSprite;

    [Header("Dark Transition SFX")]
    public AudioSource darkSFXAudioSource;

    public AudioClip darkSFX;

    [Range(0f, 1f)]
    public float darkSFXVolume = 1f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            StartCoroutine(DoctorSequence());
        }
    }

    IEnumerator DoctorSequence()
    {
        // freeze player movement
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = false;
        }

        // popup hilang
        if (popupSprite != null)
        {
            popupSprite.SetActive(false);
        }

        // dialogue box muncul
        sceneIntroManager.dialogueBox.SetActive(true);

        for (int i = 0; i < doctorDialogues.Length; i++)
        {
            // typewriter
            yield return sceneIntroManager.StartCoroutine(
                sceneIntroManager.TypeWriter(
                    doctorDialogues[i]
                )
            );

            // setelah dialogue ke-2
            if (i == 1)
            {
                // STOP AMBIENT SFX
                sceneIntroManager.StopAmbientLoop();

                // dunia jadi gelap
                if (globalLight != null)
                {
                    globalLight.intensity = 0.02f;
                }

                // PLAY DARK SFX
                if (darkSFXAudioSource != null &&
                    darkSFX != null)
                {
                    darkSFXAudioSource.volume =
                        darkSFXVolume;

                    darkSFXAudioSource.PlayOneShot(
                        darkSFX
                    );
                }
            }

            // jeda antar dialogue
            yield return new WaitForSeconds(
                doctorDialogues[i].textDelay
            );
        }

        // hide dialogue
        sceneIntroManager.dialogueBox.SetActive(false);

        // character light nyala
        if (characterLight != null)
        {
            characterLight.enabled = true;
        }

        // player bisa gerak lagi
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = true;
        }
    }
}