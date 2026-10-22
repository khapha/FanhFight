using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    Rect lastArea;
    Vector2Int lastSize;

    public static RectTransform Create(Transform parent)
    {
        GameObject go = new GameObject("SafeArea", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<SafeArea>();
        return (RectTransform)go.transform;
    }

    void OnEnable() { Apply(); }

    void Update()
    {
        if (lastArea != Screen.safeArea || lastSize != new Vector2Int(Screen.width, Screen.height)) Apply();
    }

    void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;
        lastArea = Screen.safeArea;
        lastSize = new Vector2Int(Screen.width, Screen.height);
        RectTransform rt = (RectTransform)transform;
        rt.anchorMin = new Vector2(lastArea.xMin / Screen.width, lastArea.yMin / Screen.height);
        rt.anchorMax = new Vector2(lastArea.xMax / Screen.width, lastArea.yMax / Screen.height);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
