using UnityEngine;

/// <summary>
/// ゴールのスクリプト（プレイヤーが触れたら「着いた」状態になる）
/// </summary>
public class Goal : MonoBehaviour
{
    // 着いたかを外から読めるようにpubilcにする（GameManager で使う）
    public bool isReached; // プレイヤーがゴールに着いたか

    /// <summary>
    /// Is Trigger の Collider に何かが入ったとき、呼ばれる
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 触れた相手から Player を探す（Player 以外なら null）
        Player player = other.GetComponent<Player>();

        // Player でない、または死亡しているなら何もしない
        if (player == null || player.IsDead)
        {
            return;
        }

        // ゴールに着いた
        isReached = true;
    }
}