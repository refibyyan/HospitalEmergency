using System.Collections;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [System.Serializable]
    public class CharacterCard
    {
        public SpriteRenderer cardRenderer;
        public Sprite spriteON;
        public Sprite spriteOFF;
        public Vector3 positionON;
        public Vector3 positionOFF;
        public Vector3 scaleON = new Vector3(100, 100, 1);
        public Vector3 scaleOFF = new Vector3(100, 100, 1);
    }

    [Header("ESP32 Input Reference")]
    public ESP32Input esp32Input; // Drag GameObject ESP32Input ke sini

    public CharacterCard[] allCards;
    public float fadeSpeed = 1.5f;
    public float appearanceDelay = 1.0f;
    private int currentIndex = 0;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip selectingSound;
    public AudioClip pressedSound;

    private bool isJoystickHorizontalInUse = false;

    void Start()
    {
        if (esp32Input == null)
        {
            esp32Input = FindFirstObjectByType<ESP32Input>();
        }

        foreach (var card in allCards)
        {
            if (card.cardRenderer != null)
            {
                Color c = card.cardRenderer.color;
                c.a = 0;
                card.cardRenderer.color = c;
            }
        }

        RefreshCardDisplay();
        StartCoroutine(FadeInCards());
    }

    IEnumerator FadeInCards()
    {
        yield return new WaitForSeconds(appearanceDelay);

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            foreach (var card in allCards)
            {
                if (card.cardRenderer != null)
                {
                    Color c = card.cardRenderer.color;
                    c.a = alpha;
                    card.cardRenderer.color = c;
                }
            }
            yield return null;
        }
    }

    void Update()
    {
        // 1. Dapatkan input horizontal Hybrid (Keyboard & ESP32 Joystick)
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (esp32Input != null && esp32Input.isConnected)
        {
            if (Mathf.Abs(esp32Input.horizontal) > 0.5f)
            {
                horizontalInput = esp32Input.horizontal;
            }
        }

        // 2. Deteksi ketukan satu frame (Anti-Spam)
        if (horizontalInput != 0)
        {
            if (!isJoystickHorizontalInUse)
            {
                if (horizontalInput > 0.3f) ChangeCard(1);   // Kanan
                if (horizontalInput < -0.3f) ChangeCard(-1); // Kiri
                isJoystickHorizontalInUse = true;
            }
        }
        else
        {
            isJoystickHorizontalInUse = false;
        }

        // 3. Deteksi Enter/Button untuk konfirmasi karakter
        bool isConfirmPressed = Input.GetKeyDown(KeyCode.Return);
        if (esp32Input != null && esp32Input.isConnected && esp32Input.selectPressed)
        {
            isConfirmPressed = true;
        }

        if (isConfirmPressed)
        {
            PlaySFX(pressedSound);
            // Logika popup konfirmasi sudah ditangani secara paralel oleh script "Select.cs"
        }
    }

    void ChangeCard(int direction)
    {
        currentIndex += direction;
        if (currentIndex >= allCards.Length) currentIndex = 0;
        if (currentIndex < 0) currentIndex = allCards.Length - 1;

        PlaySFX(selectingSound);
        RefreshCardDisplay();
    }

    void RefreshCardDisplay()
    {
        for (int i = 0; i < allCards.Length; i++)
        {
            var card = allCards[i];
            if (card.cardRenderer != null)
            {
                bool isSelected = (i == currentIndex);
                card.cardRenderer.sprite = isSelected ? card.spriteON : card.spriteOFF;
                card.cardRenderer.transform.localPosition = isSelected ? card.positionON : card.positionOFF;
                card.cardRenderer.transform.localScale = isSelected ? card.scaleON : card.scaleOFF;
            }
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}