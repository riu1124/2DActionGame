using UnityEngine;

/// <summary>
/// 指定したレイヤー名の IsTrigger オブジェクトに触れたら自分(GameObject)を消すスクリプト
/// プレハブ側に付けておき、スポナーで生成されたオブジェクトが消えるようにします。
/// </summary>
public class DeleteOnLayer : MonoBehaviour
{
    [Header("消す対象のレイヤー名（Inspectorで変更可）")]
    [SerializeField] string deleteLayerName = "deleteObject"; // プロジェクトのレイヤー名に合わせる

    int deleteLayer = -1;

    void Awake()
    {
        // レイヤー名からレイヤー番号を取得して保持する
        deleteLayer = LayerMask.NameToLayer(deleteLayerName);
        // 指定したレイヤー名が存在しないと -1 が返るが、動作はレイヤー名比較でフォールバックするためここでは何もしない
    }

    /// <summary>
    /// IsTrigger の Collider2D に入ったときに呼ばれる
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 触れた相手のレイヤーが deleteLayer なら自分を消す
        string otherLayerName = LayerMask.LayerToName(other.gameObject.layer);

        if (deleteLayer != -1)
        {
            if (other.gameObject.layer == deleteLayer)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // deleteLayer が -1 の場合はレイヤー名で比較する（安全策）
            if (otherLayerName == deleteLayerName)
            {
                Destroy(gameObject);
            }
        }
    }
}
