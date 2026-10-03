using Unity.VisualScripting;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("与えるダメージ")]
    [SerializeField] int damage = 1;


    void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if(player == null)
        {
            return;
        }

        player.TakeDamege(damage);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
