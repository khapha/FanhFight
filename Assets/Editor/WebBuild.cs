using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
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
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("WebGL build failed: " + report.summary.result);
        PreparePage();
        Debug.Log("WebBuild: " + report.summary.result + " size " + report.summary.totalSize);
    }

    // Keep desktop panels and phones usable without scrolling around a fixed-size canvas.
    public static void PreparePage()
    {
        const string page = "WebBuild/index.html";
        string html = File.ReadAllText(page);
        if (html.Contains("id=\"responsive-game\"")) return;
        html = html.Replace("</head>", @"
    <meta name=""viewport"" content=""width=device-width, initial-scale=1, viewport-fit=cover"">
    <style id=""responsive-game"">
      html, body { margin: 0; width: 100%; height: 100%; overflow: hidden; background: #243e3a; }
      #unity-container.unity-desktop, #unity-container.unity-mobile {
        position: absolute; top: 0; left: 0; transform: none; width: 100%; height: 100%;
      }
      #unity-fullscreen-container { width: 100%; height: calc(100% - 38px); }
      #unity-canvas { display: block; width: 100% !important; height: 100% !important; }
      #unity-footer { width: 100%; height: 38px; background: #fff; }
      .unity-mobile #unity-fullscreen-container, #unity-fullscreen-container:fullscreen { height: 100%; }
    </style>
  </head>");
        File.WriteAllText(page, html);
        File.WriteAllText("WebBuild/.nojekyll", "");
    }
}
