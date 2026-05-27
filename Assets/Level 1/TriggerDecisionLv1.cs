using UnityEngine;

public class TriggerDecisionLv1 : MonoBehaviour
{
    [Header("UI Decision")]
    public GameObject decisionPanel;

    [Header("Dialogue awal")]
    public GameObject dialogueBox;

    [Header("Cutscene")]
    public CutsceneTyping cutscene;

    [Header("Player")]
    public MonoBehaviour scriptJalanLyra;

    private bool sudahTrigger = false;
    private bool panelAktif = false;

    void Start()
    {
        if (decisionPanel != null)
            decisionPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sudahTrigger) return;

        if (other.CompareTag("Player") || other.name.Contains("lyra"))
        {
            sudahTrigger = true;

            if (dialogueBox != null)
                dialogueBox.SetActive(false);

            if (scriptJalanLyra != null)
                scriptJalanLyra.enabled = false;

            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // 👉 CUTSCENE JALAN DULU
            if (cutscene != null)
                cutscene.PlayCutscene(this);
        }
    }

    // dipanggil dari cutscene
    public void ShowDecision()
    {
        if (decisionPanel == null) return;

        decisionPanel.SetActive(true);
        panelAktif = true;
    }

    public void EksekusiPilihan()
    {
        if (decisionPanel != null)
            decisionPanel.SetActive(false);

        if (scriptJalanLyra != null)
            scriptJalanLyra.enabled = true;

        Debug.Log("PILIHAN DIKONFIRM");
    }
}