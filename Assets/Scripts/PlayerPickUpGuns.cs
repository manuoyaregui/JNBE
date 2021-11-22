using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickUpGuns : MonoBehaviour
{
    [SerializeField] private GameObject[] listOfGuns;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetNumberOfGuns()
    {
        return listOfGuns.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        //si colisiono con el pick up gun
        if (other.CompareTag("PUgun"))
        {
            //desactivo todas las armas
            foreach (GameObject gun in listOfGuns)
            {
                gun.SetActive(false);
            }
            Debug.Log("Choque con el PUgun");
            //chequeo que arma es
            int numberOfGun = other.GetComponent<PickUpGunController>().GetTypeOfGun();
            listOfGuns[numberOfGun].SetActive(true);
            Destroy(other.gameObject);
        }
    }
}
