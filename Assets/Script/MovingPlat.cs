using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlat : MonoBehaviour
{
    // speed tính theo đơn vị mỗi khung hình ở 60 FPS (giữ nguyên giá trị cũ trong scene)
    public float speed = 0.06f, changeDirection = -1;
    Vector3 Move;
    // Start is called before the first frame update
    public PausedMenu pause;

    // vận tốc thật, đơn vị/giây
    public float Velocity { get { return speed * 60f; } }

    void Start()
    {
        Move = transform.position;
        pause = GameObject.FindGameObjectWithTag("MainCamera").GetComponentInParent<PausedMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pause != null && pause.pause) return;
        // nhân deltaTime để chạy như nhau ở mọi FPS
        Move.x += Velocity * Time.deltaTime;
        transform.position = Move;
    }
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Ground"))
        {
            speed *= changeDirection;
        }
    }
}
