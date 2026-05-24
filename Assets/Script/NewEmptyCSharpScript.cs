using UnityEngine;

public class SenterFading : MonoBehaviour
{
    [Header("Pengaturan Senter")]
    [Tooltip("Tarik game object Karakter Lyra ke sini")]
    public Transform targetKarakter; // Karakter Lyra

    [Tooltip("Tarik Material dari Objek Square Hitam ke sini")]
    public Material materialTirai;  // Objek Square Hitam

    void Update()
    {
        // Memastikan kedua variabel sudah diisi di Inspector agar tidak error
        if (targetKarakter != null && materialTirai != null)
        {
            // Mengirim posisi karakter ke parameter "_PlayerPos" di shader material
            materialTirai.SetVector("_PlayerPos", targetKarakter.position);
        }
    }
}