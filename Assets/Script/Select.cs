using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Select : MonoBehaviour
{
    // Variabel statis untuk menyimpan pilihan karakter agar bisa dibaca di scene Loading1
    public static string selectedCharacter = "Lyra";

    [Header("Kiro Pop Up")]
    public GameObject kiroPopUpObj;
    public Image kiroImage;
    public Sprite kiroConfirmSprite;
    public Sprite kiroCancelSprite;

    [Header("Lyra Pop Up")]
    public GameObject lyraPopUpObj;
    public Image lyraImage;
    public Sprite lyraConfirmSprite;
    public Sprite lyraCancelSprite;

    [Header("General")]
    public Button kiroButton;
    public Button lyraButton;
    public GameObject blurPanel;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip selectingSound;
    public AudioClip pressedSound;

    private bool isPopUpActive = false;
    private bool isConfirmSelected = true;
    private string activeCharacter = "";

    void Start()
    {
        kiroButton.Select();
    }

    void Update()
    {
        if (!isPopUpActive)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                PlaySFX(selectingSound);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                GameObject current = EventSystem.current.currentSelectedGameObject;

                if (current == kiroButton.gameObject)
                {
                    activeCharacter = "Kiro";
                    PlaySFX(pressedSound);
                    OpenPopUp(kiroPopUpObj);
                }
                else if (current == lyraButton.gameObject)
                {
                    activeCharacter = "Lyra";
                    PlaySFX(pressedSound);
                    OpenPopUp(lyraPopUpObj);
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                isConfirmSelected = !isConfirmSelected;
                PlaySFX(selectingSound);
                RefreshSprite();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                PlaySFX(pressedSound);
                if (isConfirmSelected)
                {
                    Debug.Log("Game Start: " + activeCharacter);

                    // Simpan data pilihan ke variabel statis sebelum pindah scene
                    selectedCharacter = activeCharacter;

                    // PINDAH SCENE: Langsung meloncat ke scene Loading1
                    SceneManager.LoadScene("Loading1");
                }
                else
                {
                    CloseAll();
                }
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

    void OpenPopUp(GameObject popUp)
    {
        isPopUpActive = true;
        popUp.SetActive(true);
        if (blurPanel != null) blurPanel.SetActive(true);
        isConfirmSelected = true;
        RefreshSprite();
    }

    void RefreshSprite()
    {
        if (activeCharacter == "Kiro")
        {
            kiroImage.sprite = isConfirmSelected ? kiroConfirmSprite : kiroCancelSprite;
        }
        else
        {
            lyraImage.sprite = isConfirmSelected ? lyraConfirmSprite : lyraCancelSprite;
        }
    }

    void CloseAll()
    {
        isPopUpActive = false;
        kiroPopUpObj.SetActive(false);
        lyraPopUpObj.SetActive(false);
        blurPanel.SetActive(false);

        if (activeCharacter == "Kiro") kiroButton.Select();
        else lyraButton.Select();
    }
}