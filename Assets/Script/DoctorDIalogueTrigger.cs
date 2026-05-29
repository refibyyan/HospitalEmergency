using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoctorDialogueTrigger : MonoBehaviour
{
    public SceneIntroManager sceneIntroManager;

    public SceneIntroManager.DialogueData[] doctorDialogues;

    [Header("Quick Decision")]
    public QuickDecisionManager quickDecisionManager;

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
        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = false;
        }

        if (popupSprite != null)
        {
            popupSprite.SetActive(false);
        }

        sceneIntroManager.dialogueBox.SetActive(true);

        for (int i = 0; i < doctorDialogues.Length; i++)
        {
            yield return sceneIntroManager.StartCoroutine(
                sceneIntroManager.TypeWriter(
                    doctorDialogues[i]
                )
            );

            if (i == 1)
            {
                sceneIntroManager.StopAmbientLoop();

                if (globalLight != null)
                {
                    globalLight.intensity = 0.02f;
                }

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

            yield return new WaitForSeconds(
                doctorDialogues[i].textDelay
            );
        }

        sceneIntroManager.dialogueBox.SetActive(false);

        if (characterLight != null)
        {
            characterLight.enabled = true;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.canMove = true;
        }

        // AKTIFKAN TRIGGER DECISION
        if (quickDecisionManager != null)
        {
            quickDecisionManager.ActivateDecisionTrigger();
        }
    }
}