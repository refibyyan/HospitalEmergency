using UnityEngine;

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
}