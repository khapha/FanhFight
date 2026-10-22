using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{
    public MovingPlat mov;
    public Vector3 movp;
    public Player player;
    private Collider2D feet;
    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponentInParent<Player>();
        foreach (Collider2D c in GetComponents<Collider2D>())
            if (c.isTrigger) feet = c;
    }

    // chỉ tính là đứng trên đất khi mặt trên của vật ở ngang chân,
    // chạm cạnh tường không tính (tránh leo tường bằng nhảy liên tục)
    bool IsFloor(Collider2D other)
    {
        if (other.CompareTag("water")) return true;
        if (other.isTrigger) return false;
        if (feet == null) return true;
        return other.bounds.max.y <= feet.bounds.center.y + 0.15f;
    }

    void OnTriggerEnter2D(Collider2D collison)
    {
        if (IsFloor(collison))
            player.grounded = true;
    }
    private void OnTriggerStay2D(Collider2D collison)
    {
        if (IsFloor(collison))
            player.grounded = true;
        if (collison.isTrigger == false && collison.CompareTag("Movingplat"))
        {
            mov = collison.GetComponent<MovingPlat>();
            if (mov == null) return;
            // chở người chơi theo platform (hàm này chạy mỗi bước vật lý)
            movp = player.transform.position;
            movp.x += mov.Velocity * Time.fixedDeltaTime;
            player.transform.position = movp;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isTrigger == false || collision.CompareTag("water"))
            player.grounded = false;
    }
}
