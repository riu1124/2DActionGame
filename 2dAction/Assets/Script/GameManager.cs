using UnityEngine;
// シーンの読み直し（リトライ）に使用する
using UnityEngine.SceneManagement;
// ボタン（Button）を使用する
using UnityEngine.UI;
// 選択中の UI を管理する EventSystem を使用する
using UnityEngine.EventSystems;

/// <summary>
/// ゲームの流れ（プレイ中 / クリア / ゲームオーバー）を管理するスクリプト
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("参照 -- プレイヤー")]
    [SerializeField] Player player;
    [Header("参照 -- ゴール（触れたらクリア）")]
    [SerializeField] Goal goal;
    [Header("UI -- ゲームオーバーパネル")]
    [SerializeField] GameObject gameOverPanel;
    [Header("UI -- クリアパネル")]
    [SerializeField] GameObject clearPanel;
    [Header("UI -- リトライボタン（各パネルのボタンをすべて入れる）")]
    [SerializeField] Button[] retryButtons;
    [Header("演出 -- 死亡からパネルを出すまでの秒数（死亡アニメを見せる）")]
    [SerializeField] float gameOverDelay = 1f;

    bool isEnded; // クリアかゲームオーバーが決まったか（二重に終わらないため）

    /// <summary>
    /// 最初のフレームの前に 1 回呼ばれる
    /// </summary>
    void Start()
    {
        // 最初はパネルを隠す
        gameOverPanel.SetActive(false);
        clearPanel.SetActive(false);

        // どのリトライボタンを押しても Retry を呼ぶように登録
        foreach (Button button in retryButtons)
        {
            button.onClick.AddListener(Retry);
        }
    }

    /// <summary>
    /// 毎フレーム呼ばれる
    /// </summary>
    void Update()
    {
        // もう終わっているなら何もしない
        if (isEnded)
        {
            return;
        }

        // プレイヤーが死亡したらゲームオーバー
        if (player.IsDead)
        {
            GameOver();
            return;
        }

        // ゴールに着いたらクリア
        if (goal.isReached)
        {
            Clear();
        }
    }

    /// <summary>
    /// クリアにして、クリアパネルを出す
    /// </summary>
    void Clear()
    {
        isEnded = true;
        // プレイヤーを止める
        StopPlayer();
        // クリアパネルを表示
        clearPanel.SetActive(true);
        // リトライボタンを選択して、決定キーで押せるようにする
        SelectButton(clearPanel);
    }

    /// <summary>
    /// ゲームオーバーにして、少し待ってからパネルを出す
    /// </summary>
    void GameOver()
    {
        isEnded = true;
        // Invoke は指定した秒数あとに関数を呼ぶ（死亡アニメを見せるため）
        Invoke(nameof(ShowGameOverPanel), gameOverDelay);
    }

    /// <summary>
    /// ゲームオーバーパネルを表示する
    /// </summary>
    void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);

        // リトライボタンを選択して、決定キーで押せるようにする
        SelectButton(gameOverPanel);
    }

    /// <summary>
    /// 操作を止め、横方向の勢いも消す（落下は重力のまま）
    /// </summary>
    void StopPlayer()
    {
        // Player スクリプトを止める（Update・FixedUpdate が呼ばれなくなる）
        player.enabled = false;
        // 物理特性を取得して、横の速度だけ 0 にする
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
    }

    /// <summary>
    /// 今のシーンを読み直して、最初からやり直す
    /// </summary>
    void Retry()
    {
        // 今のシーンの名前で読み直す
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// パネルの中のボタンを選択状態にする（決定キーで押せるようにする）
    /// </summary>
    void SelectButton(GameObject panel)
    {
        // パネルの子から最初のボタンを探す
        Button button = panel.GetComponentInChildren<Button>();
        // そのボタンを選択中にする
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}

