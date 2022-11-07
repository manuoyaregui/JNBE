using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public AudioClip PlayerDamageSound;
    [SerializeField] private AudioClip bulletHitSound;
    private bool damage;
    // Start is called before the first frame update
    void Start()
    {
        damage = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ( GameManager.singletonGameManager.GetPausedStatus() )
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collObj = collision.gameObject;
        string tagColl = collObj.tag;
        SfxManager._sfxManager.PlaySoundEffect(bulletHitSound);
        if (damage)
        {
            switch (tagColl){
                case "Player": //Si colisione con Player
                    if(this.CompareTag("EnemyBullet")) //y la bala viene del enemigo...
                    {
                        SfxManager._sfxManager.PlaySoundEffect(PlayerDamageSound);
                        collObj.GetComponent<PlayerController>().MinusLives(); // sacale una vida al Player
                        damage = false;
                        Destroy(gameObject);
                    }
                break;

                case "Enemy": //Si colision� con un enemigo
                    collObj.GetComponent<EnemyController>().MinusLives(); // sacale una vida al enemigo
                    Destroy(gameObject);
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
}