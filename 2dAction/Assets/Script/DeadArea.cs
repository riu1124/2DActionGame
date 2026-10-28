using UnityEngine;

/// <summary>
/// Dead レイヤーのオブジェクトに触れたプレイヤーを即死させるスクリプト
/// </summary>
public class DeadArea : MonoBehaviour
{
    /// <summary>
    /// Is Trigger の Collider2D にプレイヤーが入ったときに呼ばれる
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 触れた相手から Player スクリプトを探す
        Player player = other.GetComponent<Player>();

        // Player でなければ何もしない
        if (player == null)
        {
            return;
        }

        // 1000 ダメージを与えて、体力を 0 にする
        player.TakeDamage(1000);
    }

    /// <summary>
    /// Is Trigger がオフの Collider2D にプレイヤーが当たったときに呼ばれる
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 当たった相手から Player スクリプトを探す
        Player player = collision.gameObject.GetComponent<Player>();

        // Player でなければ何もしない
        if (player == null)
        {
            return;
        }

        // 1000 ダメージを与えて、体力を 0 にする
        player.TakeDamage(1000);
    }
}
