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
    private bool wasPaused = false;
    // Start is called before the first frame update
    void Start()
    {
        pauseUI.SetActive(false);
        foreach (TMPro.TMP_Text t in pauseUI.GetComponentsInChildren<TMPro.TMP_Text>(true))
            if (t.text == "Quit") t.text = "Menu";
    }

    // Update is called once per frame
    void Update()
    {
        if (GameOverUI.IsShowing) return;
        if (GameInput.Pause.WasPressedThisFrame())
        {
            pause = !pause;
            // chọn sẵn nút đầu tiên để điều khiển bằng bàn phím/tay cầm
            if (pause && EventSystem.current != null)
            {
                Button first = pauseUI.GetComponentInChildren<Button>(true);
                if (first != null) EventSystem.current.SetSelectedGameObject(first.gameObject);
            }
            if (!pause) ClearSelection();
        }
        // chỉ đổi timeScale khi vừa bật/tắt pause, để không phá hiệu ứng khựng hình
        if (pause != wasPaused)
        {
            pauseUI.SetActive(pause);
            Time.timeScale = pause ? 0 : 1;
            wasPaused = pause;
        }
        
    }
    public void Resume()
    {
        pause = false;
        ClearSelection();
    }
    // bỏ chọn nút, để phím Space (chém) không bấm nhầm Restart/Quit khi đang chơi
    void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // về menu chính thay vì thoát hẳn game
    public void Quit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
