using UnityEngine;

public class EnemyVisuals : MonoBehaviour
{
    Enemy roach;
    TurretAI turret;
    SpriteRenderer body, warning, health;
    float phase;

    void Start()
    {
        roach = GetComponent<Enemy>();
        turret = GetComponent<TurretAI>();
        body = GetComponent<SpriteRenderer>();
        phase = transform.position.x;
        if (roach != null && roach.rb2d != null) roach.rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        warning = Add("AttackWarning", CuteArt.Alert, new Vector3(0, 1, 0));
        warning.transform.localScale = Vector3.one * 0.7f;
        if (turret != null)
        {
            health = Add("TurretHealth", MiniArt.Circle, new Vector3(0, 0.7f, 0));
            health.color = CuteTheme.Mint;
        }
    }

    SpriteRenderer Add(string name, Sprite sprite, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = body != null ? body.sortingOrder + 2 : 8;
        return sr;
    }

    void LateUpdate()
    {
        if (body == null || warning == null) return;
        if (roach != null)
        {
            body.sprite = CuteArt.Roach((int)(Time.time * (roach.fly ? 10 : 4) + phase) % 2);
            warning.enabled = roach.fly && roach.player != null
                && (roach.player.transform.position - transform.position).sqrMagnitude < 16;
        }
        else if (turret != null)
        {
            warning.enabled = turret.awake && turret.bullettimer >= Mathf.Max(0, turret.shootinterval - 0.35f);
            health.enabled = turret.curlHealth < 100;
            health.transform.localScale = new Vector3(Mathf.Clamp01(turret.curlHealth / 100f) * 0.9f, 0.08f, 1);
        }
        warning.transform.localPosition = new Vector3(0, 1.05f + Mathf.Sin(Time.time * 4 + phase) * 0.05f, 0);
    }
}
