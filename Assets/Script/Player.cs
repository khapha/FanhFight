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
    public bool CanAct => !dead && ourHealth > 0 && GameSession.IsPlaying;
    public bool CanTakeDamage => CanAct && invincibleTimer <= 0 && !Dashing && !GodMode;
    public float coyoteTime = 0.1f, jumpBuffer = 0.1f, invincibleTime = 1f;
    private float coyoteTimer, bufferTimer, invincibleTimer;
    [Header("Dash")]
    public float dashSpeed = 12f, dashDuration = 0.18f, dashCooldown = 0.9f;
    private float dashTimer, dashCooldownTimer, dashFxTimer, dashDir = 1f;
    public bool Dashing => dashTimer > 0;
    // bot tự chơi dùng: giữ bất tử tạm thời để đi xuyên bẫy (không ảnh hưởng chơi thường)
    public bool GodMode { get; set; }
    private bool prevJump = false;
    private SpriteRenderer sr;
    private float dustTimer;

    // Start is called before the first frame update
    void Start()
    {
        r2 = gameObject.GetComponent<Rigidbody2D>();   
        r2.interpolation = RigidbodyInterpolation2D.Interpolate;
        anim = gameObject.GetComponent<Animator>();
        sr = gameObject.GetComponent<SpriteRenderer>();
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
        anim.SetFloat("Speed", Mathf.Abs(r2.linearVelocity.x));
        if (!CanAct)
        {
            h = 0;
            jump = prevJump = false;
            bufferTimer = 0;
            return;
        }

        // coyote time: vẫn nhảy được ngay sau khi rời mép
        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        // nút nhảy trên màn hình chỉ tính lúc vừa bấm
        bool jumpPressed = GameInput.Jump.WasPressedThisFrame() || (jump && !prevJump);
        bool jumpReleased = GameInput.Jump.WasReleasedThisFrame() || (!jump && prevJump);
        prevJump = jump;

        if (jumpPressed) bufferTimer = jumpBuffer;
        else bufferTimer -= Time.deltaTime;

        if (bufferTimer > 0 && coyoteTimer > 0)
        {
            grounded = false;
            doublejump = true;
            coyoteTimer = 0;
            bufferTimer = 0;
            r2.linearVelocity = new Vector2(r2.linearVelocity.x, 0);
            r2.AddForce(Vector2.up * jumpPow);
            Fx.Jump();
            Fx.Dust(Feet());
        }
        else if (jumpPressed && doublejump)
        {
            bufferTimer = 0;
            Djump();
        }

        // thả phím sớm thì nhảy thấp hơn
        if (jumpReleased && r2.linearVelocity.y > 0)
            r2.linearVelocity = new Vector2(r2.linearVelocity.x, r2.linearVelocity.y * 0.5f);

        // dash: lướt nhanh theo hướng đang quay, bất tử trong lúc lướt (mở khóa bằng orb)
        if (QuestState.DashUnlocked && GameInput.Dash.WasPressedThisFrame() && dashCooldownTimer <= 0)
        {
            float moveNow = Mathf.Clamp(h + GameInput.MoveX, -1f, 1f);
            dashDir = Mathf.Abs(moveNow) > 0.1f ? Mathf.Sign(moveNow) : (facerright ? 1f : -1f);
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashFxTimer = 0f;
            r2.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
            Fx.Jump();
            Fx.Dust(Feet());
        }

        // nhấp nháy khi đang bất tử
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
            if (sr != null) sr.enabled = invincibleTimer <= 0 || Mathf.Repeat(invincibleTimer, 0.2f) > 0.1f;
        }
    }
    void FixedUpdate()
    {
        if (!dead && ourHealth <= 0) Death();
        // rơi xuống vực/nước: mất mạng và hồi sinh ở checkpoint (trước đây rơi vô hạn mãi).
        // GodMode (bot tự chơi) thì được nhấc về checkpoint an toàn thay vì chết.
        if (!dead && r2 != null && r2.position.y < -7f)
        {
            if (GodMode)
            {
                Vector3 spawn = Checkpoint.SpawnPoint;
                Vector3 delta = spawn - transform.position;
                r2.linearVelocity = Vector2.zero;
                r2.position = spawn;
                transform.position = spawn;
                CameraRig.Warp(transform, delta);
                return;
            }
            Death();
            return;
        }
        if (!CanAct) return;

        // dash đang chạy: giữ nguyên vận tốc lướt, bỏ qua lực điều khiển thường
        if (dashTimer > 0)
        {
            dashTimer -= Time.fixedDeltaTime;
            r2.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
            dashFxTimer -= Time.fixedDeltaTime;
            if (dashFxTimer <= 0)
            {
                dashFxTimer = 0.05f;
                Fx.Dust(Feet());
            }
            return;
        }
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.fixedDeltaTime;

        // một lực duy nhất: nút cảm ứng (h) và bàn phím được cộng lại rồi chặn trong [-1,1],
        // không nhân đôi tốc độ khi cả hai nguồn cùng giữ
        float move = Mathf.Clamp(h + GameInput.MoveX, -1f, 1f);
        r2.AddForce((Vector2.right) * speed * move);

        //giới hạn tốc độ
        if (r2.linearVelocity.x > maxspeed)
            r2.linearVelocity = new Vector2(maxspeed, r2.linearVelocity.y);
        if (r2.linearVelocity.x < -maxspeed)
            r2.linearVelocity = new Vector2(-maxspeed, r2.linearVelocity.y);


        //giới hạn độ cao nhảy
        if (r2.linearVelocity.y > maxjump)
            r2.linearVelocity = new Vector2(r2.linearVelocity.x, maxjump);
        if(r2.linearVelocity.y < -maxjump)
            r2.linearVelocity = new Vector2(r2.linearVelocity.x, -maxjump);

        if (move > 0 && !facerright)
        {
            Flip();
        }
        if (move < 0 && facerright)
        {
            Flip();
        }
        if(grounded)
        {
            r2.linearVelocity = new Vector2(r2.linearVelocity.x * 0.7f, r2.linearVelocity.y);
        }
        // bụi khi chạy
        dustTimer -= Time.fixedDeltaTime;
        if (grounded && Mathf.Abs(r2.linearVelocity.x) > 2.5f && dustTimer <= 0)
        {
            dustTimer = 0.2f;
            Fx.Dust(Feet());
        }

    }
    public void Djump()
    {
        doublejump = false;
        r2.linearVelocity = new Vector2(r2.linearVelocity.x, 0);
        r2.AddForce(Vector2.up * jumpPow);
        Fx.Jump();
    }
    Vector3 Feet()
    {
        return sr != null ? new Vector3(transform.position.x, sr.bounds.min.y, 0) : transform.position;
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
        h = 0;
        jump = prevJump = false;
        bufferTimer = coyoteTimer = 0;
        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.CancelAttack();
        gm.SaveHighscore();
        if (sr != null) sr.enabled = true;
        Checkpoint.Lives--;
        if (Checkpoint.Lives > 0)
        {
            GameSession.BeginRespawn();
            StartCoroutine(Respawn());
        }
        else
            GameOverUI.Show("GAME OVER", gm.points, gm.highscore);
    }

    // còn mạng thì hồi sinh ở checkpoint gần nhất, đầy máu
    IEnumerator Respawn()
    {
        r2.linearVelocity = Vector2.zero;
        Fx.Burst(transform.position, Color.white, 20, 3f, 0.2f);
        if (sr != null) sr.enabled = false;
        yield return new WaitForSeconds(0.8f);
        Vector3 previousPosition = transform.position;
        r2.position = Checkpoint.SpawnPoint;
        transform.position = Checkpoint.SpawnPoint;
        CameraRig.Warp(transform, transform.position - previousPosition);
        r2.linearVelocity = Vector2.zero;
        ourHealth = maxhealth;
        dead = false;
        grounded = doublejump = false;
        GameSession.EndRespawn();
        invincibleTimer = invincibleTime * 1.5f;
        if (sr != null) sr.enabled = true;
    }



    public void Damage(int damage)
    {
        if (!CanTakeDamage) return;
        invincibleTimer = invincibleTime;
        Fx.Hurt();
        CameraRig.Shake(1f);
        ourHealth -= damage;
        Animation flash = GetComponent<Animation>();
        if (flash != null) flash.Play("RedFlash");
        if (ourHealth <= 0) Death();
    }
    public void KnockBack (float Knockpow, Vector2 sourcePos)
    {
        if (dead) return;
        // đẩy lùi ra xa nguồn gây sát thương
        float dir = transform.position.x >= sourcePos.x ? 1f : -1f;
        r2.linearVelocity = new Vector2(0, 0);
        r2.AddForce(new Vector2(dir * 100f, Knockpow));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanAct) return;
        if (collision.CompareTag("Coins"))
        {
            sound.Playsound("coins");
            Destroy(collision.gameObject);
            gm.points += 1;
            Fx.Popup(collision.transform.position, "+1", new Color(1f, 0.9f, 0.2f));

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
            // hủy đếm cũ nếu đang chạy (nhặt giày mới = đồng hồ bắt đầu lại)
            if (shoesTimer != null) StopCoroutine(shoesTimer);
            shoesTimer = StartCoroutine(timecount(5));
        }

    }
    Coroutine shoesTimer;
    IEnumerator timecount (float time)
    {
        yield return new WaitForSeconds(time);
        maxspeed = baseMaxspeed;
        speed = baseSpeed;
        shoesTimer = null;
    }

}
