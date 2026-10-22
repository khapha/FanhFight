using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public GameMaster gm;
    private bool playerInside = false;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("gamemaster").GetComponent<GameMaster>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInside && GameInput.Interact.WasPressedThisFrame())
            EnterDoor();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            savescore();
            gm.InputText.text = ("Press E / Y to enter");
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInside = true;
    }
    void EnterDoor()
    {
        savescore();
        gm.SaveHighscore();
        // sang màn kế tiếp trong Build Settings, hết màn thì thắng
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            GameOverUI.Show("YOU WIN!", gm.points, gm.highscore);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            gm.InputText.text = ("");
        }
    }
    void savescore()
    {
        PlayerPrefs.SetInt("points", gm.points);
    }
}
