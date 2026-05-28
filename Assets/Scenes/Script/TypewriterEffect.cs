using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [System.Serializable]
    public class SoundSession
    {
        public float startAtSecond; 
        public float duration;      
    }

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float startDelay = 0f;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip typingSound;
    public float minTimeBetweenSounds = 0.1f; 

    [Header("Sound Timing (Custom)")]
    public List<SoundSession> soundSessions; 

    private TMP_Text textMesh;
    private string fullText;
    private float lastSoundTime;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh != null)
        {
            fullText = textMesh.text;
            textMesh.text = "";
        }
    }

    void OnEnable()
    {
        if (textMesh != null)
        {
            textMesh.text = "";
            lastSoundTime = 0;
            StopAllCoroutines(); 
            StartCoroutine(BeginTypewriter());
        }
    }

    IEnumerator BeginTypewriter()
    {
        yield return new WaitForSeconds(startDelay);
        float customTimer = 0f; 

        foreach (char c in fullText)
        {
            textMesh.text += c;

            // Jika masuk dalam sesi suara
            if (c != ' ' && IsInSoundSession(customTimer))
            {
                if (Time.time - lastSoundTime >= minTimeBetweenSounds)
                {
                    if (sfxSource != null && typingSound != null)
                    {
                        sfxSource.clip = typingSound;
                        sfxSource.Play();
                        lastSoundTime = Time.time;
                    }
                }
            }
            else
            {
                // BERHENTI PAKSA: Jika sudah di luar durasi, matikan suara segera
                if (sfxSource != null && sfxSource.isPlaying && sfxSource.clip == typingSound)
                {
                    sfxSource.Stop();
                }
            }

            customTimer += typingSpeed; 
            yield return new WaitForSeconds(typingSpeed);
        }

        // Pastikan suara mati saat teks benar-benar selesai
        if (sfxSource != null) sfxSource.Stop();
    }

    bool IsInSoundSession(float time)
    {
        if (soundSessions == null || soundSessions.Count == 0) return false;
        foreach (var session in soundSessions)
        {
            if (time >= session.startAtSecond && time <= (session.startAtSecond + session.duration))
                return true;
        }
        return false;
    }

    // TAMBAHKAN FUNGSI INI DI PALING BAWAH SCRIPT TYPEWRITER-MU
    public void SetupAndPlay(string newText)
    {
        if (textMesh == null) textMesh = GetComponent<TMP_Text>();
        
        fullText = newText; // Memasukkan teks baru dari Story Manager
        textMesh.text = "";
        lastSoundTime = 0;
        
        StopAllCoroutines(); 
        StartCoroutine(BeginTypewriter());
    }
}