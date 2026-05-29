using UnityEngine;
using UnityEngine.SceneManagement; // PENTING: Wajib ditambahkan untuk mendeteksi scene

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Membuat Singleton agar objek tidak duplikat saat balik ke Main Menu
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Menjaga audio tetap hidup antar scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true; // Memastikan lagu me-loop otomatis
        audioSource.Play();
    }

    // Mengaktifkan deteksi perpindahan scene saat object aktif
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Mematikan deteksi saat object dinonaktifkan/hancur (mencegah memory leak)
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fungsi ini otomatis jalan SETIAP KALI pindah scene
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (audioSource == null) return;

        // Jika pindah ke scene "Loading1", matikan musiknya
        if (scene.name == "Loading1")
        {
            audioSource.Stop(); 
        }
        else if (scene.name == "Main Menu" || scene.name == "CharacterSelect")
        {
            // Jika balik lagi ke Main Menu atau CharacterSelect dan musik sedang mati, putar lagi
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}