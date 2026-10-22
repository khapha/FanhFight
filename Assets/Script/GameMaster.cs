using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public int points = 0;
    public int highscore = 0;
    public int EntryPoints { get; private set; }
    int displayedPoints = -1;

    public TMP_Text pointText;
    public TMP_Text Hightext;
    public TMP_Text InputText;

    
    // Start is called before the first frame update
    void Start()
    {
        Hightext.text = ("HighScore: " + PlayerPrefs.GetInt("highscore"));
        highscore = PlayerPrefs.GetInt("highscore", 0);

        if (PlayerPrefs.HasKey("points"))
        {
            Scene ActiveScreen = SceneManager.GetActiveScene();
            // màn đầu tiên luôn bắt đầu từ 0 điểm
            if (ActiveScreen.name == "Scene 1")
            {
                PlayerPrefs.DeleteKey("points");
            }
            else
                points = PlayerPrefs.GetInt("points");
        }
        EntryPoints = points;
    }

    public void SaveHighscore()
    {
        if (points > PlayerPrefs.GetInt("highscore", 0))
        {
            PlayerPrefs.SetInt("highscore", points);
            PlayerPrefs.Save();
        }
        highscore = PlayerPrefs.GetInt("highscore", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (displayedPoints == points || pointText == null) return;
        displayedPoints = points;
        pointText.text = "Points: " + points;
    }
}
