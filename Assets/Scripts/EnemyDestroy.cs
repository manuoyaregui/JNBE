using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroy : MonoBehaviour
{
    [SerializeField] GameObject enemyFrag;
    int dropCoins = 1;
    public AudioClip EnemyDestroySound;
    // Start is called before the first frame update
    void Start()
    {
        dropCoins = GetComponent<EnemyController>().GetEnemyDropCoinsValue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "PlayerBullet")
        {
            GameManager.singletonGameManager.AddCoins(dropCoins);
            GameManager.singletonGameManager.PlaySound(EnemyDestroySound);
            Instantiate(enemyFrag, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
