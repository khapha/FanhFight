using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlat : MonoBehaviour
{
    // speed tính theo đơn vị mỗi khung hình ở 60 FPS (giữ nguyên giá trị cũ trong scene)
    public float speed = 0.06f, changeDirection = -1;
    Rigidbody2D body;
    // Start is called before the first frame update
    public PausedMenu pause;

    // vận tốc thật, đơn vị/giây
    public float Velocity { get { return speed * 60f; } }

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        if (body != null) body.interpolation = RigidbodyInterpolation2D.Interpolate;
        pause = GameObject.FindGameObjectWithTag("MainCamera").GetComponentInParent<PausedMenu>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (pause != null && pause.pause) return;
        // nhân deltaTime để chạy như nhau ở mọi FPS
        if (body != null) body.MovePosition(body.position + Vector2.right * Velocity * Time.fixedDeltaTime);
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Ground"))
        {
            speed *= changeDirection;
        }
    }
}
