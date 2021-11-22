using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroy : MonoBehaviour
{
    [SerializeField] GameObject enemyFrag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "PlayerBullet")
        {
            Instantiate(enemyFrag, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
