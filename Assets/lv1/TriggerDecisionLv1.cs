using UnityEngine;

public class TriggerDecisionLv1 : MonoBehaviour
{
    [Header("UI Pilihan Utama")]
    public GameObject decisionPanel;
    public GameObject dialogueBox;

    [Header("Visual Gambar Figma (Kanan/Kiri)")]
    public GameObject tampilKiri;
    public GameObject tampilKanan;

    private bool diKotakKiri = true;
    private bool panelAktif = false;

    void Update()
    {
        if (panelAktif)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                PindahKeKanan();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                PindahKeKiri();
            }

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
            if (dialogueBox != null) dialogueBox.SetActive(false);

            if (decisionPanel != null)
            {
                decisionPanel.SetActive(true);
                panelAktif = true;
                PindahKeKiri();
            }
        }
    }

    public void PindahKeKanan()
    {
        diKotakKiri = false;
        if (tampilKiri != null) tampilKiri.SetActive(false);
        if (tampilKanan != null) tampilKanan.SetActive(true);
    }

    public void PindahKeKiri()
    {
        diKotakKiri = true;
        if (tampilKiri != null) tampilKiri.SetActive(true);
        if (tampilKanan != null) tampilKanan.SetActive(false);
    }

    public void EksekusiPilihan()
    {
        panelAktif = false;
        if (decisionPanel != null) decisionPanel.SetActive(false);

        if (diKotakKiri)
        {
            Debug.Log("PILIHAN FINAL: GIVE FIRST AID!");
        }
        else
        {
            Debug.Log("PILIHAN FINAL: RUSH TO THE ER!");
        }
    }
}