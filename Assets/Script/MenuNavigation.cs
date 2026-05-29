using UnityEngine;
using UnityEngine.EventSystems; // Penting untuk mendeteksi pilihan menu

public class MenuNavigation : MonoBehaviour
{
    public GameObject arrow;      // Masukkan objek panah merah ke sini
    public float xOffset = -150f; // Sesuaikan jaraknya (karena ini UI, angkanya biasanya besar)

    void Update()
    {
        // Cek objek apa yang lagi dipilih oleh EventSystem (Keyboard/Gamepad)
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null && arrow != null)
        {
            // Ambil posisi objek yang lagi dipilih
            Vector3 targetPos = selected.transform.position;

            // Pindahkan panah ke posisi tersebut
            arrow.transform.position = new Vector3(targetPos.x + xOffset, targetPos.y, targetPos.z);
        }
    }
}