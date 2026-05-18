using UnityEngine;
using System.Collections;

public partial class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject blurPanel;
    public GameObject choicePopUp;
    public GameObject movementPopUp;
    
    [Header("Text Elements")]
    public GameObject popupTutorialText;
    public GameObject learnHowToText;

    public void ShowTutorial()
    {
        StartCoroutine(TutorialSequence());
    }

    IEnumerator TutorialSequence()
    {
        // 1. Aktifkan semua elemen dasar
        blurPanel.SetActive(true);
        choicePopUp.SetActive(true);
        movementPopUp.SetActive(true);
        
        // 2. Aktifkan teks dan picu script Typewriter
        popupTutorialText.SetActive(true);
        learnHowToText.SetActive(true);

        // Opsional: Jika script Typewriter kamu butuh dipicu manual, 
        // kamu bisa memanggil fungsinya di sini, contoh:
        // popupTutorialText.GetComponent<Typewriter>().StartTyping();

        // 3. Tunggu selama 10 detik
        yield return new WaitForSeconds(10f);

        // 4. Hilangkan semua elemen otomatis
        blurPanel.SetActive(false);
        choicePopUp.SetActive(false);
        movementPopUp.SetActive(false);
        popupTutorialText.SetActive(false);
        learnHowToText.SetActive(false);
    }
}