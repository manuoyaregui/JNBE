using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShielPUController : MonoBehaviour
{
    private GameObject Player;
    [SerializeField] private GameObject ShieldIndicator;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("entre al colission enter");
        ShieldIndicator.SetActive(true);
        Player.GetComponent<PlayerController>().lives = 2;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerBullet"))
        {
            ShieldIndicator.SetActive(true);
            Player.GetComponent<PlayerController>().lives = 2;
            Destroy(gameObject);
        }
    }
}
