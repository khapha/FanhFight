using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Hiện tên màn khi bắt đầu, cho người chơi biết đang ở đâu trên hành trình
public class LevelIntro : MonoBehaviour
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            // index 0 là menu, các màn bắt đầu từ 1
            int level = GameHUD.LevelNumber(scene.name);
            if (level < 1 || level > AdventureProgress.Count) return;
            GameObject go = new GameObject("LevelIntro");
            go.AddComponent<LevelIntro>().Show("Level " + level + "/" + AdventureProgress.Count, AdventureProgress.Titles[level - 1]);
        };
    }

    void Show(string top, string title)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        UnityEngine.UI.CanvasScaler scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        CanvasGroup group = gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        CuteTheme.Card(transform, Vector2.one * 0.5f, new Vector2(0, 70), new Vector2(480, 175));
        AddText(top, 36, 90);
        AddText(title, 72, 20);
        StartCoroutine(Fade(group));
    }

    void AddText(string s, int size, float y)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(transform, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        CuteTheme.Text(t, size);
        t.rectTransform.sizeDelta = new Vector2(1200, size * 1.5f);
        t.rectTransform.anchoredPosition = new Vector2(0, y);
    }

    IEnumerator Fade(CanvasGroup group)
    {
        yield return new WaitForSecondsRealtime(2f);
        for (float a = 1; a > 0; a -= Time.unscaledDeltaTime)
        {
            group.alpha = a;
            yield return null;
        }
        Destroy(gameObject);
    }
}
