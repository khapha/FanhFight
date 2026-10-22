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
        // tháp pháo nhân bản từ màn khác sẽ mất tham chiếu tới người chơi
        if (target == null) target = GameObject.FindGameObjectWithTag("Player").transform;

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
            Fx.Burst(transform.position, new Color(1f, 0.6f, 0.2f), 30, 5f, 0.3f);
            CameraRig.Shake(2f);
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
            // bắn đón đầu: nhắm vào chỗ player sẽ tới khi đạn bay đến
            Vector2 aim = target.transform.position;
            Rigidbody2D trb = target.GetComponent<Rigidbody2D>();
            if (trb != null)
            {
                float t = Vector2.Distance(transform.position, aim) / Mathf.Max(bulletspeed, 0.1f);
                aim += trb.linearVelocity * Mathf.Min(t, 1f);
            }
            Vector2 direction = (aim - (Vector2)transform.position).normalized;
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
                bulletclone.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletspeed;
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
                bulletclone.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletspeed;
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
