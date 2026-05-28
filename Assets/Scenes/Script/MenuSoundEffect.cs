using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSoundEffects : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    public AudioSource sfxSource;
    public AudioClip selectingSound;
    public AudioClip pressedSound;

    // Trigger saat navigasi pake arrow/keyboard
    public void OnSelect(BaseEventData eventData)
    {
        PlaySound(selectingSound);
    }

    // Trigger saat kursor mouse menyentuh button
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlaySound(selectingSound);
    }

    // Panggil fungsi ini di OnClick() button melalui Inspector
    public void PlayConfirmSound()
    {
        PlaySound(pressedSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}