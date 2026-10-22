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
            rb2d.velocity = new Vector2(moveSpeed, 0);

            transform.localScale = new Vector2(-1, 1);
        }
        else if (transform.position.x > player.transform.position.x)
        {
            //Enemy move left
            rb2d.velocity = new Vector2(-moveSpeed, 0);
            transform.localScale = new Vector2(1, 1);
        }
        fly = true;
    }
    private void stopchase()
    {
        rb2d.velocity = new Vector2(0, 0);
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
            Destroy(gameObject);
        }
    }

}
