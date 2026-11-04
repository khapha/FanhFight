using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public int dmg = 20;
    // mỗi nhát dép chỉ trúng mỗi địch MỘT lần, kể cả khi vùng chém được bật lên
    // trong lúc đã chồng sẵn lên địch (OnTriggerEnter không fire trong trường hợp đó)
    readonly HashSet<GameObject> hitThisSwing = new HashSet<GameObject>();
    Collider2D myTrigger;
    readonly Collider2D[] overlaps = new Collider2D[64];

    void Awake()
    {
        myTrigger = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision) => TryHit(collision);

    // Stay bắt cả trường hợp bật vùng chém khi đã đang chồng lên địch
    void OnTriggerStay2D(Collider2D collision) => TryHit(collision);

    void FixedUpdate()
    {
        if (!GameSession.IsPlaying || myTrigger == null || !myTrigger.enabled) return;
        // Enabling a trigger over sleeping bodies does not reliably send a new Enter/Stay.
        // Query the real shape as well; the per-enemy swing set prevents duplicate damage.
        int count = myTrigger.OverlapCollider(new ContactFilter2D { useTriggers = true }, overlaps);
        for (int i = 0; i < count; i++) TryHit(overlaps[i]);
    }

    void TryHit(Collider2D collision)
    {
        if (!GameSession.IsPlaying) return;
        if (collision == null || myTrigger == null || !myTrigger.enabled) return;
        // dép trúng mọi collider gắn tag Enemy (cả sensor trigger của gián code-gen:
        // collider đặc của Vua Gián nằm dưới đất nên trước đây dép không bao giờ chạm)
        if (collision.CompareTag("Enemy"))
        {
            GameObject enemy = collision.attachedRigidbody != null
                ? collision.attachedRigidbody.gameObject : collision.gameObject;
            if (!hitThisSwing.Add(enemy)) return;
            collision.SendMessageUpwards("Damage", dmg);
            Fx.HitStop();
        }
    }

    void Update()
    {
        // nhát chém kết thúc (vùng dép tắt) -> cho phép trúng lại ở nhát sau
        if (myTrigger != null && !myTrigger.enabled) hitThisSwing.Clear();
    }
}
