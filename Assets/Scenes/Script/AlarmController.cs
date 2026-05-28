using UnityEngine;
using UnityEngine.UI;

public class AlarmController : MonoBehaviour
{
    [Header("UI Element")]
    public GameObject alarmOverlay; // Tempat naruh objek Alarm_Overlay

    [Header("Alarm Settings")]
    public float flashSpeed = 2f; // Kecepatan kedap-kedip lampu merah
    private Image overlayImage;
    private bool isAlarmActive = false;

    void Start()
    {
        if (alarmOverlay != null)
        {
            overlayImage = alarmOverlay.GetComponent<Image>();
            // Matikan alarm di awal game biar player bisa liat map dulu
            alarmOverlay.SetActive(false);
        }
    }

    void Update()
    {
        // Jika alarm aktif, bikin efek kedap-kedip merah melarut (fade)
        if (isAlarmActive && overlayImage != null)
        {
            float alpha = Mathf.PingPong(Time.time * flashSpeed, 0.5f); // Maksimal kepekatan merah 50%
            Color newColor = overlayImage.color;
            newColor.a = alpha;
            overlayImage.color = newColor;
        }
    }

    // Fungsi untuk menyalakan alarm dari script lain (misal pas pot jatuh atau waktu habis)
    public void StartAlarm()
    {
        isAlarmActive = true;
        if (alarmOverlay != null)
        {
            alarmOverlay.SetActive(true);
        }
    }

    // Fungsi untuk mematikan alarm
    public void StopAlarm()
    {
        isAlarmActive = false;
        if (alarmOverlay != null)
        {
            alarmOverlay.SetActive(false);
        }
    }
}