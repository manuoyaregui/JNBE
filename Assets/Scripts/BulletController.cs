using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collObj = collision.gameObject;
        string tagColl = collObj.tag;

        switch (tagColl){
            case "Player":
                if(this.CompareTag("EnemyBullet")) //Si la bala es del enemigo, dañá al jugador
                {
                    collObj.GetComponent<PlayerController>().lives--;
                }
                Destroy(gameObject);
            break;
            case "Enemy":
                collObj.GetComponent<EnemyController>().lives--;
                Destroy(gameObject);
            break;
        }
        
    }
}