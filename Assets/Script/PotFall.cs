using UnityEngine;

public class PotFall : MonoBehaviour
{
    [Header("Sprite")]
    public Sprite standingSprite;
    public Sprite fallenSprite;

    [Header("Player")]
    public Transform player;
    public float triggerDistance = 2f;

    [Header("SFX")]
    public AudioClip breakSound;

    private SpriteRenderer sr;
    private AudioSource audioSource;

    private bool hasFallen = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Pastikan awal sprite berdiri
        sr.sprite = standingSprite;
    }

    void Update()
    {
        if (hasFallen) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= triggerDistance)
        {
            FallPot();
        }
    }

    void FallPot()
    {
        hasFallen = true;

        sr.sprite = fallenSprite;

        transform.rotation = Quaternion.Euler(0, 0, -90);

        if (breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }
    }
}