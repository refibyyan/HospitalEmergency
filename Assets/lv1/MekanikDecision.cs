using UnityEngine;
using UnityEngine.UI; 

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

    private bool pilihKiri = true; 

    void Start()
    {
        // Pas awal mulai, langsung atur tampilan awal (kiri ijo, kanan polos)
        AturGambarPilihan();
    }

    void Update()
    {
        // Deteksi tombol kiri / A
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            pilihKiri = true;
            AturGambarPilihan();
        }
        // Deteksi tombol kanan / D
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            pilihKiri = false;
            AturGambarPilihan();
        }

        // Eksekusi kalau pencet Enter / Space
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (pilihKiri) {
                Debug.Log("Memilih: GIVE FIRST AID");
            } else {
                Debug.Log("Memilih: RUSH TO THE ER");
            }
        }
    }

    void AturGambarPilihan()
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
}