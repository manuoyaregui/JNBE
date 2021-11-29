using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Este script va unido al Player
public class PlayerPickUpGuns : MonoBehaviour
{
    [SerializeField] private GameObject[] listOfGuns;
    [SerializeField] private int extraBullets;
    private GameObject gun;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        gun = gameObject.GetComponent<PlayerPickUpGuns>().GetActiveGun();
        extraBullets = gun.GetComponent<ShootWeapon>().GunSettings.magazine;
    }

    public int GetNumberOfGuns()
    {
        return listOfGuns.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        //si colisiono con el PickUpGun
        if (other.CompareTag("PUgun"))
        {
            int numberOfGun = other.GetComponent<PickUpGunController>().GetTypeOfGun(); //chequeo que tipo de arma es
            if (listOfGuns[numberOfGun].activeSelf == true)
            {
                Debug.Log("el arma esta activa");
                gun.GetComponent<ShootWeapon>().SetExtraBullets(extraBullets);
            }
            else
            {
                //desactivo todas las armas
                foreach (GameObject gun in listOfGuns)
                {
                    gun.SetActive(false);
                }
                Debug.Log("cambio de arma");
                listOfGuns[numberOfGun].SetActive(true); //la activo en el player
                gun.GetComponent<ShootWeapon>().EmptyCurrentBullets();
                gun.GetComponent<ShootWeapon>().SetExtraBullets(extraBullets);

            }
            
            Destroy(other.gameObject); //y destruyo el PickUpGun
        }
    }

    public GameObject GetActiveGun()
    {
        foreach (GameObject gun in listOfGuns)
        {
            if(gun.activeSelf == true)
            {
                return gun;
            }
        }

        return null;
    }
}
