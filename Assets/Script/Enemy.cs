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

    public float verticalRange = 1.5f;
    public float retreatTime = 1f;
    private float homeX, homeY, retreatTimer;
    bool defeated;
    // Start is called before the first frame update
    private void Awake()
    {
        FlyAnimator = GetComponent<Animator>();
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        homeX = transform.position.x;
        homeY = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
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
        // vừa cắn trúng thì lùi ra xa một lúc
        float dirX = Mathf.Sign(player.transform.position.x - transform.position.x);
        if (retreatTimer > 0)
        {
            retreatTimer -= Time.deltaTime;
            dirX = -dirX;
        }
        // chỉ bay quanh độ cao ban đầu, không chui xuống đất
        float targetY = Mathf.Clamp(player.transform.position.y + 0.3f, homeY - verticalRange, homeY + verticalRange);
        float bob = Mathf.Sin(Time.time * 6f) * 0.3f;
        float vy = Mathf.Clamp(targetY - transform.position.y, -1f, 1f) * moveSpeed + bob;
        rb2d.linearVelocity = new Vector2(dirX * moveSpeed, vy);
        fly = true;
    }
    private void stopchase()
    {
        // bay về chỗ cũ
        Vector2 toHome = new Vector2(homeX, homeY) - (Vector2)transform.position;
        rb2d.linearVelocity = toHome.magnitude > 0.2f ? toHome.normalized * moveSpeed * 0.5f : Vector2.zero;
        fly = toHome.magnitude > 0.2f;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.isTrigger == false)
        {
            if (collision.CompareTag("Player") && player.CanTakeDamage)
            {
                collision.SendMessageUpwards("Damage", 1);
                player.KnockBack(400f, transform.position);
                retreatTimer = retreatTime;

            }
        }
        if (collision.CompareTag("actacktrigger"))
            Defeat();
    }

    // AttackTrigger sends Damage upwards; multiple colliders may report one hit.
    void Damage(int damage)
    {
        if (damage > 0) Defeat();
    }

    void Defeat()
    {
        if (defeated || player == null || !player.CanAct) return;
        defeated = true;
        Fx.HitStop();
        Fx.Popup(transform.position, "+5", new Color(1f, 0.6f, 0.3f));
        if (player.gm != null) player.gm.points += 5;
        Fx.Burst(transform.position, new Color(0.5f, 0.3f, 0.2f), 15, 3f, 0.2f);
        Destroy(gameObject);
    }

}
