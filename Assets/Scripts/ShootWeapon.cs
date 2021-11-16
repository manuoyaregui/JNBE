using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWeapon : MonoBehaviour
{
    [SerializeField] Transform barrel;

    [SerializeField] GameObject ammo;

    [SerializeField] float shotSpeed = 1500f;

    float rpm = 500f;

    float rps;

    float timeBwShots;

    float shootTime;


    // Start is called before the first frame update
    void Start()
    {
        rps = rpm / 60f;
        timeBwShots = 1 / rps;
    }

    // Update is called once per frame
    void Update()
    {
        FireWeapon();
    }
    void FireWeapon()
    {
        if (Time.time > shootTime && Input.GetButtonDown("Fire1"))
        {
            GameObject newAmmo;

            newAmmo = Instantiate(ammo, barrel.position, barrel.rotation);

            newAmmo.GetComponent<Rigidbody>().AddForce(barrel.forward*shotSpeed);

            shootTime = Time.time + timeBwShots;

            Destroy(newAmmo, 4f);
        }
    }
}
