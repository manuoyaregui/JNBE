using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public AudioClip PlayerDamageSound;
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
                    GameManager.singletonGameManager.PlaySound(PlayerDamageSound);
                    collObj.GetComponent<PlayerController>().MinusLives(); // sacale una vida al Player
                }
                 // Destruir la bala
            break;
            case "Enemy": //Si colisioné con un enemigo
                collObj.GetComponent<EnemyController>().MinusLives(); // sacale una vida al enemigo
                 // Destruir la bala
            break;
            default:
                if (this.CompareTag("EnemyBullet"))
                {
                    Destroy(gameObject);
                }
                break;
        }
        

    }
}