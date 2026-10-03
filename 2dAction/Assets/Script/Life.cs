using Unity.VisualScripting;
using UnityEngine;

public class Life : MonoBehaviour
{

    [Header("ハート一個分のprefab(UIのImage)")]
    [SerializeField] GameObject heartPrefab;

    GameObject[] lifeObjects;


    public void Setup(int maxLife)
    {
        lifeObjects = new GameObject[maxLife];
        
        for (int i = 0; i < lifeObjects.Length; i++ )
        {
            lifeObjects[i] = Instantiate(heartPrefab, transform);
        }
    }

    public void SetLife(int currentLife)
    {
        for (int  i = 0; i < lifeObjects.Length; i++)
        {
            lifeObjects[i].SetActive(i < currentLife);
        }
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
