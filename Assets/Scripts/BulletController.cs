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
            case "Player": //Si colisione con Player
                if(this.CompareTag("EnemyBullet")) //y la bala viene del enemigo...
                {
                    collObj.GetComponent<PlayerController>().MinusLives(); // sacale una vida al Player
                }
                Destroy(gameObject); // Destruir la bala
            break;
            case "Enemy": //Si colisioné con un enemigo
                collObj.GetComponent<EnemyController>().MinusLives(); // sacale una vida al enemigo
                Destroy(gameObject); // Destruir la bala
            break;
        }
        
    }
}