using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainEnemy : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] private GameObject playerGameObject;

    [SerializeField] private float speed = 15f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per fram
    void Update()
    {
        Move();
        LookPlayer();
    }
    private void Move()
    {
        if(player.position.z - transform.position.z > 25)
        {
            speed = 35f;
        }
        else if(player.position.z - transform.position.z > 60)
        {
            speed = 80f;
        }
        else
        {
            speed = 17.5f;
        }
        transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
    }
    private void LookPlayer()
    {
        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerGameObject.GetComponent<PlayerController>().SetLives(0);
        }
    }
}
