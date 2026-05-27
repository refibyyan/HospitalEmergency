using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("--- Fade Settings ---")]
    [SerializeField] private Image layarHitam;
    [SerializeField] private float durasiFade = 2.0f;

    [Header("--- Pop-Up Settings ---")]
    [SerializeField] private GameObject popUpKiriAtas;

    [Header("--- Timer Settings ---")]
    [SerializeField] private GameObject popUpTimer;
    [SerializeField] private TextMeshProUGUI teksDetik;
    [SerializeField] private float waktuTersisa = 20f;

    [Header("--- Audio Settings ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxTimerStart; // Suara jam dinding (akan di-loop)
    [SerializeField] private AudioClip sfxTimerEnd;   // Suara saat level selesai

    private bool timerBerjalan = false;
    private bool levelSelesai = false;

    void Start()
    {
        if (layarHitam != null) layarHitam.gameObject.SetActive(true);
        if (popUpKiriAtas != null) popUpKiriAtas.SetActive(false);
        if (popUpTimer != null) popUpTimer.SetActive(false);

        StartCoroutine(ProsesFadeOut());
    }

    void Update()
    {
        if (timerBerjalan && !levelSelesai)
        {
            if (waktuTersisa > 0)
            {
                waktuTersisa -= Time.deltaTime;
                TampilkanWaktu(waktuTersisa);
            }
            else
            {
                waktuTersisa = 0;
                TampilkanWaktu(waktuTersisa);
                timerBerjalan = false;
                WaktuHabis();
            }
        }
    }

    private IEnumerator ProsesFadeOut()
    {
        float waktuBerjalan = 0f;
        Color warnaAwal = Color.black;
        Color warnaAkhir = new Color(0, 0, 0, 0);

        while (waktuBerjalan < durasiFade)
        {
            waktuBerjalan += Time.deltaTime;
            layarHitam.color = Color.Lerp(warnaAwal, warnaAkhir, waktuBerjalan / durasiFade);
            yield return null;
        }

        layarHitam.gameObject.SetActive(false);

        if (popUpKiriAtas != null) popUpKiriAtas.SetActive(true);
        if (popUpTimer != null) popUpTimer.SetActive(true);

        // ===== PENGATURAN LOOP AUDIO TIMER =====
        if (audioSource != null && sfxTimerStart != null)
        {
            audioSource.clip = sfxTimerStart;
            audioSource.loop = true; // Menyalakan fitur berulang-ulang otomatis
            audioSource.Play();      // Mulai putar audio jam
        }

        timerBerjalan = true;
    }

    private void TampilkanWaktu(float waktuMundur)
    {
        if (teksDetik != null)
        {
            float menit = Mathf.FloorToInt(waktuMundur / 60);
            float detik = Mathf.FloorToInt(waktuMundur % 60);
            teksDetik.text = string.Format("{0:00}:{1:00}", menit, detik);
        }
    }

    private void WaktuHabis()
    {
        Debug.Log("WAKTU HABIS! Game Over.");

        // Matikan suara jam dinding loop karena waktu sudah habis
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    public void SelesaiLevel()
    {
        if (levelSelesai) return;

        levelSelesai = true;
        timerBerjalan = false;

        // Matikan suara jam dinding loop terlebih dahulu
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();

            // Putar suara tanda menang/selesai sekali saja (tidak di-loop)
            if (sfxTimerEnd != null)
            {
                audioSource.PlayOneShot(sfxTimerEnd);
            }
        }

        Debug.Log("Hore! Menyentuh Exit dan Suara End Berbunyi.");
    }
}