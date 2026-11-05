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
        public string tag;
        public int DamageCalls;
        public int LastDamage;
        public readonly Dictionary<System.Type, object> Components = new();
        public Transform transform = new();
        public T GetComponent<T>() where T : class => Components.TryGetValue(typeof(T), out object c) ? c as T : null;
        public static GameObject FindGameObjectWithTag(string tag) => new();
    }
    public class Transform
    {
        public Vector3 localScale = new Vector3 { x = 1, y = 1, z = 1 };
        public Vector3 position, localPosition;
        public Vector3 lossyScale => localScale;
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 one => new(1, 1, 1);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public class Rigidbody2D { public GameObject gameObject = new(); }
    public class Collider2D
    {
        public Collider2D[] TestOverlaps = System.Array.Empty<Collider2D>();
        public bool enabled = true;
        public GameObject gameObject = new();
        public Rigidbody2D attachedRigidbody;
        public Transform transform => gameObject.transform;
        public T GetComponent<T>() where T : class => gameObject.GetComponent<T>();
        public bool CompareTag(string tag) => gameObject.tag == tag;
        public int OverlapCollider(ContactFilter2D filter, Collider2D[] results)
        {
            int count = System.Math.Min(results.Length, TestOverlaps.Length);
            System.Array.Copy(TestOverlaps, results, count);
            return count;
        }
        public void SendMessageUpwards(string method, int value)
        {
            GameObject receiver = attachedRigidbody?.gameObject ?? gameObject;
            receiver.DamageCalls++;
            receiver.LastDamage = value;
        }
    }
    public class BoxCollider2D : Collider2D { public Vector2 offset, size; }
    public struct ContactFilter2D { public bool useTriggers; }
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
        public static Vector2 zero => new(0, 0);
        public static implicit operator Vector2(Vector3 value) => new(value.x, value.y);
    }
    public static class Mathf
    {
        public static int Clamp(int value, int min, int max) => System.Math.Clamp(value, min, max);
        public static float Clamp(float value, float min, float max) => System.Math.Clamp(value, min, max);
        public static int Max(int a, int b) => System.Math.Max(a, b);
        public static int Min(int a, int b) => System.Math.Min(a, b);
        public static float Max(float a, float b) => System.Math.Max(a, b);
        public static float Abs(float a) => System.Math.Abs(a);
    }
    public static class PlayerPrefs
    {
        static readonly Dictionary<string, int> values = new();
        public static int GetInt(string key, int fallback = 0) => values.TryGetValue(key, out int value) ? value : fallback;
        public static void SetInt(string key, int value) => values[key] = value;
        public static void DeleteKey(string key) => values.Remove(key);
        public static bool HasKey(string key) => values.ContainsKey(key);
        public static void Save() { }
        public static void ClearTestData() => values.Clear();
    }
}

public class Player
{
    public bool Alive = true;
    public int ourHealth = 4;
    public bool CanAct => Alive && GameSession.IsPlaying;
}
public class RoachStatus
{
    public int Hits, LastDamage;
    public bool LastSpray;
    public UnityEngine.Vector2 LastSource;
    public void ApplyWeaponHit(int damage, bool spray, UnityEngine.Vector2 source)
    {
        Hits++;
        LastDamage = damage;
        LastSpray = spray;
        LastSource = source;
    }
}
public class SoundManager { }
public static class GameHUD
{
    public static int LevelNumber(string name) => AdventureProgress.LevelNumber(name);
    public static void Toast(string text) { }
}
public static class GameInput
{
    public sealed class TestAction
    {
        public bool Pressed;
        public bool WasPressedThisFrame() => Pressed;
        public bool IsPressed() => Pressed;
    }
    public static TestAction Attack = new();
}
public static class Fx
{
    public static void HitStop() { }
    public static int Hits;
    public static void Hit() { Hits++; }
    public static int Sprays;
    public static UnityEngine.Vector3 LastSprayPosition;
    public static void Spray(UnityEngine.Vector3 position, bool right) { Sprays++; LastSprayPosition = position; }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public string name; }
    public enum LoadSceneMode { Single }
    public static class SceneManager
    {
        public static event System.Action<Scene, LoadSceneMode> sceneLoaded;
        public static int LoadedIndex = -1;
        public static void LoadScene(int index)
        {
            LoadedIndex = index;
            sceneLoaded?.Invoke(new Scene { name = index >= 0 && index < SceneUtility.Paths.Count
                ? System.IO.Path.GetFileNameWithoutExtension(SceneUtility.Paths[index]) : "" }, LoadSceneMode.Single);
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
