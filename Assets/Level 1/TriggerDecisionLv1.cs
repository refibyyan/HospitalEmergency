using UnityEngine;

public class TriggerDecisionLv1 : MonoBehaviour
{
    [Header("UI Pilihan Utama")]
    public GameObject decisionPanel;
    public GameObject dialogueBox;

    [Header("Visual Gambar Figma (Kanan/Kiri)")]
    public GameObject tampilKiri;
    public GameObject tampilKanan;

    [Header("Karakter Player")]
    public MonoBehaviour scriptJalanLyra;

    [Header("Popup Win")]
    public MekanikDecision mekanikDecision;

    private bool diKotakKiri = true;
    private bool panelAktif = false;

    void Start()
    {
        if (decisionPanel != null)
        {
            decisionPanel.SetActive(false);
        }

        panelAktif = false;
    }

    void Update()
    {
        if (panelAktif)
        {
            // pindah kanan
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                PindahKeKanan();
            }

            // pindah kiri
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                PindahKeKiri();
            }

            // pilih
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                EksekusiPilihan();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.name.Contains("lyra"))
        {
            // hilangkan dialogue
            if (dialogueBox != null)
                dialogueBox.SetActive(false);

            // tampilkan panel pilihan
            if (decisionPanel != null)
            {
                decisionPanel.SetActive(true);
                panelAktif = true;

                // default pilihan kiri
                PindahKeKiri();

                // matikan gerak player
                if (scriptJalanLyra != null)
                {
                    scriptJalanLyra.enabled = false;

                    Rigidbody2D rbLyra = other.GetComponent<Rigidbody2D>();

                    if (rbLyra != null)
                    {
                        rbLyra.linearVelocity = Vector2.zero;
                    }
                }
            }
        }
    }

    public void PindahKeKanan()
    {
        diKotakKiri = false;

        if (tampilKiri != null)
            tampilKiri.SetActive(false);

        if (tampilKanan != null)
            tampilKanan.SetActive(true);
    }

    public void PindahKeKiri()
    {
        diKotakKiri = true;

        if (tampilKiri != null)
            tampilKiri.SetActive(true);

        if (tampilKanan != null)
            tampilKanan.SetActive(false);
    }

    public void EksekusiPilihan()
    {
        panelAktif = false;

        // sembunyikan panel pilihan
        if (decisionPanel != null)
            decisionPanel.SetActive(false);

        // aktifkan lagi gerakan player
        if (scriptJalanLyra != null)
            scriptJalanLyra.enabled = true;

        // debug pilihan
        if (diKotakKiri)
        {
            Debug.Log("PILIHAN FINAL: GIVE FIRST AID!");
        }
        else
        {
            Debug.Log("PILIHAN FINAL: RUSH TO THE ER!");
        }

        // munculkan popup menang
        if (mekanikDecision != null)
        {
            mekanikDecision.Menang();
        }
    }
}