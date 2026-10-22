// Test doubles for the small Unity API surface used by the linked production files.
// These tests do not simulate Unity rendering, physics, event routing or disk persistence.
namespace UnityEngine
{
    public class MonoBehaviour
    {
        public GameObject gameObject = new();
        public Transform transform => gameObject.transform;
        public T GetComponent<T>() where T : class => gameObject.GetComponent<T>();
    }
    public class GameObject
    {
        public readonly Dictionary<System.Type, object> Components = new();
        public Transform transform = new();
        public T GetComponent<T>() where T : class => Components.TryGetValue(typeof(T), out object c) ? c as T : null;
        public static GameObject FindGameObjectWithTag(string tag) => new();
    }
    public class Transform
    {
        public Vector3 localScale = new Vector3 { x = 1, y = 1, z = 1 };
        public Vector3 position;
    }
    public struct Vector3 { public float x, y, z; }
    public class Collider2D { public bool enabled = true; }
    public class Animator
    {
        public bool Attacking;
        public void SetBool(string name, bool value) { Attacking = value; }
    }
    public enum RuntimeInitializeLoadType { BeforeSceneLoad }
    public sealed class RuntimeInitializeOnLoadMethodAttribute : System.Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { }
    }
    public static class Time { public static float timeScale = 1, deltaTime = 0.1f; }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }
    public static class Mathf
    {
        public static int Clamp(int value, int min, int max) => System.Math.Clamp(value, min, max);
        public static float Clamp(float value, float min, float max) => System.Math.Clamp(value, min, max);
        public static int Max(int a, int b) => System.Math.Max(a, b);
        public static int Min(int a, int b) => System.Math.Min(a, b);
    }
    public static class PlayerPrefs
    {
        static readonly Dictionary<string, int> values = new();
        public static int GetInt(string key, int fallback = 0) => values.TryGetValue(key, out int value) ? value : fallback;
        public static void SetInt(string key, int value) => values[key] = value;
        public static void DeleteKey(string key) => values.Remove(key);
        public static void Save() { }
        public static void ClearTestData() => values.Clear();
    }
}

public class Player
{
    public bool Alive = true;
    public bool CanAct => Alive && GameSession.IsPlaying;
}
public class SoundManager { }
public static class GameInput
{
    public sealed class TestAction
    {
        public bool Pressed;
        public bool WasPressedThisFrame() => Pressed;
    }
    public static TestAction Attack = new();
}
public static class Fx
{
    public static int Sprays;
    public static void Spray(UnityEngine.Vector3 position, bool right) { Sprays++; }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { }
    public enum LoadSceneMode { Single }
    public static class SceneManager
    {
        public static event System.Action<Scene, LoadSceneMode> sceneLoaded;
        public static int LoadedIndex = -1;
        public static void LoadScene(int index)
        {
            LoadedIndex = index;
            sceneLoaded?.Invoke(new Scene(), LoadSceneMode.Single);
        }
    }
    public static class SceneUtility
    {
        public static readonly List<string> Paths = new();
        public static int GetBuildIndexByScenePath(string path) => Paths.IndexOf(path);
    }
}

namespace UnityEngine.EventSystems
{
    public class PointerEventData { public int pointerId; }
    public interface IPointerDownHandler { void OnPointerDown(PointerEventData data); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData data); }
    public interface IPointerExitHandler { void OnPointerExit(PointerEventData data); }
}
