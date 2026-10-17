using UnityEngine;

// アイテム系プレハブに付けるスクリプト
// プレイヤーが触れたら指定した効果（ここでは体力回復）を与えてアイテムを消します。
public class ItemPickup : MonoBehaviour
{
    [Header("アイテム設定")]
    [SerializeField] int healAmount = 1; // 回復量（Inspectorで変更可）
    [SerializeField] AudioClip pickupSound = null; // 取得時に鳴らす音（任意）

    // 当たり判定は Trigger にして使う想定です（Collider2D の IsTrigger をオンにしてください）
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player タグのオブジェクトと当たったら処理
        if (!other.CompareTag("Player")) return;

        // Player のスクリプトを取得して Heal を呼ぶ
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.Heal(healAmount);
        }

        // 取得音が設定されていたら再生（このオブジェクトの位置で鳴らす）
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // アイテムを消す
        Destroy(gameObject);
    }
}
