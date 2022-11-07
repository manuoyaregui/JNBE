using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ShielPUController : MonoBehaviour
{
    protected GameObject Player;

    public AudioClip PickUpShield;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * 50 * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerBullet")) // si choco o disparo, activar el escudo
        {
            SfxManager._sfxManager.PlaySoundEffect(PickUpShield);
            GiveShield();

            //TUTORIAL -- > si no estoy en el tutorial destruyo el pickup
            if (!SceneManager.GetSceneByName("Tutorial").Equals(SceneManager.GetActiveScene()))
                Destroy(gameObject); 
        }
    }


    //POSIBLE CAMBIO: al tomar el superCubo, el jugador obtiene el doble de puntos por 'x' tiempo,
    //si agarra otro, se reinicia el timer y se duplica nuevamente, indefinidamente.
    private void GiveShield()
    {
        GameManager.singletonGameManager.playerPickedUpASuperCube();
    }
}
