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
        if (GameInput.Interact.WasPressedThisFrame()) TryEnter();
    }
    public void TryEnter()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (playerInside && player != null && player.GetComponent<Player>().CanAct)
            EnterDoor();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            gm.InputText.text = Application.isMobilePlatform ? "Tap Enter to go through the door" : "Press E / Y to enter";
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
        AdventureProgress.Complete(GameHUD.LevelNumber(SceneManager.GetActiveScene().name), gm.points - gm.EntryPoints);
        // sang màn kế tiếp trong Build Settings, hết màn thì thắng
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        // chỉ sang màn kế nếu đó là màn phiêu lưu (sau màn cuối là các mini-game)
        string nextName = next < SceneManager.sceneCountInBuildSettings
            ? System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(next)) : "";
        if (GameHUD.LevelNumber(nextName) > 0)
            GameSession.LoadScene(next);
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
