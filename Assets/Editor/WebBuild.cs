using System.Linq;
using UnityEditor;
using UnityEngine;

// Build bản web (WebGL) vào thư mục WebBuild, chạy được trên GitHub Pages
public static class WebBuild
{
    public static void Build()
    {
        // GitHub Pages không gửi header nén, nên để trình duyệt tự giải nén
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;

        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var report = BuildPipeline.BuildPlayer(scenes, "WebBuild", BuildTarget.WebGL, BuildOptions.None);
        Debug.Log("WebBuild: " + report.summary.result + " size " + report.summary.totalSize);
    }
}
