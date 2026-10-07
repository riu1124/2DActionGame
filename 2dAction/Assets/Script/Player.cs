using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("移動スピード")]
    [SerializeField] float moveSpeed = 5f;
    [Header("ジャンプ -- 高さ")]
    [SerializeField] float jumpHeight = 3f;

    [Header("ジャンプ -- 上昇時間 (頂点まで何秒か。小さい程早く上がる)")]
    [SerializeField] float riseTime = 0.4f;

    [Header("ジャンプ -- 降下時間(頂点空同じ高さに戻るまで何秒か。小さいほど早く落ちる)")]
    [SerializeField] float fallTime = 0.3f;

    [Header("ジャンプ -- 落ちる早さの上限(高いところから落ちた時用)")]
    [SerializeField] float maxFallSpeed = 20f;

    [Header("接地判定 -- 足元の位置")]
    [SerializeField] Transform groundCheck;

    [Header("接地判定 -- 足元の円の半径")]
    [SerializeField] float groundRadius = 0.12f;

    [Header("接地判定 -- 地面と見なるレイヤー")]
    [SerializeField] LayerMask GroundLayers;

    [Header("HP -- 最大値")]
    [SerializeField] int maxHp = 4;

    [Header("HP -- ライフ表示用のUI")]
    [SerializeField] Life life;

    [Header("被弾 -- 飛ばされる速度(X+ = 向いている方向、x- = 真後ろ、Y+ = 上")]
    [SerializeField] Vector2 knockbackVelocity = new Vector2(-4f, 6f);

    [Header("被弾　-- 操作できない時間(秒)")]
    [SerializeField] float hitStunTime = 0.4f;

    [Header("サウンド -- 移動開始 SE")]
    [SerializeField] AudioClip moveStartClip;
    [Header("サウンド -- ジャンプ SE")]
    [SerializeField] AudioClip jumpClip;
    [Header("サウンド -- 着地 SE")]
    [SerializeField] AudioClip landClip;
    [Header("サウンド -- ダメージ SE")]
    [SerializeField] AudioClip damageClip;
    [Header("サウンド -- 死亡 SE")]
    [SerializeField] AudioClip deathClip;
    float minInputToMove = 0.2f;
    Rigidbody2D rigidBody2D;            //物理特性を扱う変数
    SpriteRenderer spriteRenderer;      //見た目(左右反転に使用)
    InputAction moveAction;             //Player/Moveの入力
    InputAction jumpAction;             //Player/Jumpの入力
    float moveInputX;                   //左右の入力値
    int facing = 1;                     //向いている方向(1=右、-1=左)
    bool isGrounded;
    int currentHp;
    float hitStunTimer;

    bool isDead;

    bool wasGrounded;

    AudioSource audioSource;
    bool wasMoving;

    void Awake()
    {

        rigidBody2D = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();

        moveAction = InputSystem.actions.FindAction("Player/Move");

        jumpAction = InputSystem.actions.FindAction("Player/Jump");

        currentHp = maxHp;
      
  
    }

    void OnEnable()
    {
        moveAction.Enable();    //入力を受け付ける
        jumpAction.Enable();    
    }

    void OnDisable()
    {
        moveAction.Disable();   //入力を受け付けない
        jumpAction.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        life.Setup(maxHp);
        life.SetLife(currentHp);
    }

    // Update is called once per frame
    void Update()
    {
        if(hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
        }

        if (isDead)
        {
            moveInputX = 0f;
            return;
        }
        Vector2 move = moveAction.ReadValue<Vector2>();
        moveInputX = move.x;

        if (Mathf.Abs(moveInputX) < minInputToMove || hitStunTimer > 0f)
        {
            moveInputX = 0f;
        }

        // 地面に立っているか調べる
        UpdateGrounded();

        // 移動開始・着地の SE
        HandleMoveStartSound();
        HandleLandSound();

        // 操作できない間は、向きの変更とジャンプをしない
        if (hitStunTimer > 0f)
        {
            return;
        }

        // 向きを更新して、見た目を反転
        UpdateFacing();

        // ジャンプボタンが押されたら跳ぶ
        TryJump();


        if (Mathf.Abs(moveInputX) < minInputToMove)
        {
            moveInputX = 0f;
        }

        UpdateFacing();

        if(hitStunTimer > 0f)
        {
            return;
        }

        UpdateGrounded();

        TryJump();
    }

    void FixedUpdate()
    {
        if(hitStunTimer <= 0f && !isDead)
        {
            Vector2 velocity = rigidBody2D.linearVelocity;

            velocity.x = moveInputX * moveSpeed;

            rigidBody2D.linearVelocity = velocity;
        }
        

        ApplyJumpGravity();

    }

    void UpdateFacing()
    {
        if(moveInputX > 0f)
        {
            facing = 1;
        }
        else if(moveInputX < 0f)
        {
            facing = -1;
        }

        spriteRenderer.flipX = facing < 0;
    }

    void UpdateGrounded()
    {

        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, GroundLayers) != null;

    }
    void TryJump()
    {
        if (!jumpAction.WasPressedThisFrame())
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        Vector2 velocity = rigidBody2D.linearVelocity;
        velocity.y = 2f * jumpHeight / riseTime;
        rigidBody2D.linearVelocity = velocity;


        PlayOneShot(jumpClip);
    }

    void OnDrawGizmosSelected()
    {

        if(groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

    }

    void ApplyJumpGravity()
    {
        float riseGravity = 2f * jumpHeight / (riseTime * riseTime);
        float fallGravity = 2f * jumpHeight / (fallTime * fallTime);

        float gravity = rigidBody2D.linearVelocity.y > 0f ? riseGravity : fallGravity;

        rigidBody2D.gravityScale = gravity / Mathf.Abs(Physics2D.gravity.y);

        Vector2 velocity = rigidBody2D.linearVelocity;
        if(velocity.y < -maxFallSpeed)
        {
            velocity.y = maxFallSpeed;
            rigidBody2D.linearVelocity = velocity;

        }
    }

    public void TakeDamege(int amount)
    {

        if (isDead)
        {
            return;
        }
        currentHp = Mathf.Max(currentHp - amount, 0);

        life.SetLife(currentHp);

        ApplyKnockback();

        if(currentHp <= 0)
        {
            Die();
        }

        else
        {
            PlayOneShot(damageClip);
        }

    }

    void ApplyKnockback()
    {
        // X に向き（右 = 1、左 = -1）を掛けて、向きに合わせた速度にする
        rigidBody2D.linearVelocity = new Vector2(knockbackVelocity.x * facing, knockbackVelocity.y);
        // 操作できない時間を始める
        hitStunTimer = hitStunTime;
    }

    public void Die()
    {
        isDead = true;

        Vector2 velocity = rigidBody2D.linearVelocity;
        velocity.x = 0f;
        rigidBody2D.linearVelocity = velocity;

        PlayOneShot(deathClip);
    }

    void PlayOneShot(AudioClip clip)
    {
       
        if (clip == null || audioSource == null)
        {
            return;
        }
        
        audioSource.PlayOneShot(clip);
    }
    
    void HandleMoveStartSound()
    {
        
        bool isMoving = isGrounded && Mathf.Abs(moveInputX) > minInputToMove;
        
        if (isMoving && !wasMoving)
        {
            PlayOneShot(moveStartClip);
        }
        
        wasMoving = isMoving;
    }
    
    void HandleLandSound()
    {
        
        if (!wasGrounded && isGrounded)
        {
            PlayOneShot(landClip);
        }
    }
}
