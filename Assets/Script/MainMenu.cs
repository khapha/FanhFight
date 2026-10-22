using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    GameObject helpPanel;
    TextMeshProUGUI musicText, sfxText;
    Button playButton, helpButton;

    void Start()
    {
        // menu chọn game: nhân bản nút Quit thành các nút mới, xếp thành cột dưới nút Play
        Button quit = null;
        foreach (Button b in GetComponentsInChildren<Button>(true))
        {
            if (b.name == "Play") playButton = b;
            if (b.name == "Quit") quit = b;
        }
        if (quit != null)
        {
            RectTransform p = (RectTransform)playButton.transform;
            float step = p.anchoredPosition.y - ((RectTransform)quit.transform).anchoredPosition.y;
            playButton.GetComponentInChildren<TMP_Text>().text = "Phieu luu";
            Button smash = CloneButton(quit, "Dap dep", () => SceneManager.LoadScene("MiniSmash"));
            Button spray = CloneButton(quit, "Khu vuon hoa", () => SceneManager.LoadScene("MiniSpray"));
            helpButton = CloneButton(quit, "Huong dan", HowToPlay);
            Button[] order = { playButton, smash, spray, helpButton, quit };
            for (int i = 0; i < order.Length; i++)
                ((RectTransform)order[i].transform).anchoredPosition = p.anchoredPosition - new Vector2(0, step * i);
        }
        // tiêu đề sáng, có viền cho nổi trên nền rừng
        foreach (TMP_Text t in GetComponentInParent<Canvas>().GetComponentsInChildren<TMP_Text>(true))
            if (t.text == "Fanh Fight")
            {
                t.color = new Color(1f, 0.9f, 0.4f);
                t.outlineWidth = 0.3f;
                t.outlineColor = Color.black;
            }
        BuildHelpPanel();
        Select(playButton);
    }

    void Update()
    {
        // nền rừng trôi chậm phía sau menu
        if (Camera.main != null)
            Camera.main.transform.position += new Vector3(Time.deltaTime * 1.5f, 0, 0);
        if (helpPanel != null && helpPanel.activeSelf && GameInput.Pause.WasPressedThisFrame())
            CloseHelp();
    }

    Button CloneButton(Button source, string label, UnityEngine.Events.UnityAction onClick)
    {
        Button b = Instantiate(source, source.transform.parent);
        b.name = label;
        b.onClick = new Button.ButtonClickedEvent();
        b.onClick.AddListener(onClick);
        b.GetComponentInChildren<TMP_Text>().text = label;
        return b;
    }

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
        helpPanel.SetActive(true);
        Select(helpPanel.GetComponentInChildren<Button>());
    }
    void CloseHelp()
    {
        helpPanel.SetActive(false);
        Select(helpButton);
    }

    static void Select(Component c)
    {
        if (c != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(c.gameObject);
    }

    // bảng hướng dẫn phím + chỉnh âm lượng
    void BuildHelpPanel()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        helpPanel = new GameObject("HelpPanel");
        helpPanel.transform.SetParent(canvas.transform, false);
        Image bg = helpPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.06f, 0.1f, 0.92f);
        RectTransform rt = bg.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        TMP_FontAsset font = playButton.GetComponentInChildren<TMP_Text>().font;
        Label("Huong dan", 40, new Vector2(0, 0.85f), font);
        Label("Di chuyen:  <- ->  hoac  A D\n" +
              "Nhay:  ^  hoac  W   (bam 2 lan de nhay doi)\n" +
              "Xit thuoc diet gian:  Space  hoac  J\n" +
              "Vao cua:  E      Xuong platform:  v  hoac  S\n" +
              "Tam dung:  Esc\n" +
              "Tay cam:  A nhay - X xit - Y vao cua - Start tam dung",
              22, new Vector2(0, 0.58f), font);

        musicText = Label("", 24, new Vector2(0, 0.33f), font);
        VolumeButtons(new Vector2(0, 0.33f), v => { Fx.MusicVolume += v; Refresh(); }, font);
        sfxText = Label("", 24, new Vector2(0, 0.24f), font);
        VolumeButtons(new Vector2(0, 0.24f), v => { Fx.SfxVolume += v; Fx.Coin(); Refresh(); }, font);

        Button close = MakeButton("Dong", new Vector2(0.5f, 0.1f), new Vector2(200, 50), font);
        close.onClick.AddListener(CloseHelp);
        Refresh();
        helpPanel.SetActive(false);
    }

    void Refresh()
    {
        musicText.text = "Nhac nen: " + Mathf.RoundToInt(Fx.MusicVolume * 100) + "%";
        sfxText.text = "Hieu ung: " + Mathf.RoundToInt(Fx.SfxVolume * 100) + "%";
    }

    TextMeshProUGUI Label(string s, int size, Vector2 anchorY, TMP_FontAsset font)
    {
        GameObject go = new GameObject("Label");
        go.transform.SetParent(helpPanel.transform, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.font = font;
        t.text = s;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorY.y);
        rt.sizeDelta = new Vector2(900, size * 8);
        return t;
    }

    void VolumeButtons(Vector2 anchorY, System.Action<float> change, TMP_FontAsset font)
    {
        MakeButton("-", new Vector2(0.33f, anchorY.y), new Vector2(50, 40), font).onClick.AddListener(() => change(-0.1f));
        MakeButton("+", new Vector2(0.67f, anchorY.y), new Vector2(50, 40), font).onClick.AddListener(() => change(0.1f));
    }

    // nút mới theo kiểu nút Play có sẵn
    Button MakeButton(string label, Vector2 anchor, Vector2 size, TMP_FontAsset font)
    {
        Button b = Instantiate(playButton, helpPanel.transform);
        b.name = label;
        b.onClick = new Button.ButtonClickedEvent();
        b.GetComponentInChildren<TMP_Text>().text = label;
        RectTransform rt = (RectTransform)b.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        return b;
    }
}
