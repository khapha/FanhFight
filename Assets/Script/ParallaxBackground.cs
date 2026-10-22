using UnityEngine;

// Nền nhiều lớp trôi theo camera: lớp càng xa trôi càng chậm
[ExecuteAlways]
public class ParallaxBackground : MonoBehaviour
{
    public Sprite[] layers;            // từ xa tới gần
    public Color tint = Color.white;
    public float farFactor = 0.95f;    // lớp xa nhất gần như đứng yên so với camera
    public float nearFactor = 0.4f;

    Transform[] objs;
    float[] widths;
    Camera cam;

    void OnEnable() { Build(); }

    void Build()
    {
        cam = GetComponent<Camera>();
        Transform old = transform.Find("Parallax");
        if (old != null) DestroyImmediate(old.gameObject);
        if (layers == null || layers.Length == 0 || cam == null) return;

        GameObject root = new GameObject("Parallax");
        root.hideFlags = HideFlags.DontSave;
        root.transform.SetParent(transform, false);
        objs = new Transform[layers.Length];
        widths = new float[layers.Length];
        float viewH = cam.orthographicSize * 2f;
        for (int i = 0; i < layers.Length; i++)
        {
            GameObject go = new GameObject(layers[i].name);
            go.hideFlags = HideFlags.DontSave;
            go.transform.SetParent(root.transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = layers[i];
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.color = tint;
            sr.sortingOrder = -100 + i;
            // phóng cho ảnh cao hơn khung nhìn một chút
            float scale = viewH * 1.15f / layers[i].bounds.size.y;
            go.transform.localScale = new Vector3(scale, scale, 1);
            widths[i] = layers[i].bounds.size.x * scale;
            sr.size = new Vector2(layers[i].bounds.size.x * 4, layers[i].bounds.size.y);
            objs[i] = go.transform;
        }
        LateUpdate();
    }

    void LateUpdate()
    {
        if (objs == null || cam == null) return;
        Vector3 c = transform.position;
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] == null) continue;
            float t = objs.Length == 1 ? 0 : (float)i / (objs.Length - 1);
            float factor = Mathf.Lerp(farFactor, nearFactor, t);
            // lớp trôi chậm hơn camera, cuộn vòng theo chiều rộng ảnh
            float offset = Mathf.Repeat(c.x * (1 - factor), widths[i]);
            objs[i].position = new Vector3(c.x - offset, c.y, c.z + 100f - i);
        }
    }
}
