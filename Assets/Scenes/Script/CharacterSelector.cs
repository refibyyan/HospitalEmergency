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

    public CharacterCard[] allCards;
    public float fadeSpeed = 1.5f; 
    public float appearanceDelay = 1.0f; 
    private int currentIndex = 0;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip selectingSound;
    public AudioClip pressedSound;

    void Start()
    {
        foreach (var card in allCards)
        {
            Color c = card.cardRenderer.color;
            c.a = 0;
            card.cardRenderer.color = c;
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
                Color c = card.cardRenderer.color;
                c.a = alpha;
                card.cardRenderer.color = c;
            }
            yield return null;
        }
    }

    void Update()
    {
        // Deteksi pindah kartu
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeCard(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeCard(-1);

        // Deteksi tekan Enter untuk pilih karakter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PlaySFX(pressedSound);
            // Tambahkan logika buka Pop Up konfirmasi di sini
        }
    }

    void ChangeCard(int direction)
    {
        currentIndex += direction;
        if (currentIndex >= allCards.Length) currentIndex = 0;
        if (currentIndex < 0) currentIndex = allCards.Length - 1;
        
        // MAINKAN SUARA SAAT PINDAH KARTU
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

    // Fungsi bantuan untuk memutar suara
    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}