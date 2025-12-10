using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnPlayButton()
    {
        Debug.Log("Oyna tuþuna basýldý"); // test için
        SceneManager.LoadScene("SaveSlots");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
