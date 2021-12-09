using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShotgunController : ShootWeapon
{

    CharacterController playerCharacController;
    [SerializeField] private float recoilTime;
    [SerializeField] private float recoilSpeed;
    public static event Action<float,float> OnShotgunRecoil;

    protected override bool FireWeapon()
    {
        if (base.FireWeapon())
        {
            Debug.Log("Estoy en el fire weapon");
            ShotgunRecoil();
        }
        return false;
    }

    private void ShotgunRecoil()
    {
        OnShotgunRecoil?.Invoke(recoilTime,recoilSpeed);   
    }

    
}
