using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

// Thay CameraFollow bằng Cinemachine khi scene tải: bám theo player, giới hạn khung, rung màn
public class CameraRig : MonoBehaviour
{
    static CinemachineImpulseSource impulse;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) => Setup();
    }

    static void Setup()
    {
        Camera main = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (main == null || player == null) return;

        CameraFollow oldFollow = main.GetComponent<CameraFollow>();
        if (oldFollow != null) oldFollow.enabled = false;
        if (main.GetComponent<CinemachineBrain>() == null)
            main.gameObject.AddComponent<CinemachineBrain>();

        GameObject rig = new GameObject("CM Camera");
        rig.transform.position = main.transform.position;
        CinemachineCamera cam = rig.AddComponent<CinemachineCamera>();
        cam.Lens = LensSettings.FromCamera(main);
        cam.Follow = player.transform;

        CinemachinePositionComposer composer = rig.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = Mathf.Abs(main.transform.position.z - player.transform.position.z);
        composer.Damping = new Vector3(0.3f, 0.4f, 0);
        composer.TargetOffset = new Vector3(0, 1f, 0);
        composer.Lookahead.Enabled = true;
        composer.Lookahead.Time = 0.2f;
        composer.Lookahead.IgnoreY = true;

        // giữ giới hạn khung cũ của CameraFollow (vùng tâm camera được đi tới + nửa khung nhìn)
        if (oldFollow != null && oldFollow.bound)
        {
            float halfH = main.orthographicSize;
            float halfW = halfH * main.aspect;
            Vector2 min = oldFollow.minipos - new Vector2(halfW, halfH);
            Vector2 max = oldFollow.maxpos + new Vector2(halfW, halfH);
            GameObject bounds = new GameObject("CameraBounds");
            PolygonCollider2D poly = bounds.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;
            poly.points = new Vector2[] {
                new Vector2(min.x, min.y), new Vector2(max.x, min.y),
                new Vector2(max.x, max.y), new Vector2(min.x, max.y) };
            CinemachineConfiner2D confiner = rig.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = poly;
            confiner.InvalidateBoundingShapeCache();
        }

        rig.AddComponent<CinemachineImpulseListener>();
        impulse = rig.AddComponent<CinemachineImpulseSource>();
        impulse.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulse.ImpulseDefinition.ImpulseDuration = 0.2f;
    }

    public static void Shake(float force)
    {
        if (impulse != null) impulse.GenerateImpulseWithForce(force * 0.3f);
    }
}
