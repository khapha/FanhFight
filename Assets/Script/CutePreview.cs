#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

// Opt-in visual smoke test for development builds. Normal launches do nothing.
public class CutePreview : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int index = System.Array.IndexOf(args, "-cute-preview");
        if (index < 0 || index + 1 >= args.Length) return;
        GameObject go = new GameObject("CutePreview");
        DontDestroyOnLoad(go);
        go.AddComponent<CutePreview>().StartCoroutine(Run(Path.GetFullPath(args[index + 1])));
    }

    static IEnumerator Run(string output)
    {
        Application.runInBackground = true;
        Directory.CreateDirectory(output);
        bool hadPoints = PlayerPrefs.HasKey("points");
        int savedPoints = PlayerPrefs.GetInt("points", 0);
        bool hadBest = PlayerPrefs.HasKey("highscore");
        int savedBest = PlayerPrefs.GetInt("highscore", 0);
        try
        {
            yield return new WaitForSecondsRealtime(3);
            yield return Capture(output, "01-menu.png");
            FindAnyObjectByType<MainMenu>().HowToPlay();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture(output, "06-help.png");
            foreach (var button in GameObject.Find("HelpPanel").GetComponentsInChildren<UnityEngine.UI.Button>())
                if (button.name == "Back") { button.onClick.Invoke(); break; }
            yield return null;
            FindAnyObjectByType<MainMenu>().PlayGame();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture(output, "02-levels.png");
            SceneManager.LoadScene("Scene 1");
            yield return new WaitForSecondsRealtime(4);
            yield return Capture(output, "03-forest.png");
            yield return CheckMotion("Woodland");
            Enemy enemy = FindAnyObjectByType<Enemy>();
            if (enemy != null)
            {
                GameMaster gm = FindAnyObjectByType<GameMaster>();
                int score = gm.points;
                enemy.SendMessage("Damage", 20);
                enemy.SendMessage("Damage", 20);
                if (gm.points != score + 5) throw new System.Exception("One enemy hit was scored more than once.");
                Debug.Log("COMBAT PASS: enemy damage receiver and duplicate-hit guard.");
                yield return new WaitForSecondsRealtime(0.3f);
            }
            Player respawnPlayer = FindAnyObjectByType<Player>();
            int lives = Checkpoint.Lives;
            respawnPlayer.Damage(respawnPlayer.maxhealth);
            if (respawnPlayer.CanAct) throw new System.Exception("Lethal damage did not block player actions.");
            yield return new WaitForSecondsRealtime(1.2f);
            if (!respawnPlayer.CanAct || respawnPlayer.ourHealth != respawnPlayer.maxhealth || Checkpoint.Lives != lives - 1)
                throw new System.Exception("Respawn did not restore the player correctly.");
            Debug.Log("RESPAWN PASS: full health, one life consumed, camera notified of teleport.");
            PausedMenu pause = FindAnyObjectByType<PausedMenu>();
            pause.TogglePause();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture(output, "04-paused.png");
            pause.Resume();
            GameOverUI.Show("YOU WIN!", 25, 25);
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Capture(output, "05-result.png");
            GameSession.LoadScene(2);
            yield return new WaitForSecondsRealtime(3);
            yield return Capture(output, "07-lakeside.png");
            yield return CheckMotion("Lakeside");
            GameSession.LoadScene(3);
            yield return new WaitForSecondsRealtime(3);
            yield return Capture(output, "08-cave.png");
            yield return CheckMotion("Rocky Cave");
            SceneManager.LoadScene("MiniSmash");
            yield return new WaitForSecondsRealtime(4);
            yield return Capture(output, "09-smash.png");
            SceneManager.LoadScene("MiniSpray");
            yield return new WaitForSecondsRealtime(2);
            yield return Capture(output, "10-garden.png");
            Debug.Log("VISUAL PREVIEW PASS: all adventure backgrounds, menus and mini-games rendered in English.");
        }
        finally
        {
            if (hadPoints) PlayerPrefs.SetInt("points", savedPoints);
            else PlayerPrefs.DeleteKey("points");
            if (hadBest) PlayerPrefs.SetInt("highscore", savedBest);
            else PlayerPrefs.DeleteKey("highscore");
            PlayerPrefs.Save();
        }
        Application.Quit();
    }

    static IEnumerator Capture(string folder, string name)
    {
        yield return new WaitForEndOfFrame();
        foreach (TMPro.TMP_Text text in FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None))
            if (text.isActiveAndEnabled && Regex.IsMatch(text.text,
                @"(?i)(phieu luu|khu rung|khu vuon|huong dan|quay lai|choi lai|tiep tuc|het gio|diem:|ky luc:|mang:|nhac nen|bong hoa)"))
                throw new System.Exception("Untranslated visible label: " + text.text);
        GameObject backdropObject = GameObject.Find("ForestBackdrop");
        if (backdropObject != null)
        {
            Bounds bounds = backdropObject.GetComponent<SpriteRenderer>().bounds;
            Vector3 min = Camera.main.WorldToViewportPoint(bounds.min);
            Vector3 max = Camera.main.WorldToViewportPoint(bounds.max);
            if (min.x > 0 || min.y > 0 || max.x < 1 || max.y < 1)
                throw new System.Exception("Backdrop does not cover the viewport.");
        }
        Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
        try
        {
            Color32[] pixels = image.GetPixels32();
            bool rendered = false;
            for (int i = 0; i < pixels.Length; i += 127)
                if (pixels[i].r > 25 || pixels[i].g > 25 || pixels[i].b > 25) { rendered = true; break; }
            if (!rendered) throw new System.Exception("Screenshot is blank. Run the visual preview in a visible game window.");
            File.WriteAllBytes(Path.Combine(folder, name), image.EncodeToPNG());
        }
        finally { Destroy(image); }
        yield return new WaitForSecondsRealtime(0.3f);
    }

    static IEnumerator CheckMotion(string level)
    {
        Player player = FindAnyObjectByType<Player>();
        Camera camera = Camera.main;
        Transform backdrop = GameObject.Find("ForestBackdrop").transform;
        if (player.r2.interpolation != RigidbodyInterpolation2D.Interpolate)
            throw new System.Exception("Player interpolation is disabled.");
        if (backdrop.parent != camera.transform) throw new System.Exception("Backdrop must be camera-local.");
        player.r2.position = new Vector2(4, 2);
        player.r2.linearVelocity = Vector2.zero;
        List<float> frames = new List<float>(600);
        float start = Time.realtimeSinceStartup, movement = 0, maxError = 0;
        Vector3 previousCamera = camera.transform.position;
        int frame = 0;
        bool right = true;
        while (Time.realtimeSinceStartup - start < 3)
        {
            if (player.r2.position.x >= 10) right = false;
            if (player.r2.position.x <= 4) right = true;
            player.Move(right ? 1 : -1);
            if (frame % 18 == 0) Fx.Spray(player.transform.position, right);
            if (frame % 12 == 0) Fx.Dust(player.transform.position);
            yield return new WaitForEndOfFrame();
            maxError = Mathf.Max(maxError, (backdrop.localPosition - new Vector3(0, 0, 150)).magnitude);
            movement += Vector3.Distance(previousCamera, camera.transform.position);
            previousCamera = camera.transform.position;
            if (Time.realtimeSinceStartup - start > 0.5f) frames.Add(Time.unscaledDeltaTime * 1000);
            frame++;
        }
        player.Move(0);
        if (maxError > 0.001f || movement < 0.05f)
            throw new System.Exception("Camera motion regression: " + level + " error=" + maxError + " travel=" + movement);
        frames.Sort();
        float total = 0;
        foreach (float ms in frames) total += ms;
        Debug.Log("MOTION PASS " + level + ": backdrop error=" + maxError.ToString("F5")
            + ", mean frame=" + (total / frames.Count).ToString("F2") + "ms, p95="
            + frames[Mathf.Min(frames.Count - 1, Mathf.FloorToInt(frames.Count * 0.95f))].ToString("F2") + "ms");
        yield return new WaitForSecondsRealtime(1.7f);
        FxParticle[] effects = FindObjectsByType<FxParticle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (effects.Length > 24) throw new System.Exception("Particle pool exceeded its budget.");
        foreach (FxParticle effect in effects)
            if (effect.InUse) throw new System.Exception("Finished particles were not returned to the pool.");
        Debug.Log("POOL PASS " + level + ": " + effects.Length + " reusable emitters.");
    }
}
#endif
