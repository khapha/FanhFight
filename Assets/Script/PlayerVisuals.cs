using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    Player player;
    PlayerAttack attack;
    SpriteRenderer body, shadow, bottle;
    bool wasGrounded;
    float splashAt;

    void Start()
    {
        player = GetComponent<Player>();
        attack = GetComponent<PlayerAttack>();
        body = GetComponent<SpriteRenderer>();
        shadow = Add("FootShadow", MiniArt.Circle, body.sortingOrder - 1);
        shadow.color = new Color(0.16f, 0.3f, 0.25f, 0.2f);
        shadow.transform.localScale = new Vector3(0.8f, 0.13f, 1);
        bottle = Add("SprayBottle", MiniArt.Bottle, body.sortingOrder + 1);
        bottle.transform.localPosition = new Vector3(0.65f, -0.3f, 0);
        bottle.transform.localScale = Vector3.one * 0.45f;
        wasGrounded = player.grounded;
    }

    SpriteRenderer Add(string name, Sprite sprite, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }

    void LateUpdate()
    {
        if (player == null || body == null) return;
        shadow.enabled = player.grounded && player.ourHealth > 0 && GameSession.State != GameSession.Phase.Respawning;
        shadow.transform.localPosition = new Vector3(0, (body.bounds.min.y - transform.position.y + 0.03f) / transform.lossyScale.y, 0);
        bottle.enabled = attack != null && attack.attacking && body.enabled && player.CanAct;
        if (player.grounded && !wasGrounded && player.CanAct) Fx.Dust(new Vector3(transform.position.x, body.bounds.min.y, 0));
        wasGrounded = player.grounded;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null || !player.CanAct || !other.CompareTag("water") || Time.time < splashAt) return;
        splashAt = Time.time + 0.5f;
        Fx.Burst(transform.position, new Color(0.55f, 0.95f, 1), 12, 2, 0.12f);
    }
}
