using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator FlyAnimator;

    public bool fly = false;

    public Player player;

    public float argoRange;

    public float moveSpeed;

    public Rigidbody2D rb2d;
    // Start is called before the first frame update
    private void Awake()
    {
        FlyAnimator = GetComponent<Animator>();
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        FlyAnimator.SetBool("Fly", fly);
        //distance to player
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
       

        if(distToPlayer < argoRange)
        {
            //chase player
            chaseplayer();
        }
        else
        {
            //stop chasing player
            stopchase();
        }
    }

    private void chaseplayer()
    {
        if(transform.position.x < player.transform.position.x)
        {
            //Enemy move right
            transform.localScale = new Vector2(-1, 1);
        }
        else if (transform.position.x > player.transform.position.x)
        {
            //Enemy move left
            transform.localScale = new Vector2(1, 1);
        }
        // bay thẳng về phía player (cả chiều dọc), lượn nhẹ lên xuống
        Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        float bob = Mathf.Sin(Time.time * 6f) * 0.5f;
        rb2d.linearVelocity = dir * moveSpeed + new Vector2(0, bob);
        fly = true;
    }
    private void stopchase()
    {
        rb2d.linearVelocity = new Vector2(0, 0);
        fly = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.isTrigger == false)
        {
            if (collision.CompareTag("Player"))
            {
                collision.SendMessageUpwards("Damage", 1);
                player.KnockBack(400f, transform.position);

            }
        }
        if (collision.CompareTag("actacktrigger"))
        {
            Fx.Burst(transform.position, new Color(0.5f, 0.3f, 0.2f), 15, 3f, 0.2f);
            Destroy(gameObject);
        }
    }

}
