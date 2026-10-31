using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public int dmg = 20;
    // mỗi nhát dép chỉ trúng mỗi địch MỘT lần, kể cả khi vùng chém được bật lên
    // trong lúc đã chồng sẵn lên địch (OnTriggerEnter không fire trong trường hợp đó)
    readonly HashSet<Collider2D> hitThisSwing = new HashSet<Collider2D>();
    Collider2D myTrigger;

    void Awake()
    {
        myTrigger = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision) => TryHit(collision);

    // Stay bắt cả trường hợp bật vùng chém khi đã đang chồng lên địch
    void OnTriggerStay2D(Collider2D collision) => TryHit(collision);

    void TryHit(Collider2D collision)
    {
        if (!GameSession.IsPlaying) return;
        if (collision == null || hitThisSwing.Contains(collision)) return;
        // dép trúng mọi collider gắn tag Enemy (cả sensor trigger của gián code-gen:
        // collider đặc của Vua Gián nằm dưới đất nên trước đây dép không bao giờ chạm)
        if (collision.CompareTag("Enemy"))
        {
            hitThisSwing.Add(collision);
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
