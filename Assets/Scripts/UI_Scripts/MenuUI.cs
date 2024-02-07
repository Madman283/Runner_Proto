using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Assets/Scenes/Titlescreen.unity");
    }
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Assets/Scenes/Runner_Level_01.unity");
    }
    public void ArtGalleryScene()
    {
        SceneManager.LoadScene("Assets/Scenes/ArtGallery.unity");
    }
    public void CreditsScene()
    {
        SceneManager.LoadScene("Assets/Scenes/Credits.unity");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
