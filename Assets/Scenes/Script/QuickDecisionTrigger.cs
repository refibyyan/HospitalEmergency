using UnityEngine;

public class QuickDecisionTrigger : MonoBehaviour
{
    public QuickDecisionManager manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.StartDecision();
        }
    }
}