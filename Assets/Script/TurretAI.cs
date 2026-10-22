using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour
{
    public int curlHealth = 100;
    public float distance;
    public float wakerange;
    public float shootinterval;
    public float bulletspeed = 2;
    public float bullettimer;

    public bool awake = false;
    public bool lookingRight = true;
    public bool enemyfaceright = true;

    public GameObject bullet;
    public Transform target;
    public Animator anim;
    public Transform shootpointL, shootpointR;

    public SoundManager sound;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }


    // Start is called before the first frame update
    void Start()
    {
        sound = GameObject.FindGameObjectWithTag("Sounds").GetComponent<SoundManager>();

    }
    public void Flip()
    {
        enemyfaceright = !enemyfaceright;
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        if ((target.transform.position.x > transform.position.x) && enemyfaceright)
        {
            Flip();
        }
        if ((target.transform.position.x < transform.position.x) && !enemyfaceright)
        {
            Flip();
        }
    }
    void Update()
    {
       
        anim.SetBool("Awake", awake);
        anim.SetBool("LookRight", lookingRight);
        RangeCheck();
        
       
        if (target.transform.position.x > transform.position.x)
        {
            lookingRight = true;

        }
        if (target.transform.position.x < transform.position.x)
        {
            lookingRight = false;
        }
        if (curlHealth <= 0)
        {
            sound.Playsound("destroy");
            Destroy(gameObject);
        }
    }
   
    void RangeCheck()
    {
        distance = Vector2.Distance(transform.position, target.transform.position);
        if(distance < wakerange)
        {
            awake = true;
        }
        if(distance > wakerange)
        {
            awake = false;
        }
    }
    public void Attack (bool attactright)
    {
        bullettimer += Time.deltaTime;
        if (bullettimer >= shootinterval)
        {
            Vector2 direction = target.transform.position - transform.position;
            direction.Normalize();
            if (attactright)
            {
                GameObject bulletclone;
                bulletclone = Instantiate(bullet, shootpointR.transform.position, shootpointR.transform.rotation) as GameObject;
                if (!enemyfaceright)
                {
                    Vector3 s = bulletclone.transform.localScale;
                    s.x *= -1;
                    bulletclone.transform.localScale = s;
                }
                bulletclone.GetComponent<Rigidbody2D>().velocity = direction * bulletspeed;
                bullettimer = 0;
            }
            if (!attactright)
            {
                GameObject bulletclone;
                bulletclone = Instantiate(bullet, shootpointL.transform.position, shootpointL.transform.rotation) as GameObject;
                if (!enemyfaceright)
                {
                    Vector3 s = bulletclone.transform.localScale;
                    s.x *= -1;
                    bulletclone.transform.localScale = s;
                }
                bulletclone.GetComponent<Rigidbody2D>().velocity = direction * bulletspeed;
                bullettimer = 0;
            }
        }
    }
    void Damage(int damage)
    {
        curlHealth -= damage;
        gameObject.GetComponent<Animation>().Play("RedFlash");
    }
}
