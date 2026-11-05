using UnityEngine;

// Flying roaches hover into range, warn, then commit to a dodgeable swoop.
public class Enemy : MonoBehaviour
{
    public Animator FlyAnimator;
    public bool fly;
    public Player player;
    public float argoRange = 6f, moveSpeed = 3f;
    public Rigidbody2D rb2d;
    public float verticalRange = 1.5f, retreatTime = 1f;
    enum State { Hover, Windup, Swoop, Retreat }
    State state;
    RoachStatus status;
    Vector2 home, swoopDirection;
    float stateTime, attackCooldown = 1f;
    bool defeated;
    int health = 24;
    public int Health => health;

    void Awake()
    {
        FlyAnimator = GetComponent<Animator>();
        status = GetComponent<RoachStatus>();
        if (status == null) status = gameObject.AddComponent<RoachStatus>();
        status.Type = RoachStatus.Species.Flyer;
    }
    void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null) player = target.GetComponent<Player>();
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
        home = transform.position;
    }
    void Enter(State next, float duration)
    {
        state = next;
        stateTime = duration;
        status.Telegraphing = next == State.Windup;
    }
    void FixedUpdate()
    {
        if (rb2d == null || defeated) return;
        if (!GameSession.IsPlaying || player == null || !player.CanAct)
        {
            rb2d.linearVelocity = Vector2.zero;
            fly = false;
            return;
        }
        float dt = Time.fixedDeltaTime;
        float speed = Mathf.Max(1f, moveSpeed);
        Vector2 position = transform.position;
        Vector2 toPlayer = (Vector2)player.transform.position - position;
        float sign = toPlayer.x >= 0 ? 1f : -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -sign;
        transform.localScale = scale;
        attackCooldown = Mathf.Max(0, attackCooldown - dt);
        if (status.Stunned)
        {
            Enter(State.Retreat, Mathf.Max(0.7f, retreatTime));
            rb2d.linearVelocity = status.KnockbackVelocity;
            fly = false;
            if (FlyAnimator != null) FlyAnimator.SetBool("Fly", false);
            return;
        }
        stateTime -= dt;
        if (state == State.Windup && stateTime <= 0)
        {
            // Aim locks when the warning ends; it never tracks during the rush.
            Vector2 aim = player.transform.position;
            aim.y = Mathf.Clamp(aim.y + 0.1f, home.y - verticalRange, home.y + verticalRange);
            swoopDirection = (aim - position).normalized;
            Enter(State.Swoop, 0.62f);
        }
        else if (state == State.Swoop && stateTime <= 0) Enter(State.Retreat, Mathf.Max(0.8f, retreatTime));
        else if (state == State.Retreat && stateTime <= 0)
        {
            Enter(State.Hover, 0);
            attackCooldown = 0.8f;
        }
        Vector2 velocity;
        if (state == State.Windup) velocity = new Vector2(0, Mathf.Sin(Time.time * 18f) * 0.2f);
        else if (state == State.Swoop) velocity = swoopDirection * speed * 1.8f;
        else if (state == State.Retreat)
        {
            Vector2 retreat = new Vector2(Mathf.Clamp(position.x - sign * 2f, home.x - argoRange, home.x + argoRange), home.y + 0.4f);
            velocity = Vector2.ClampMagnitude((retreat - position) * 1.5f, speed);
        }
        else if (toPlayer.sqrMagnitude < argoRange * argoRange)
        {
            Vector2 hover = new Vector2(player.transform.position.x - sign * 2.5f,
                Mathf.Clamp(player.transform.position.y + 1.1f, home.y - verticalRange, home.y + verticalRange));
            hover.x = Mathf.Clamp(hover.x, home.x - argoRange, home.x + argoRange);
            velocity = Vector2.ClampMagnitude(hover - position, speed * 0.65f);
            velocity.y += Mathf.Sin(Time.time * 6f) * 0.2f;
            if (attackCooldown <= 0 && toPlayer.sqrMagnitude < 16 && Mathf.Abs(toPlayer.y) < 2.6f)
            {
                Enter(State.Windup, 0.65f);
                velocity = Vector2.zero;
            }
        }
        else
        {
            Vector2 rest = home + Vector2.up * Mathf.Sin(Time.time * 3f) * 0.15f;
            velocity = Vector2.ClampMagnitude(rest - position, speed * 0.5f);
        }
        velocity *= status.SpeedMultiplier;
        if ((position.y < home.y - verticalRange && velocity.y < 0)
            || (position.y > home.y + verticalRange + 0.5f && velocity.y > 0)) velocity.y = 0;
        rb2d.linearVelocity = velocity;
        fly = state == State.Windup || state == State.Swoop || velocity.sqrMagnitude > 0.1f;
        if (FlyAnimator != null) FlyAnimator.SetBool("Fly", fly);
    }
    void OnTriggerEnter2D(Collider2D collision) => TouchPlayer(collision);
    void OnTriggerStay2D(Collider2D collision) => TouchPlayer(collision);
    void TouchPlayer(Collider2D collision)
    {
        if (defeated || !GameSession.IsPlaying || status.Stunned || state != State.Swoop) return;
        if (collision.isTrigger || !collision.CompareTag("Player")) return;
        Player target = collision.GetComponent<Player>();
        if (target == null || !target.CanTakeDamage) return;
        target.Damage(1);
        target.KnockBack(250f, transform.position);
        Enter(State.Retreat, Mathf.Max(0.8f, retreatTime));
    }
    void Damage(int damage)
    {
        if (defeated || !GameSession.IsPlaying || damage <= 0) return;
        health = Mathf.Max(0, health - damage);
        if (health <= 0) Defeat();
    }
    void Defeat()
    {
        if (defeated || player == null || !player.CanAct) return;
        defeated = true;
        status.Telegraphing = false;
        Fx.HitStop();
        Fx.Popup(transform.position, "+5", new Color(1f, 0.6f, 0.3f));
        if (player.gm != null) player.gm.points += 5;
        Fx.Burst(transform.position, new Color(0.5f, 0.3f, 0.2f), 15, 3f, 0.2f);
        Destroy(gameObject);
    }
}