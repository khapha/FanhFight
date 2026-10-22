using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float acttackdelay = 0.3f;
    public bool attacking = false;
    public bool battack = false;
    public SoundManager sound;
    public Animator anim;

    public Collider2D trigger;

    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        trigger.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        sound = GameObject.FindGameObjectWithTag("Sounds").GetComponent<SoundManager>();

    }

    // Update is called once per frame
    void Update()
    {
        if((Input.GetKeyDown(KeyCode.Space) && !attacking) || (battack == true && !attacking))
        {
            attacking = true;
            trigger.enabled = true;
            acttackdelay = 0.3f;
            sound.Playsound("sword");

        }
        if (attacking)
        {
            if(acttackdelay > 0)
            {
                acttackdelay -= Time.deltaTime;
            }
            else
            {
                attacking = false;
                trigger.enabled = false;
            }
        }
        anim.SetBool("Attacking", attacking);
    }
    public void PlayerActtack(bool attack)
    {
        battack = attack;
    }
}
