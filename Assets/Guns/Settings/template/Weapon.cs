using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSettings", menuName = "WeaponSettings")]

public class Weapon : ScriptableObject
{
    public float bulletTime; //cuando vive la bala luego de ser creada
    public float shotSpeed; // Velocidad de disparo
    public int initialBullets; //Cuantas Balas le quedan al arma
    public float rpm; // Rondas por minuto del arma
    public int magazine;
    public int maxBulletDistance;
}
