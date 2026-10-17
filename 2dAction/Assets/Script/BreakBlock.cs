using UnityEngine;

public class BreakBlock : MonoBehaviour
{
    // 破壊時の効果音を Inspector で割り当てる
    [Header("サウンド -- 破壊効果音")]
    [SerializeField] AudioClip breakSound;

    // Inspector でオンにするとアイテムを出す
    [Header("アイテム設定")]
    [SerializeField] bool spawnItem = false;          // チェックを入れるとアイテムを出す
    [SerializeField] GameObject itemPrefab = null;    // 出現させるアイテムのプレハブを割り当てる

    // このオブジェクトに AudioSource を付けている場合に備えて保持する（今回の実装では必須ではない）
    AudioSource audioSource;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Player タグのオブジェクトと衝突したときのみ処理する
        if (!collision.gameObject.CompareTag("Player")) return;

        // まずサウンドを再生する（null チェックを必ず行う）
        if (breakSound != null)
        {
            // PlayClipAtPoint は内部で一時的な GameObject を作って音を鳴らします。
            // そのためこのオブジェクトをすぐ Destroy しても音が止まりません。
            // （初心者向け補足）PlayClipAtPoint は簡単に「破壊と同時に音を鳴らす」用途に適しています。
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // アイテム出現フラグがオンで、プレハブが割り当てられていれば生成する
        if (spawnItem && itemPrefab != null)
        {
            // Instantiate はプレハブのコピーを作ります。
            // 第1引数: プレハブ、第2引数: 生成位置、第3引数: 生成時の回転
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        // ブロックを破壊（即時破壊）。音は PlayClipAtPoint によって継続再生されます。
        Destroy(gameObject);
    }

    void Start()
    {
        // AudioSource が同じオブジェクトに付いているなら取得しておく（将来的な拡張のため）
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // このスクリプトでは Update は使わないが、将来的に処理を追加する場合のために空メソッドとして残す
    }
}
