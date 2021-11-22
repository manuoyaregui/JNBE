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
        typeofGun = Random.Range(1, player.GetComponent<PlayerPickUpGuns>().GetNumberOfGuns()-1);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetTypeOfGun()
    {
        return typeofGun;
    }
}
