using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestroy : MonoBehaviour
{
    [SerializeField] GameObject enemyFrag;
    int dropCoins = 1;
    public AudioClip EnemyDestroySound;
    [SerializeField] int deathScoreAddition = 100;
    // Start is called before the first frame update
    void Start()
    {
        dropCoins = GetComponent<EnemyController>().GetEnemyDropCoinsValue();
    }
    private void OnTriggerEnter(Collider other)
    {
        /*if(other.tag == "PlayerBullet")
        {

            GameManager.singletonGameManager.EnemyKilledAction(deathScoreAddition);
            GameManager.singletonGameManager.AddCoins(dropCoins);

            SfxManager._sfxManager.PlaySoundEffect(EnemyDestroySound);
            Instantiate(enemyFrag, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }*/
    }

    public void KillEnemy()
    {
        GameManager.singletonGameManager.EnemyKilledAction(deathScoreAddition);
        GameManager.singletonGameManager.AddCoins(dropCoins);

        SfxManager._sfxManager.PlaySoundEffect(EnemyDestroySound);
        Instantiate(enemyFrag, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
