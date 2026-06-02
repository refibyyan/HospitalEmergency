using UnityEngine;

public class GeneratorTrigger : MonoBehaviour
{
    public QuickDecisionManager quickDecisionManager;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip triggerSFX;
    [Range(0f, 1f)] public float volume = 1f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // 🔊 Play sound sekali
            if (audioSource != null && triggerSFX != null)
            {
                audioSource.PlayOneShot(triggerSFX, volume);
            }

            // 🎮 Trigger event
            if (quickDecisionManager != null)
            {
                quickDecisionManager.TriggerGeneratorEvent();
            }
        }
    }
}