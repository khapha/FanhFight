using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    public float speed = 50f, maxspeed = 4, maxjump = 4, jumpPow = 220f;
    public bool grounded = true, facerright = true, doublejump = false;
    public int ourHealth;
    public int maxhealth = 4;
    public Rigidbody2D r2;
    public Animator anim;
    public GameMaster gm;
    public SoundManager sound;
    public float h = 0;
    public bool jump = false;
    private float baseSpeed, baseMaxspeed;
    private bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        r2 = gameObject.GetComponent<Rigidbody2D>();   
        anim = gameObject.GetComponent<Animator>();
        baseSpeed = speed;
        baseMaxspeed = maxspeed;
        ourHealth = maxhealth;
        sound = GameObject.FindGameObjectWithTag("Sounds").GetComponent<SoundManager>();
        gm = GameObject.FindGameObjectWithTag("gamemaster").GetComponent<GameMaster>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Grounded", grounded);
        anim.SetFloat("Speed", Mathf.Abs(r2.velocity.x));
        if (Input.GetKeyDown(KeyCode.UpArrow) || jump == true)
        {
            if (grounded)
            {
                grounded = false;
                doublejump = true;
                r2.AddForce(Vector2.up * jumpPow);
            }
            else
            {
                if (doublejump)
                {
                    doublejump = false;
                    Invoke("Djump", 0.5f);
                }
            }
                
        }
    }
    void FixedUpdate()
    {
        Move(h);
        r2.AddForce((Vector2.right) * speed * h);

        float hkey = Input.GetAxis("Horizontal");
        r2.AddForce((Vector2.right) * speed * hkey);



        //giới hạn tốc độ
        if (r2.velocity.x > maxspeed)
            r2.velocity = new Vector2(maxspeed, r2.velocity.y);
        if (r2.velocity.x < -maxspeed)
            r2.velocity = new Vector2(-maxspeed, r2.velocity.y);


        //giới hạn độ cao nhảy
        if (r2.velocity.y > maxjump)
            r2.velocity = new Vector2(r2.velocity.x, maxjump);
        if(r2.velocity.y < -maxjump)
            r2.velocity = new Vector2(r2.velocity.x, -maxjump);

        if ((h >0 && !facerright) || (hkey > 0 && !facerright))
        {
            Flip();
        }
        if ((h < 0 && facerright) || (hkey < 0 && facerright))
        {
            Flip();
        }
        if(grounded)
        {
            r2.velocity = new Vector2(r2.velocity.x * 0.7f, r2.velocity.y);
        }
        if (ourHealth <= 0)
        {
            Death();
        }

    }
    public void Djump()
    {
        doublejump = false;
        r2.velocity = new Vector2(r2.velocity.x, 0);
        r2.AddForce(Vector2.up * jumpPow);
    }
    public void Jumping (bool Bjump)
    {
        jump = Bjump;
    }
    public void Move (float Binput)
    {
        h = Binput;
    }

    public void Jump()
    {
        doublejump = false;

    }
    public void Flip()
    {
        facerright = !facerright;
        Vector3 Scale;
        Scale = transform.localScale;
        Scale.x *= -1;
        transform.localScale = Scale;
    }
    public void Death()
    {
        if (dead) return;
        dead = true;
        if(PlayerPrefs.GetInt("highscore") < gm.points)
        {
            PlayerPrefs.SetInt("highscore", gm.points);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    public void Damage(int damage)
    {
        ourHealth -= damage;
        gameObject.GetComponent<Animation>().Play("RedFlash");
    }
    public void KnockBack (float Knockpow, Vector2 sourcePos)
    {
        // đẩy lùi ra xa nguồn gây sát thương
        float dir = transform.position.x >= sourcePos.x ? 1f : -1f;
        r2.velocity = new Vector2(0, 0);
        r2.AddForce(new Vector2(dir * 100f, Knockpow));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coins"))
        {
            sound.Playsound("coins");
            Destroy(collision.gameObject);
            gm.points += 1;

        }
        if (collision.CompareTag("heart"))
        {
            Destroy(collision.gameObject);
            ourHealth = maxhealth;
        }
        if (collision.CompareTag("shoes"))
        {
            Destroy(collision.gameObject);
            maxspeed = 6;
            speed = 100f;
            StartCoroutine(timecount(5));
        }

    }
    IEnumerator timecount (float time)
    {
        yield return new WaitForSeconds(time);
        maxspeed = baseMaxspeed;
        speed = baseSpeed;
        yield return 0;
            
    }

}
