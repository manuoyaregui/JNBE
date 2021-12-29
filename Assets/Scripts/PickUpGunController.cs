using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpGunController : MonoBehaviour
{
    private int typeofGun;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        RerollNumber(); // Defino aleatoriamente
                                                                                                // que arma va a dar el PickUpGun 
    }
    // Update is called once per frame
    void Update()
    {

    }

    public int GetTypeOfGun()
    {
        return typeofGun;
    }

    public void RerollNumber()
    {
        typeofGun = Random.Range(0, player.GetComponent<PlayerPickUpGuns>().GetNumberOfGuns());
    }
}
