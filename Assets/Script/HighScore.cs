using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HighScore : MonoBehaviour
{
    public TMP_Text Hightext;

    // Start is called before the first frame update
    void Start()
    {
        Hightext.text = ("HighScore: " + PlayerPrefs.GetInt("highscore"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
