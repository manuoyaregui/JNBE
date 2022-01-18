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
    public Animator anim;


    public static event Action<int, GameObject> onBulletsChange;

    protected GameObject player;
    [SerializeField] private ParticleSystem shootParticles;

    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip emptyMagazineClip;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        shootParticles.Pause();
        bulletsRemaining = GunSettings.initialBullets;
        player = GameObject.FindGameObjectWithTag("Player");
        rps = GunSettings.rpm / 60f; // Pasaje de Rondas por minuto a rondas por segundo
        timeBwShots = 1 / rps; // Dividir el 1 entre las rps, da el tiempo que pasa entre cada disparo

        onBulletsChange?.Invoke(bulletsRemaining,gameObject);
    }

    // Update is called once per frame
    void Update()
    {
            if (HUDController.isPause == false
                &&
                player.GetComponent<PlayerController>().isAlive //si está vivo
                &&
                bulletsRemaining > 0) //y tiene al menos una bala
            {
                FireWeapon(); // puede disparar el arma
            }
            else
            {
            anim.SetBool("isShoot", false);
            }

            if(HUDController.isPause == false
               && 
               bulletsRemaining == 0 
               && 
               Input.GetButtonDown("Fire1"))
            {
                GameManager.singletonGameManager.PlaySound(emptyMagazineClip);
            }
    }

    protected virtual bool FireWeapon()
    {

        if (bulletsRemaining > 0 && Time.time > shootTime && Input.GetButtonDown("Fire1")) //Si el tiempo es mayor al tiempo de disparo
        {
            anim.SetBool("isShoot", true);
            GameManager.singletonGameManager.PlaySound(shootClip);
            StartCoroutine(IShootParticle());
            GameObject newAmmo; //Nuevo GameObject para instanciar la bala

            newAmmo = Instantiate(ammo, barrel.position, barrel.rotation); //Se instancia la bala

            newAmmo.GetComponent<Rigidbody>().AddForce(barrel.forward * GunSettings.shotSpeed); //Se agrega fuerza al rigidbody para que la bala se mueva

            shootTime = Time.time + timeBwShots; // Variable para calcular la cadencia de disparo

            Destroy(newAmmo, GunSettings.bulletTime); // La bala es destruida 4 segundos despues de ser instanciada

            MinusBullets(); //Resto una Bala del cargador

            anim.SetBool("isShoot", true);

            return true;
        }
        else
        {
            anim.SetBool("isShoot", false);
            return false;
            if(bulletsRemaining <= 0 && Input.GetButtonDown("Fire1"))
            {
                GameManager.singletonGameManager.PlaySound(emptyMagazineClip);
            }
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

    public void SetExtraBullets()
    {
        bulletsRemaining += GunSettings.magazine;
        if (gameObject.activeSelf) //si el arma esta en mano lanza el evento
        {
            onBulletsChange?.Invoke(bulletsRemaining,gameObject);
        }
    }

    public void EmptyCurrentBullets()
    {
        bulletsRemaining = 0;
    }

    private void MinusBullets()
    {
        bulletsRemaining--;
        onBulletsChange?.Invoke(bulletsRemaining,gameObject);

    }

    private void OnEnable()
    {
        shootParticles.Pause();
    }

}
