using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Este script va unido al Player
public class PlayerPickUpGuns : MonoBehaviour
{
    [SerializeField] private GameObject[] listOfGuns;

    private GameObject gun;

    public static event Action<GameObject> onGunChange;
    public static event Action OnExtraBullets;

    public AudioClip ChangeWeapon;

    public AudioClip PickUp;

    private void Awake()
    {
        gun = GetActiveGun();
        onGunChange?.Invoke(gun);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        gun = GetActiveGun();
        if(HUDController.isPause == false)
            SwitchWeapons();
    }

    private void SwitchWeapons()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.singletonGameManager.PlaySound(ChangeWeapon);
            //obtener el nro de arma activa
            int currentWeapon = 0;
            for (int i=0 ; i < listOfGuns.Length; i++)
            {
                if(listOfGuns[i] != null && listOfGuns[i].activeSelf == true)
                {
                    currentWeapon = i;
                }
            }
            //desactivar todas las armas
            foreach(GameObject gun in listOfGuns)
            {
                gun.SetActive(false);
            }
            //activar arma con el nro siguiente
            //si paso el largo del array volver al inicio
            if(currentWeapon + 1 >= listOfGuns.Length)
            {
                listOfGuns[0].SetActive(true);
                onGunChange?.Invoke(listOfGuns[0]);
            }
            else
            {
                listOfGuns[currentWeapon + 1].SetActive(true);
                onGunChange?.Invoke(listOfGuns[currentWeapon + 1]);
            }
            
            
        }
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
            GameManager.singletonGameManager.PlaySound(PickUp);
            int numberOfGun = other.GetComponent<PickUpGunController>().GetTypeOfGun(); //chequeo que tipo de arma es
            listOfGuns[numberOfGun].GetComponent<ShootWeapon>().SetExtraBullets(); //y le agrego balas
            OnExtraBullets?.Invoke();
            Destroy(other.gameObject); // destruyo el pickup
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
