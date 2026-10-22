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
    Player player;

    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        player = GetComponent<Player>();
        CancelAttack();
    }

    // Start is called before the first frame update
    void Start()
    {
        sound = GameObject.FindGameObjectWithTag("Sounds").GetComponent<SoundManager>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!GameSession.IsPlaying || (player != null && !player.CanAct))
        {
            CancelAttack();
            return;
        }
        if((GameInput.Attack.WasPressedThisFrame() && !attacking) || (battack == true && !attacking))
        {
            attacking = true;
            trigger.enabled = true;
            acttackdelay = 0.3f;
            Fx.Spray(transform.position, transform.localScale.x > 0);

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

    public void CancelAttack()
    {
        attacking = battack = false;
        acttackdelay = 0;
        if (trigger != null) trigger.enabled = false;
        if (anim != null) anim.SetBool("Attacking", false);
    }

    void OnDisable() { CancelAttack(); }
}
