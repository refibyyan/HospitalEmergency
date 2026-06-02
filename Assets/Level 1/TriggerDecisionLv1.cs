using UnityEngine;

public class TriggerDecisionLv1 : MonoBehaviour
{
    [Header("UI Decision")]
    public GameObject decisionPanel;

    [Header("Dialogue awal")]
    public GameObject dialogueBox;

    [Header("Cutscene")]
    public CutsceneTyping cutscene;

    [Header("Player Movement")]
    public MonoBehaviour playerMovement; // drag script movement

    [Header("Player Rigidbody")]
    public Rigidbody2D playerRb; // 👉 drag Rigidbody Player ke sini

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

        if (other.CompareTag("Player") || other.name.ToLower().Contains("lyra"))
        {
            sudahTrigger = true;

            // Matikan dialogue
            if (dialogueBox != null)
                dialogueBox.SetActive(false);

            // 🔥 FREEZE TOTAL PLAYER

            // 1. Disable script movement
            if (playerMovement != null)
                playerMovement.enabled = false;

            // 2. Stop velocity
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                // 3. Ubah jadi STATIC (INI YANG PALING PENTING)
                rb.bodyType = RigidbodyType2D.Static;
            }

            // (backup kalau kamu drag manual)
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.bodyType = RigidbodyType2D.Static;
            }

            // ▶️ Cutscene
            if (cutscene != null)
                cutscene.PlayCutscene(this);
        }
    }

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

        // 🔥 BALIKIN PLAYER

        // enable movement lagi
        if (playerMovement != null)
            playerMovement.enabled = true;

        // balikin Rigidbody ke Dynamic
        if (playerRb != null)
            playerRb.bodyType = RigidbodyType2D.Dynamic;

        Debug.Log("PILIHAN DIKONFIRM");
    }
}