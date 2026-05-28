using UnityEngine;

public class RestartTrigger : MonoBehaviour
{
    public QuickDecisionManager quickDecisionManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            quickDecisionManager.TriggerRestartEvent();
        }
    }
}