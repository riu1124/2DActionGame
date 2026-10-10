using UnityEngine;
// UnityのInputSystemを使用する
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーのスクリプト
/// </summary>
public class Player : MonoBehaviour
{
    [Header("移動スピード")]
    [SerializeField] float moveSpeed = 5f;
    [Header("ジャンプ -- 高さ")]
    [SerializeField] float jumpHeight = 3f;
    [Header("ジャンプ -- 上昇時間（頂点まで何秒か。小さいほど速く上がる）")]
    [SerializeField] float riseTime = 0.4f;
    [Header("ジャンプ -- 下降時間（頂点から同じ高さに戻るまで何秒か。小さいほど速く落ちる）")]
    [SerializeField] float fallTime = 0.3f;
    [Header("ジャンプ -- 落ちる速さの上限（高い所から落ちたとき用）")]
    [SerializeField] float maxFallSpeed = 20f;
    [Header("接地判定 -- 足元の位置")]
    [SerializeField] Transform groundCheck;
    [Header("接地判定 -- 足元の円の半径")]
    [SerializeField] float groundRadius = 0.12f;
    [Header("接地判定 -- 地面とみなすレイヤー")]
    [SerializeField] LayerMask groundLayers;
    [Header("HP -- 最大値")]
    [SerializeField] int maxHp = 3;
    [Header("HP -- ライフ表示")]
    [SerializeField] Life life;
    [Header("被弾 -- 飛ばされる速度（X+ = 向いている方向、X- = 真後ろ、Y+ = 上）")]
    [SerializeField] Vector2 knockbackVelocity = new Vector2(-4f, 6f);
    [Header("被弾 -- 操作できない時間（秒）")]
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

    // 死亡したかを外から読めるようにする（GameManager・Goal で使う）
    public bool IsDead => isDead;

    // これ未満の入力は無視（スティックのわずかな傾き対策）
    float minInputToMove = 0.2f;
    Rigidbody2D rigidBody2D;       // 物理特性（速度・重力など）を扱う変数
    SpriteRenderer spriteRenderer; // 見た目（左右反転に使う）
    InputAction moveAction;        // Player/Move の入力
    InputAction jumpAction;        // Player/Jump の入力
    float moveInputX;              // 左右の入力値（-1 ～ 1）
    int facing = 1;                // 向いている方向（1 = 右、-1 = 左）
    bool isGrounded;               // 地面に足がついているか
    int currentHp;                 // 今の HP
    float hitStunTimer;            // 操作できない残り時間（0 より大きい間は操作不可）
    bool isDead;                   // 死亡したか（true の間は移動・ジャンプしない）
    bool wasGrounded;              // 前のフレームで地面に足がついていたか（着地 SE 用）
    AudioSource audioSource;         // 効果音用のサウンド
    bool wasMoving;                // 前のフレームで動いていたか（移動開始 SE 用）
    Animator animator;             // アニメーションを切り替える部品
    string currentAnim = "Idle";   // 今のアニメーション名（同じ Trigger を何度も送らないため）

    /// <summary>
    /// オブジェクトが読み込まれたとき、呼ばれる
    /// </summary>
    void Awake()
    {
        // 物理特性を取得して保持
        rigidBody2D = GetComponent<Rigidbody2D>();

        // 見た目を取得して保持
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 効果音を鳴らす部品を取得して保持
        audioSource = GetComponent<AudioSource>();

        // アニメーションを切り替える部品を取得して保持
        animator = GetComponent<Animator>();

        // InputSystemを探して保持
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");

        // HP を最大にする
        currentHp = maxHp;
    }

    /// <summary>
    /// 最初のフレームの前に 1 回呼ばれる
    /// </summary>
    void Start()
    {
        // ライフ表示に、最大 HP の数だけハートを作らせる
        life.Setup(maxHp);
        life.SetLife(currentHp);
    }

    /// <summary>
    /// オブジェクトやスクリプトが有効になった時
    /// </summary>
    void OnEnable()
    {
        // 入力を受け付ける
        moveAction.Enable();
        jumpAction.Enable();
    }

    /// <summary>
    /// オブジェクトやスクリプトが無効・消えるときに呼ばれる。
    /// </summary>
    void OnDisable()
    {
        // 入力を止める
        moveAction.Disable();
        jumpAction.Disable();
    }

    /// <summary>
    /// 毎フレーム呼ばれる
    /// </summary>
    void Update()
    {
        // 操作できない時間を減らす
        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
        }

        // 死亡後は入力を受け付けない（移動・向き・ジャンプをしない）
        if (isDead)
        {
            moveInputX = 0f;
            return;
        }

        // InputSystemの左右キーの入力値を取得
        Vector2 move = moveAction.ReadValue<Vector2>();
        moveInputX = move.x;

        // 僅かな傾きは無視する（スティックのわずかな傾き対策）
        // Mathf.Absは絶対値を返す関数。
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
    }

    /// <summary>
    /// 全ての Update が終わった後に、毎フレーム呼ばれる
    /// </summary>
    void LateUpdate()
    {
        // 今の状態に合わせてアニメーションを切り替える
        UpdateAnimation();

        // 死亡アニメーションが終わったら非表示にする
        HideAfterDeath();
    }

    /// <summary>
    /// 一定間隔で、物理の計算の前に呼ばれる
    /// </summary>
    void FixedUpdate()
    {
        // 操作できる間だけ、横方向の速度を入力で決める
        // 操作できない間・死亡後は上書きしない
        if (hitStunTimer <= 0f && !isDead)
        {
            // 今の速度を取得
            Vector2 velocity = rigidBody2D.linearVelocity;
            // 横方向の速度を入力値に応じて設定
            velocity.x = moveInputX * moveSpeed;
            // 設定した速度を物理特性に反映
            rigidBody2D.linearVelocity = velocity;
        }

        // 上昇中・下降中で重力を切り替える
        ApplyJumpGravity();
    }

    /// <summary>
    /// 入力の向きに合わせて、スプライトを左右反転する
    /// </summary>
    void UpdateFacing()
    {
        // 右に入力したら右向き、左なら左向き
        // 入力が 0 のときは変えない（止まっても最後の向きのまま）
        if (moveInputX > 0f)
        {
            facing = 1;
        }
        else if (moveInputX < 0f)
        {
            facing = -1;
        }
        // flipX が true だと左右反転して表示される
        spriteRenderer.flipX = facing < 0;
    }

    /// <summary>
    /// 足元の円が地面レイヤーと重なっているかで、接地を調べる
    /// </summary>
    void UpdateGrounded()
    {
        // 前のフレームの状態を覚えておく（着地 SE 用）
        wasGrounded = isGrounded;

        // OverlapCircle は円と重なった Collider を返す（無ければ null）
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayers) != null;
    }

    /// <summary>
    /// 地面にいるときに、ジャンプボタンが押された瞬間だけ跳ぶ
    /// </summary>
    void TryJump()
    {
        // WasPressedThisFrame は押した瞬間だけ true（押しっぱなしでは連続で跳ばない）
        if (!jumpAction.WasPressedThisFrame())
        {
            return;
        }

        // 空中では跳べない
        if (!isGrounded)
        {
            return;
        }

        // 高さと上昇時間から初速を計算して、縦方向の速度に設定
        Vector2 velocity = rigidBody2D.linearVelocity;
        velocity.y = 2f * jumpHeight / riseTime;
        rigidBody2D.linearVelocity = velocity;

        // ジャンプ SE
        PlayOneShot(jumpClip);
    }

    /// <summary>
    /// Scene ビューで選んでいるとき、足元の円を緑で表示する
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }

    /// <summary>
    /// 上昇中は上昇時間に、下降中は下降時間に合う重力にする
    /// </summary>
    void ApplyJumpGravity()
    {
        // 上昇中と下降中の重力の強さを計算
        float riseGravity = 2f * jumpHeight / (riseTime * riseTime);
        float fallGravity = 2f * jumpHeight / (fallTime * fallTime);
        // 上に動いているときは上昇用、それ以外（下降・地上）は下降用
        float gravity = rigidBody2D.linearVelocity.y > 0f ? riseGravity : fallGravity;
        // Gravity Scale は「Unity の標準重力の何倍か」なので、標準重力で割って設定
        rigidBody2D.gravityScale = gravity / Mathf.Abs(Physics2D.gravity.y);

        // 落ちる速さが上限を超えないようにする
        Vector2 velocity = rigidBody2D.linearVelocity;
        if (velocity.y < -maxFallSpeed)
        {
            velocity.y = -maxFallSpeed;
            rigidBody2D.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// ダメージを受ける（トゲなどから呼ばれる）
    /// </summary>
    public void TakeDamage(int amount)
    {
        // 死亡後はダメージを受けない
        if (isDead)
        {
            return;
        }

        // HP を減らす（0 より下にはしない）
        currentHp = Mathf.Max(currentHp - amount, 0);

        // ライフ表示を今の HP に合わせる
        life.SetLife(currentHp);

        // 後ろに飛ばして、しばらく操作できなくする
        ApplyKnockback();

        // HP が 0 になったら死亡
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            PlayOneShot(damageClip);
        }
    }

    /// <summary>
    /// 向いている方向を基準に飛ばし、操作できない時間を始める
    /// </summary>
    void ApplyKnockback()
    {
        // X に向き（右 = 1、左 = -1）を掛けて、向きに合わせた速度にする
        rigidBody2D.linearVelocity = new Vector2(knockbackVelocity.x * facing, knockbackVelocity.y);
        // 操作できない時間を始める
        hitStunTimer = hitStunTime;
    }

    /// <summary>
    /// 死亡する（HP が 0 になったとき、落下したときなどに呼ぶ）
    /// </summary>
    public void Die()
    {
        // 死亡フラグを立てる（Update・FixedUpdate が移動とジャンプをしなくなる）
        isDead = true;

        // 横の勢いを止める（縦は重力のまま）
        Vector2 velocity = rigidBody2D.linearVelocity;
        velocity.x = 0f;
        rigidBody2D.linearVelocity = velocity;

        // 死亡 SE
        PlayOneShot(deathClip);
    }

    /// <summary>
    /// 効果音を 1 回鳴らす
    /// </summary>
    void PlayOneShot(AudioClip clip)
    {
        // 音か AudioSource が無ければ鳴らさない
        if (clip == null || audioSource == null)
        {
            return;
        }
        // PlayOneShot は今鳴っている音を止めずに重ねて鳴らす
        audioSource.PlayOneShot(clip);
    }
    /// <summary>
    /// 止まっていた状態から地上で動き出した瞬間だけ、SE を 1 回鳴らす
    /// </summary>
    void HandleMoveStartSound()
    {
        // 地上で、左右に入力しているか
        bool isMoving = isGrounded && Mathf.Abs(moveInputX) > minInputToMove;
        // 「前は止まっていて、今は動いている」瞬間だけ鳴らす
        if (isMoving && !wasMoving)
        {
            PlayOneShot(moveStartClip);
        }
        // 次のフレームのために今の状態を覚える
        wasMoving = isMoving;
    }
    /// <summary>
    /// 空中から地面についた瞬間に、着地 SE を鳴らす
    /// </summary>
    void HandleLandSound()
    {
        // 「前は空中で、今は地面」の瞬間だけ鳴らす
        if (!wasGrounded && isGrounded)
        {
            PlayOneShot(landClip);
        }
    }

    /// <summary>
    /// 状態からアニメーション名を決めて、変わったときだけ Trigger を送る
    /// </summary>
    void UpdateAnimation()
    {
        // 優先度の高い順に、次のアニメーションを決める
        string nextAnim;
        if (isDead)
        {
            nextAnim = "Death";
        }
        else if (hitStunTimer > 0f)
        {
            nextAnim = "Damage";
        }
        else if (!isGrounded)
        {
            nextAnim = "Jump";
        }
        else if (moveInputX != 0f)
        {
            nextAnim = "Move";
        }
        else
        {
            nextAnim = "Idle";
        }
        // 今と同じなら何もしない（毎フレーム送ると最初のコマに戻り続ける）
        if (nextAnim == currentAnim)
        {
            return;
        }
        // Trigger を送って切り替え、今のアニメーション名を覚える
        animator.SetTrigger(nextAnim);
        currentAnim = nextAnim;
    }

    /// <summary>
    /// 死亡アニメーションを最後まで再生したら、見た目を消す
    /// </summary>
    void HideAfterDeath()
    {
        // 死んでいない、またはもう消えているなら何もしない
        if (!isDead || !spriteRenderer.enabled)
        {
            return;
        }

        // 今再生中のアニメーションの情報を取得（0 は Base Layer）
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // normalizedTime は再生の進み具合（0 = 開始、1 = 最後まで再生）
        if (state.IsName("Death") && state.normalizedTime >= 1f)
        {
            spriteRenderer.enabled = false;
        }
    }
}
