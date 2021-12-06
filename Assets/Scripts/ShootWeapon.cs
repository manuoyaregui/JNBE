using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShootWeapon : MonoBehaviour
{
    public Weapon GunSettings;
    [SerializeField] protected Transform barrel; //de donde salen las balas?
    [SerializeField] protected GameObject ammo;  //qué balas usa?
    protected float rps; // Rondas por segundo del arma
    protected float timeBwShots; // Tiempo entre disparos
    protected int bulletsRemaining;
    protected float shootTime; //Tiempo de disparo
    public static event Action<int> onBulletsChange;
    protected GameObject player;
    [SerializeField] private ParticleSystem shootParticles;
    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        shootParticles.Pause();
        bulletsRemaining = GunSettings.initialBullets;
        player = GameObject.FindGameObjectWithTag("Player");
        rps = GunSettings.rpm / 60f; // Pasaje de Rondas por minuto a rondas por segundo
        timeBwShots = 1 / rps; // Dividir el 1 entre las rps, da el tiempo que pasa entre cada disparo
        onBulletsChange?.Invoke(bulletsRemaining);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<PlayerController>().isAlive //si está vivo
            &&
            bulletsRemaining > 0) //y tiene al menos una bala
        {
            FireWeapon(); // puede disparar el arma

        }
    }

    protected virtual void FireWeapon()
    {
        if (Time.time > shootTime && Input.GetButtonDown("Fire1")) //Si el tiempo es mayor al tiempo de disparo
        {
            StartCoroutine(IShootParticle());
            GameObject newAmmo; //Nuevo GameObject para instanciar la bala

            newAmmo = Instantiate(ammo, barrel.position, barrel.rotation); //Se instancia la bala

            newAmmo.GetComponent<Rigidbody>().AddForce(barrel.forward * GunSettings.shotSpeed); //Se agrega fuerza al rigidbody para que la bala se mueva

            shootTime = Time.time + timeBwShots; // Variable para calcular la cadencia de disparo

            Destroy(newAmmo, GunSettings.bulletTime); // La bala es destruida 4 segundos despues de ser instanciada

            MinusBullets(); //Resto una Bala del cargador

        }
    }
    IEnumerator IShootParticle()
    {
        shootParticles.Play();
        yield return new WaitForSeconds(0.05f);
        shootParticles.Stop();
        yield return null;
    }
    public int GetBulletsRemaining()
    {
        return bulletsRemaining;
    }

    public void SetExtraBullets(int value)
    {
        Debug.Log("Mas balas lokooo");
        bulletsRemaining += value;
        onBulletsChange?.Invoke(bulletsRemaining);
    }

    public void EmptyCurrentBullets()
    {
        bulletsRemaining = 0;
    }

    private void MinusBullets()
    {
        bulletsRemaining--;
        onBulletsChange?.Invoke(bulletsRemaining);

    }

}
