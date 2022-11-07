using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class ShielPUController : MonoBehaviour
{
    protected GameObject Player;
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
        transform.Rotate (Vector3.up * 50 * Time.deltaTime, Space.World);

    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerBullet")) // si choco o disparo, activar el escudo
        {
            SfxManager._sfxManager.PlaySoundEffect(PickUpShield);
            GiveShield();
            if (!SceneManager.GetSceneByName("Tutorial").Equals(SceneManager.GetActiveScene()))
                Destroy(gameObject); //si no estoy en el tutorial destruyo el pickup
        }
    }

    private void GiveShield()
    {
        OnShieldPickedUp?.Invoke(2);
    }
}
