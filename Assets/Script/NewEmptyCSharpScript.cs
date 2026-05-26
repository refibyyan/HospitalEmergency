using UnityEngine;

public class PotTrap : MonoBehaviour
{
    [Header("Target Pot & Physics")]
    public Rigidbody2D potRigidbody; // Tempat naruh Rigidbody si Pot_Trap
    public SpriteRenderer potSpriteRenderer; // Tempat naruh SpriteRenderer si Pot_Trap

    [Header("Broken Sprite Asset")]
    public Sprite brokenPotSprite; // Tempat naruh gambar pot jatoh (tumpah)

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang lewat itu player kamu (lyra_depan atau Kiro)
        if ((other.gameObject.name == "lyra_depan" || other.gameObject.name == "Kiro") && !isTriggered)
        {
            isTriggered = true;
            DropPot();
        }
    }

    void DropPot()
    {
        // Mengubah pot dari Kinematic jadi Dynamic biar jatuh kena gravitasi
        if (potRigidbody != null)
        {
            potRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }

        // Memberi jeda waktu jatuh sekitar 0.4 detik sebelum gambarnya berubah jadi tumpah
        Invoke("ChangeToBrokenSprite", 0.4f);

        Debug.Log("Pot Jatuh! Jalan tertutup.");
    }

    void ChangeToBrokenSprite()
    {
        if (brokenPotSprite != null && potSpriteRenderer != null)
        {
            // Mengubah gambar pot berdiri jadi pot tumpah berserakan
            potSpriteRenderer.sprite = brokenPotSprite;
        }
    }
}