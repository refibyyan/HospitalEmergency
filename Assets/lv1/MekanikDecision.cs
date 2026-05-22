using UnityEngine;
using UnityEngine.UI;
using TMPro; // Ditambahkan agar bisa membaca komponen TextMeshPro untuk countdown detiknya

public class MekanikDecision : MonoBehaviour
{
    [Header("Object UI di Hierarchy")]
    public Image cardKiriUI;   // Tarik object cardKiri dari Hierarchy ke sini
    public Image cardKananUI;  // Tarik object cardKanan dari Hierarchy ke sini

    [Header("Aset Gambar Kiri (Sprite)")]
    public Sprite gambarKiriIjo;   // Gambar Kiri yang ada border + panah ijo
    public Sprite gambarKiriPolos; // Gambar Kiri yang POLOS

    [Header("Aset Gambar Kanan (Sprite)")]
    public Sprite gambarKananIjo;   // Gambar Kanan yang ada border + panah ijo
    public Sprite gambarKananPolos; // Gambar Kanan yang POLOS

    // ==========================================
    // TAMBAHAN FUNGSI YANG DIBUTUHKAN (TIMER)
    // ==========================================
    [Header("Sistem Waktu / Timer Bar")]
    public Image barMerahTimer;       // Tarik objek TimerRed ke sini
    public TMP_Text teksCountdown;    // Tarik objek TeksWaktu ke sini
    public float waktuMaksimal = 15f; // Durasi waktu berpikir dalam detik
    private float waktuBerjalan;
    // ==========================================

    private bool pilihKiri = true;

    void Start()
    {
        // Pas awal mulai, langsung atur tampilan awal (kiri ijo, kanan polos)
        AturGambarPilihan();

        // TAMBAHAN: Set waktu berjalan sesuai waktu maksimal saat game dimulai
        waktuBerjalan = waktuMaksimal;
        if (barMerahTimer != null) barMerahTimer.fillAmount = 1f;
    }

    void Update()
    {
        // ==========================================
        // TAMBAHAN: LOGIKA COUNTDOWN TIMER
        // ==========================================
        if (waktuBerjalan > 0)
        {
            waktuBerjalan -= Time.deltaTime; // Kurangi waktu setiap detik

            // Update visual bar merah (mengubah nilai Fill Amount dari 1 menuju 0)
            if (barMerahTimer != null)
            {
                barMerahTimer.fillAmount = waktuBerjalan / waktuMaksimal;
            }

            // Update teks angka detiknya (dibulatkan ke atas jadi angka utuh)
            if (teksCountdown != null)
            {
                teksCountdown.text = Mathf.CeilToInt(waktuBerjalan).ToString();
            }
        }
        else
        {
            // Panggil fungsi tambahan jika waktu habis dan player belum memilih
            WaktuHabisBatalMilih();
        }
        // ==========================================

        // Deteksi tombol kiri / A (Bawaan asli kamu, tidak disentuh)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihKiri = true;
            AturGambarPilihan();
        }
        // Deteksi tombol kanan / D (Bawaan asli kamu, tidak disentuh)
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihKiri = false;
            AturGambarPilihan();
        }

        // Eksekusi kalau pencet Enter / Space (Bawaan asli kamu, tidak disentuh)
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (pilihKiri)
            {
                Debug.Log("Memilih: GIVE FIRST AID");
            }
            else
            {
                Debug.Log("Memilih: RUSH TO THE ER");
            }
        }
    }

    void AturGambarPilihan() // (Bawaan asli kamu, tidak disentuh)
    {
        if (pilihKiri == true)
        {
            // Kiri pakai yang ijo, Kanan pakai yang polos
            cardKiriUI.sprite = gambarKiriIjo;
            cardKananUI.sprite = gambarKananPolos;
        }
        else
        {
            // Kiri pakai yang polos, Kanan pakai yang ijo
            cardKiriUI.sprite = gambarKiriPolos;
            cardKananUI.sprite = gambarKananIjo;
        }
    }

    // ==========================================
    // TAMBAHAN: FUNGSI OTOMATIS JIKA WAKTU HABIS
    // ==========================================
    void WaktuHabisBatalMilih()
    {
        Debug.Log("WAKTU HABIS! Player gagal memilih tindakan tepat waktu!");
        // Kamu bisa taruh efek game over atau panel close di sini nanti
        enabled = false; // Mematikan script agar tidak looping error waktu habis
    }
}