using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MainEnemy : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] private GameObject playerGameObject;

    private float enemySpeed;
    [SerializeField] private float speed1 = 15f;
    [SerializeField] private float speed2 = 30f;
    [SerializeField] private float speed3 = 50f;


    [SerializeField] private GameObject playerBullet;
    [SerializeField] private GameObject enemyBullet;

    //[SerializeField] private AudioClip playerBeingEatedSound;
    [SerializeField] private PlayableDirector playerBeingEatedClip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per fram
    void Update()
    {
        if( ! GameManager.singletonGameManager.GetPausedStatus() )
        {
            Move();
            LookPlayer();
        }
    }
    private void Move()
    {
        float distance = player.position.z - transform.position.z;
        if ( distance > 25 && distance <= 60)
        {
            enemySpeed = speed1;
        }
        else if(distance > 60)
        {
            enemySpeed = speed3;
        }
        else
        {
            enemySpeed = speed1;
        }
        transform.Translate(new Vector3(0, 0, enemySpeed * Time.deltaTime));
    }
    private void LookPlayer()
    {
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerBeingEatedClip.Play();
            playerGameObject.GetComponent<PlayerController>().SetLives(0);
            //GameManager.singletonGameManager.PlaySound(playerBeingEatedSound);
        }
    }

    
}
