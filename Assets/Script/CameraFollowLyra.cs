using UnityEngine;

public class CameraFollowLyra : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (target == null) return;

        // Kamera langsung ngikut target TANPA delay/geter
        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            offset.z
        );
    }
}