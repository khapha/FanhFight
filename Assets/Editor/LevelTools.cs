using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Công cụ xem bố cục màn chơi: ghi danh sách object và chụp ảnh toàn màn
public static class LevelTools
{
    const string OutDir = "LevelDump";

    public static void Dump()
    {
        Directory.CreateDirectory(OutDir);
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(s.path);
            string name = Path.GetFileNameWithoutExtension(s.path);
            StringBuilder sb = new StringBuilder();
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                Write(sb, root.transform, 0);
            foreach (Tilemap tm in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            {
                tm.CompressBounds();
                sb.AppendLine("TILEMAP " + tm.name + " cellBounds=" + tm.cellBounds + " world=" + tm.localBounds + " tiles=" + tm.GetUsedTilesCount());
            }
            File.WriteAllText(Path.Combine(OutDir, name + ".txt"), sb.ToString());
            Render(Path.Combine(OutDir, name + ".png"));
        }
    }

    static void Write(StringBuilder sb, Transform t, int depth)
    {
        if (depth > 3) return;
        List<string> comps = new List<string>();
        foreach (Component c in t.GetComponents<Component>())
            if (c != null && !(c is Transform)) comps.Add(c.GetType().Name);
        sb.Append(new string(' ', depth * 2));
        sb.AppendLine(t.name + " [" + t.tag + "] pos=" + t.position.ToString("F1") + " scale=" + t.lossyScale.ToString("F1") + " active=" + t.gameObject.activeInHierarchy + " {" + string.Join(",", comps) + "}" + (t.childCount > 0 ? " children=" + t.childCount : ""));
        foreach (Transform c in t) Write(sb, c, depth + 1);
    }

    // chụp góc nhìn của camera game tại vài điểm dọc màn (xem nền, UI không có)
    public static void Shots()
    {
        Directory.CreateDirectory(OutDir);
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(s.path);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Camera cam = Camera.main;
            if (player == null || cam == null) continue;
            string name = Path.GetFileNameWithoutExtension(s.path);
            CameraFollow cf = cam.GetComponent<CameraFollow>();
            float[] xs = { player.transform.position.x, (cf.minipos.x + cf.maxpos.x) / 2, cf.maxpos.x };
            for (int i = 0; i < xs.Length; i++)
            {
                float y = i == 0 ? player.transform.position.y + 1 : cam.transform.position.y;
                cam.transform.position = new Vector3(Mathf.Clamp(xs[i], cf.minipos.x, cf.maxpos.x), Mathf.Clamp(y, cf.minipos.y, cf.maxpos.y), cam.transform.position.z);
                ParallaxBackground pb = cam.GetComponent<ParallaxBackground>();
                if (pb != null) pb.SendMessage("LateUpdate");
                // ảnh chụp không có UI
                foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) c.enabled = false;
                RenderTexture rt = new RenderTexture(1280, 720, 24);
                cam.targetTexture = rt;
                cam.aspect = 16f / 9f;
                cam.Render();
                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                File.WriteAllBytes(Path.Combine(OutDir, name + "_view" + i + ".png"), tex.EncodeToPNG());
                cam.targetTexture = null;
                RenderTexture.active = null;
            }
        }
    }

    // chụp toàn bộ màn chơi (theo bao của mọi renderer) ra PNG
    public static void Render(string path)
    {
        Bounds b = new Bounds();
        bool first = true;
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r is ParticleSystemRenderer || r.bounds.size.x > 500) continue;
            if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
        }
        GameObject go = new GameObject("RenderCam");
        Camera cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
        int w = 4096;
        int h = Mathf.Clamp((int)(w * b.size.y / Mathf.Max(b.size.x, 1)), 256, 4096);
        cam.aspect = (float)w / h;
        cam.orthographicSize = Mathf.Max(b.size.y / 2, b.size.x / 2 / cam.aspect);
        go.transform.position = new Vector3(b.center.x, b.center.y, -50);
        RenderTexture rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        RenderTexture.active = null;
        Object.DestroyImmediate(go);
        Debug.Log("LevelTools: rendered " + path + " bounds=" + b);
    }
}
