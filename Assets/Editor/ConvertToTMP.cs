using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Đổi UI.Text sang TextMeshPro trong mọi scene của build.
// Bước 1 (Convert): ghi lại các field script đang trỏ tới Text, rồi thay Text bằng TextMeshProUGUI.
// Bước 2 (Relink, sau khi đổi kiểu field sang TMP_Text): gán lại các field đó.
public static class ConvertToTMP
{
    const string RefFile = "Assets/Editor/tmp_refs.txt";

    public static void Convert()
    {
        List<string> refs = new List<string>();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            var scene = EditorSceneManager.OpenScene(s.path);
            foreach (MonoBehaviour mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (mb == null || mb is Text) continue;
                SerializedObject so = new SerializedObject(mb);
                SerializedProperty p = so.GetIterator();
                while (p.Next(true))
                {
                    if (p.propertyType == SerializedPropertyType.ObjectReference && p.objectReferenceValue is Text t)
                        refs.Add(s.path + "|" + GlobalObjectId.GetGlobalObjectIdSlow(mb) + "|" + p.propertyPath + "|" + GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject));
                }
            }

            int count = 0;
            foreach (Text t in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                GameObject go = t.gameObject;
                string text = t.text;
                int size = t.fontSize;
                Color color = t.color;
                TextAnchor anchor = t.alignment;
                bool raycast = t.raycastTarget;
                bool best = t.resizeTextForBestFit;
                int min = t.resizeTextMinSize, max = t.resizeTextMaxSize;
                FontStyle style = t.fontStyle;
                // Outline/Shadow chỉ dùng được cho UI.Text
                foreach (var e in go.GetComponents<Shadow>()) Object.DestroyImmediate(e);
                Object.DestroyImmediate(t);

                TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.fontSize = size;
                tmp.color = color;
                tmp.raycastTarget = raycast;
                tmp.alignment = Align(anchor);
                tmp.textWrappingMode = TextWrappingModes.Normal;
                if (style == FontStyle.Bold || style == FontStyle.BoldAndItalic) tmp.fontStyle |= FontStyles.Bold;
                if (style == FontStyle.Italic || style == FontStyle.BoldAndItalic) tmp.fontStyle |= FontStyles.Italic;
                if (best)
                {
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = min;
                    tmp.fontSizeMax = max;
                }
                count++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("ConvertToTMP: " + s.path + " converted " + count + " Text");
        }
        File.WriteAllLines(RefFile, refs);
        Debug.Log("ConvertToTMP: saved " + refs.Count + " references");
    }

    public static void Relink()
    {
        string[] lines = File.ReadAllLines(RefFile);
        string open = null;
        foreach (string line in lines)
        {
            string[] f = line.Split('|');
            if (open != f[0])
            {
                if (open != null) EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.OpenScene(f[0]);
                open = f[0];
            }
            GlobalObjectId.TryParse(f[1], out GlobalObjectId compId);
            GlobalObjectId.TryParse(f[3], out GlobalObjectId goId);
            Object comp = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(compId);
            GameObject go = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(goId) as GameObject;
            if (comp == null || go == null)
            {
                Debug.LogWarning("ConvertToTMP: cannot resolve " + line);
                continue;
            }
            SerializedObject so = new SerializedObject(comp);
            so.FindProperty(f[2]).objectReferenceValue = go.GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(go.scene);
            Debug.Log("ConvertToTMP: relinked " + comp.GetType().Name + "." + f[2] + " -> " + go.name);
        }
        if (open != null) EditorSceneManager.SaveOpenScenes();
    }

    static TextAlignmentOptions Align(TextAnchor a)
    {
        switch (a)
        {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            default: return TextAlignmentOptions.BottomRight;
        }
    }
}
