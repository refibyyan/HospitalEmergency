using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Tarik karakter kamu ke sini di Inspector
    public float smoothSpeed = 0.125f; // Semakin kecil, semakin smooth kameranya
    public Vector3 offset = new Vector3(0, 0, -10); // Jaga jarak Z agar kamera tidak menempel pas di karakter

    void LateUpdate()
    {
        if (target != null)
        {
            // Tentukan posisi tujuan kamera (hanya ikuti X dan Y, Z pakai offset)
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);

            // Haluskan pergerakan dari posisi sekarang ke posisi tujuan
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Terapkan posisi baru ke kamera
            transform.position = smoothedPosition;
        }
    }
}