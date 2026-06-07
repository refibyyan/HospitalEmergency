using UnityEngine;

public class TriggerDecisionLv1 : MonoBehaviour
{
    [Header("UI Decision")]
    [Tooltip("Drag GameObject yang memegang script MekanikDecision atau Panel Decision ke sini")]
    public GameObject decisionPanel;

    [Header("Dialogue awal")]
    public GameObject dialogueBox;

    [Header("Player Movement")]
    public MonoBehaviour playerMovement; // drag script movement

    [Header("Player Rigidbody")]
    public Rigidbody2D playerRb; // 👉 drag Rigidbody Player ke sini

    private bool sudahTrigger = false;

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

            // Matikan dialogue box awal jika masih ada yang aktif
            if (dialogueBox != null)
                dialogueBox.SetActive(false);

            // 🔥 FREEZE TOTAL PLAYER
            // 1. Disable script movement
            if (playerMovement != null)
                playerMovement.enabled = false;

            // 2. Stop velocity via Collider component
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                // 3. Ubah jadi STATIC (Agar tidak terpengaruh physics/gravitasi saat memilih)
                rb.bodyType = RigidbodyType2D.Static;
            }

            // (backup kalau kamu drag manual di Inspector)
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.bodyType = RigidbodyType2D.Static;
            }

            // ▶️ ALUR BARU: Langsung panggil panel decision di sini saat nabrak trigger
            MulaiMekanikDecision();
        }
    }

    // Mengaktifkan panel decision/pilihan di awal flow
    public void MulaiMekanikDecision()
    {
        if (decisionPanel == null) return;

        decisionPanel.SetActive(true);
        
        // Mengambil script MekanikDecision secara otomatis jika menempel di game object yang sama
        MekanikDecision mekanik = decisionPanel.GetComponent<MekanikDecision>();
        if (mekanik != null)
        {
            // Pastikan timeScale diatur normal di awal mekanik agar input joystick/keyboard terbaca
            Time.timeScale = 1f; 
        }
    }

    // Fungsi ini bisa dipanggil jika kamu membutuhkan reset player ke kondisi normal di luar alur Menang/Pindah Scene
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