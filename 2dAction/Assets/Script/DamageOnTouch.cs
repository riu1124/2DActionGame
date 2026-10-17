using UnityEngine;

/// <summary>
/// 触れたプレイヤーにダメージを与えるコンポーネント（Spike と同様の挙動）
/// プレハブに付けるか、Stone が生成時に自動で追加します。
/// </summary>
public class DamageOnTouch : MonoBehaviour
{
    [Header("与えるダメージ")]
    [SerializeField] int damage = 1;

    /// <summary>
    /// 他のスクリプトからダメージ量を設定するためのメソッド
    /// </summary>
    public void SetDamage(int amount)
    {
        damage = amount;
    }

    /// <summary>
    /// Is Trigger の Collider2D に他の Collider2D が入ったときに呼ばれる
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null) return;

        player.TakeDamage(damage);
    }

    /// <summary>
    /// Collider が IsTrigger でない場合に当たる Collider で呼ばれることがあるため
    /// こちらでも同じ処理を行う（どちらのケースでもダメージを与えられるようにする）。
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null) return;

        player.TakeDamage(damage);
    }
}
