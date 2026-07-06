using System;
using UnityEngine;
using System.Collections;

public class Player : Character
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 7;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private Kunai kunaiPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject attackArea;
    //Dung box check chan cua player da cham groundLayer
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private bool isGrounded = true;
    private bool isAttack = false;
    //private bool isJumping = false;
    private float horizontal;
    private int coin = 0;
    private Vector3 savePoint;

    [Header("Combo Timing Settings")]
    [SerializeField] private GameObject timingCirclePrefab;
    [SerializeField] private float shrinkTime = 0.5f;
    [SerializeField] private float perfectWindow = 0.15f; //Thoi gian duoc phep bam som hoac muon hon

    public int GetComboStep() => comboStep;
    public CombatText GetCombatTextPrefab() => combatTextPrefab;

    private bool isTimingActive = false;
    private float timingStartTime = 0f;
    private int comboStep = 0;
    private GameObject currentCircle;
    private Vector3 lastTargetPos;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isDashing = false;
    private float lastDashTime = -100f;
    private float originalGravity;

    private void Awake()
    {
        coin = PlayerPrefs.GetInt("coin", 0);        
    }

    void Update()
    {
        isGrounded = CheckGrounded();
        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsDead)
        {
            return;
        }

        if (isDashing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
            return;
        }

        if (isGrounded && Input.GetKeyDown(KeyCode.J))
        {
            if (!isAttack || isTimingActive)
            {
                Attack();
            }
        }

        if (isAttack)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (isGrounded)
        {
            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    Attack();
            //    return; //ngung update frame ngay
            //}

            if (Input.GetKeyDown(KeyCode.K))
            {
                Throw();
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                Jump();
            }
        }

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                ChangeAnim("jump");
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                ChangeAnim("fall");
            }
        }

        if (Mathf.Abs(horizontal) > 0.1f)
        {
            if (isGrounded) ChangeAnim("run");

            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
            transform.rotation = Quaternion.Euler(new Vector3(0, horizontal > 0 ? 0 : 180, 0));
        }
        else
        {
            if (isGrounded) ChangeAnim("idle");

            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public override void OnInit()
    {
        base.OnInit();

        isAttack = false;
        isDashing = false;
        originalGravity = rb.gravityScale; //nho trong luc goc

        transform.position = savePoint;
        ChangeAnim("idle");
        DeactiveAttack();

        SavePoint();
        UIManager.instance.SetCoin(coin);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        OnInit();
    }

    public override void OnHit(float damage)
    {
        if (isDashing)
        {
            return;
        }
        base.OnHit(damage);
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }

    private bool CheckGrounded()
    {
        //Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.1f, Color.red);
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        //return hit.collider != null;

        Collider2D collider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return collider != null;
    }

    public void Attack()
    {
        //Huy lenh tat danh de khong ghi de len combo
        CancelInvoke(nameof(ResetAttack));
        CancelInvoke(nameof(DeactiveAttack));

        isAttack = true;
        //ChangeAnim("attack");
        //Invoke(nameof(ResetAttack), 0.5f);
        //ActiveAttack();
        //Invoke(nameof(DeactiveAttack), 0.5f);

        bool comboCheck = false;
        if (isTimingActive)
        {
            float timePassed = Time.time - timingStartTime;
            if(Mathf.Abs(timePassed - shrinkTime) <= perfectWindow)
            {
                comboStep++;
                if(comboStep > 2)
                {
                    comboStep = 0;
                }
                comboCheck = true;
            }
            else
            {
                comboStep = 0;

                if (combatTextPrefab != null)
                {
                    Vector3 textSpawnPos = transform.position + Vector3.up * 2.5f;
                    Instantiate(combatTextPrefab, textSpawnPos, Quaternion.identity).OnInit("Miss!");
                }
            }

            isTimingActive = false;
            if(currentCircle != null)
            {
                Destroy(currentCircle);
            }
        }
        else
        {
            comboStep = 0;
        }

        //Neu bam dung nhip hien vong timing moi
        if (comboCheck && comboStep > 0)
        {
            TriggerTiming(lastTargetPos);
        }

        if (comboStep == 1) ChangeAnim("attack1");
        else if (comboStep == 2) ChangeAnim("attack2");
        else ChangeAnim("attack");

        Invoke(nameof(ResetAttack), 0.5f);
        ActiveAttack();
        Invoke(nameof(DeactiveAttack), 0.5f);
    }

    public void Throw()
    {
        if (isGrounded)
        {
            isAttack = true;
            ChangeAnim("throw");
            Invoke(nameof(ResetAttack), 0.5f);

            Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
        }
    }

    private void ResetAttack()
    {
        isAttack = false;
        //update co the tu tinh toan chuyen ve idle
    }

    public void Jump()
    {
        //isJumping = true;
        //ChangeAnim("jump");
        //rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void Dash()
    {
        if (!isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Coin")
        {
            coin++;
            PlayerPrefs.SetInt("coin", coin);
            UIManager.instance.SetCoin(coin);
            Destroy(collision.gameObject);
        }
        if(collision.tag == "DeathZone")
        {
            ChangeAnim("die");

            Invoke(nameof(OnInit), 1f);
        }
    }

    private void ActiveAttack()
    {
        attackArea.SetActive(true);
    }

    private void DeactiveAttack()
    {
        attackArea.SetActive(false);
    }

    //public void SetMove(float horizontal)
    //{
    //    this.horizontal = horizontal;
    //}

    internal void SavePoint()
    {
        savePoint = transform.position;
    }

    internal void TriggerTiming(Vector3 spawnPosition)
    {
        lastTargetPos = spawnPosition;
        isTimingActive = true;
        timingStartTime = Time.time;

        if(timingCirclePrefab != null)
        {
            currentCircle = Instantiate(timingCirclePrefab, spawnPosition, Quaternion.identity);
            currentCircle.GetComponent<TimingCircle>().OnInit(shrinkTime);
        }
    }

    private void ChangeAlpha(float alpha)
    {
        if(spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    public IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        ChangeAnim("dash");
        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }
        ChangeAlpha(0.4f);

        rb.gravityScale = 0f;
        rb.linearVelocity = transform.right * dashSpeed;
        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
        ChangeAlpha(1f);
    }
}