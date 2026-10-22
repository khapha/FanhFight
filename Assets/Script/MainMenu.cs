using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
   public void PlayGame()
    {
        SceneManager.LoadScene(1); // Scene 1 là màn đầu tiên, index 0 là menu
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void HowToPlay()
    {

    }
}
