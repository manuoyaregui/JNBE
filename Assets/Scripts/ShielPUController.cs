using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShielPUController : MonoBehaviour
{
    private GameObject Player;
    public static event Action<int> OnShieldPickedUp;

    public AudioClip PickUpShield;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerBullet")) // si choco o disparo, activar el escudo
        {
            GameManager.singletonGameManager.PlaySound(PickUpShield);
            GiveShield();
        }
    }

    private void GiveShield()
    {
        OnShieldPickedUp?.Invoke(2);
        Player.GetComponent<PlayerController>().SetLives(2); // cambio vida de player a 2, valor el cual pone el escudo activo
        Destroy(gameObject);
    }
}
