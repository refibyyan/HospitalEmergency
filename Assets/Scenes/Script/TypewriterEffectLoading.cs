using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffectLoading : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private AudioSource typingAudio;
    
    // Properti agar bisa dibaca oleh StorySequenceManager
    public bool IsTyping { get; private set; } 

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        typingAudio = GetComponent<AudioSource>();
        
        // Pastikan audio tidak main otomatis saat start
        if(typingAudio != null) typingAudio.playOnAwake = false;
    }

    public void SetupAndPlay(string content, float speed)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(content, speed));
    }

    IEnumerator TypeText(string content, float speed)
    {
        IsTyping = true;
        textComponent.text = "";

        // Mulai suara ketikan
        if (typingAudio != null) 
        {
            typingAudio.loop = true; // Paksa loop agar suara tidak putus di tengah teks panjang
            typingAudio.Play();
        }

        foreach (char c in content.ToCharArray())
        {
            textComponent.text += c;
            // Menunggu sesuai kecepatan yang diatur di manager
            yield return new WaitForSeconds(speed);
        }

        // --- KUNCI PERBAIKAN ---
        // Segera hentikan audio tepat setelah loop huruf selesai
        if (typingAudio != null) 
        {
            typingAudio.Stop(); 
        }
        
        IsTyping = false; 
    }
}