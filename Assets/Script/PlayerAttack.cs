using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float acttackdelay = 0.3f;
    public float attackTime = 0.3f;
    public bool attacking = false;
    public bool battack = false;
    public SoundManager sound;
    public Animator anim;

    public Collider2D trigger;
    Player player;
    float sprayTimer;
    bool impactSound;
    bool pendingSpray;
    [System.NonSerialized] public Transform Muzzle;
    public bool InputHeld => battack || GameInput.Attack.IsPressed();
    public float Progress => attacking ? 1f - acttackdelay / attackTime : 0;

    public Vector3 Nozzle => Muzzle != null ? Muzzle.position : transform.position
        + new Vector3(transform.lossyScale.x > 0 ? WeaponLoadout.Start : -WeaponLoadout.Start, WeaponLoadout.Height, 0);

    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        player = GetComponent<Player>();
        attackTime = WeaponLoadout.Duration;
        if (trigger is BoxCollider2D box)
        {
            float sx = Mathf.Max(Mathf.Abs(transform.lossyScale.x), 0.001f);
            float sy = Mathf.Max(Mathf.Abs(transform.lossyScale.y), 0.001f);
            trigger.transform.localScale = Vector3.one;
            trigger.transform.localPosition = new Vector3((WeaponLoadout.Start + WeaponLoadout.Reach * 0.5f) / sx, WeaponLoadout.Height / sy, 0);
            box.offset = Vector2.zero;
            box.size = new Vector2(WeaponLoadout.Reach / sx, (WeaponLoadout.IsSpray ? 0.85f : 1.2f) / sy);
        }
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
        if((GameInput.Attack.IsPressed() || battack) && !attacking)
        {
            attacking = true;
            trigger.enabled = true;
            acttackdelay = attackTime;   // giữ giá trị hiệu chỉnh từ Inspector
            sprayTimer = 0;
            impactSound = false;

        }
        if (attacking)
        {
            acttackdelay = Mathf.Max(0, acttackdelay - Time.deltaTime);
            trigger.enabled = WeaponLoadout.DamageWindow(Progress);
            if (WeaponLoadout.IsSpray && trigger.enabled)
            {
                sprayTimer -= Time.deltaTime;
                if (sprayTimer <= 0)
                {
                    sprayTimer = 0.12f;
                    pendingSpray = true;
                    if (Muzzle == null) EmitSpray();
                }
            }
            if (!WeaponLoadout.IsSpray && trigger.enabled && !impactSound)
            {
                impactSound = true;
                Fx.Hit();
            }
            if (acttackdelay <= 0)
            {
                attacking = false;
                trigger.enabled = false;
            }
        }
        // The legacy attack frames contain a grey gun; the held tool now animates separately.
        if (anim != null) anim.SetBool("Attacking", false);
    }
    public void PlayerActtack(bool attack)
    {
        battack = attack;
    }

    public void SyncMuzzle()
    {
        if (WeaponLoadout.IsSpray && trigger != null && Muzzle != null)
            trigger.transform.position = Nozzle + new Vector3((transform.lossyScale.x > 0 ? 1 : -1) * WeaponLoadout.Reach * 0.5f, 0, 0);
    }

    // Called after the visual hand, bottle and nozzle have been positioned this frame.
    public void EmitSpray()
    {
        if (!pendingSpray || !attacking || trigger == null || !trigger.enabled) return;
        pendingSpray = false;
        Fx.Spray(Nozzle, transform.lossyScale.x > 0);
    }

    public void CancelAttack()
    {
        attacking = battack = false;
        pendingSpray = false;
        acttackdelay = 0;
        if (trigger != null) trigger.enabled = false;
        if (anim != null) anim.SetBool("Attacking", false);
    }

    void OnDisable() { CancelAttack(); }
}
