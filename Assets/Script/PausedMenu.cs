using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PausedMenu : MonoBehaviour
{
    public GameObject pauseUI;
    public bool pause = false;
    // Start is called before the first frame update
    void Start()
    {
        pauseUI.SetActive(false);
        Canvas canvas = MiniUI.MakeCanvas("CutePauseMenu");
        canvas.sortingOrder = 85;
        MiniUI.Panel(canvas.transform, new Color(0.12f, 0.28f, 0.23f, 0.4f));
        Transform safe = SafeArea.Create(canvas.transform);
        Transform card = CuteTheme.Card(safe, Vector2.one * 0.5f, Vector2.zero, new Vector2(470, 400)).transform;
        var title = MiniUI.MakeText(card, "Take a little break!", 32, Vector2.one * 0.5f, new Vector2(0, 125));
        CuteTheme.Text(title, 32);
        MiniUI.MakeButton(card, "Resume", Vector2.one * 0.5f, new Vector2(0, 40), new Vector2(330, 58), Resume);
        MiniUI.MakeButton(card, "Retry", Vector2.one * 0.5f, new Vector2(0, -40), new Vector2(330, 58), Restart);
        MiniUI.MakeButton(card, "Main Menu", Vector2.one * 0.5f, new Vector2(0, -120), new Vector2(330, 58), Quit);
        pauseUI = canvas.gameObject;
        pauseUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameOverUI.IsShowing) return;
        if (GameInput.Pause.WasPressedThisFrame()) TogglePause();
    }
    public void TogglePause()
    {
        if (GameOverUI.IsShowing) return;
        if (GameSession.State == GameSession.Phase.Paused) Resume();
        else
        {
            GameSession.Pause();
            foreach (PlayerAttack attack in FindObjectsByType<PlayerAttack>(FindObjectsSortMode.None))
                attack.CancelAttack();
            pause = true;
            pauseUI.SetActive(true);
            MiniUI.Select(pauseUI.GetComponentInChildren<Button>(true));
        }
    }
    public void Resume()
    {
        GameSession.Resume();
        pause = false;
        pauseUI.SetActive(false);
        ClearSelection();
    }
    // bỏ chọn nút, để phím Space (chém) không bấm nhầm Restart/Quit khi đang chơi
    void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
    public void Restart()
    {
        GameSession.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // về menu chính thay vì thoát hẳn game
    public void Quit()
    {
        GameSession.LoadScene(0);
    }
}
