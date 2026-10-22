using UnityEngine;
using UnityEngine.SceneManagement;

// One source of truth for gameplay input, pause and respawn transitions.
public static class GameSession
{
    public enum Phase { Playing, Respawning, Paused, Finished }
    public static Phase State { get; private set; } = Phase.Playing;
    public static bool IsPlaying => State == Phase.Playing;
    public static int SceneVersion { get; private set; }
    static Phase resumeState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Reset();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Reset(); }

    static void Reset()
    {
        SceneVersion++;
        State = resumeState = Phase.Playing;
        Time.timeScale = 1;
    }

    public static void Pause()
    {
        if (State != Phase.Playing && State != Phase.Respawning) return;
        resumeState = State;
        State = Phase.Paused;
        Time.timeScale = 0;
    }

    public static void Resume()
    {
        if (State != Phase.Paused) return;
        State = resumeState;
        Time.timeScale = 1;
    }

    public static void BeginRespawn()
    {
        if (State != Phase.Playing) return;
        State = Phase.Respawning;
        Time.timeScale = 1;
    }

    public static void EndRespawn()
    {
        if (State == Phase.Respawning) State = Phase.Playing;
    }

    public static void Finish()
    {
        State = Phase.Finished;
        Time.timeScale = 0;
    }

    public static void LoadScene(int index)
    {
        Reset();
        SceneManager.LoadScene(index);
    }
}
