using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShootWeapon : MonoBehaviour
{
    public Weapon GunSettings;
    [SerializeField] protected Transform barrel; //de donde salen las balas?
    [SerializeField] protected GameObject ammo;  //qu� balas usa?
    protected float rps; // Rondas por segundo del arma
    protected float timeBwShots; // Tiempo entre disparos
    protected int bulletsRemaining;
    protected int maxBulletDistance;
    protected float shootTime; //Tiempo de disparo
    public Animator anim;

    private Camera weaponCam;


    public static event Action<int, GameObject> onBulletsChange;

    protected GameObject player;
    [SerializeField] private ParticleSystem shootParticles;

    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip emptyMagazineClip;

    // Start is called before the first frame update
    void Start()
    {
        weaponCam = GetComponentInParent<Camera>();
        anim = GetComponent<Animator>();
        shootParticles.Pause();
        bulletsRemaining = GunSettings.initialBullets;
        maxBulletDistance = GunSettings.maxBulletDistance;
        player = GameObject.FindGameObjectWithTag("Player");
        rps = GunSettings.rpm / 60f; // Pasaje de Rondas por minuto a rondas por segundo
        timeBwShots = 1 / rps; // Dividir el 1 entre las rps, da el tiempo que pasa entre cada disparo

        onBulletsChange?.Invoke(bulletsRemaining,gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (!GameManager.singletonGameManager.GetPausedStatus() ){

            if (!GameManager.singletonGameManager.isInCinematic
                &&
                player.GetComponent<PlayerController>().isAlive //si est� vivo
                &&
                bulletsRemaining > 0) //y tiene al menos una bala
            {
                FireWeapon(); // puede disparar el arma
            }
            else
            {
                anim.SetBool("isShoot", false);
            }

            if (bulletsRemaining <= 0
               &&
               Input.GetButtonDown("Fire1")
               &&
               !GameManager.singletonGameManager.isInCinematic)
            {
                SfxManager._sfxManager.PlaySoundEffect(emptyMagazineClip);
            }
        }
    }

    protected virtual bool FireWeapon()
    {

        if (bulletsRemaining > 0 && Time.time > shootTime && Input.GetButtonDown("Fire1")) //Si el tiempo es mayor al tiempo de disparo
        {
            anim.SetBool("isShoot", true);
            SfxManager._sfxManager.PlaySoundEffect(shootClip);
            StartCoroutine(IShootParticle());

            RaycastHit hit;
            if( Physics.Raycast(weaponCam.transform.position, weaponCam.transform.forward, out hit, maxBulletDistance) ){
                if (hit.transform.CompareTag("Enemy"))
                {
                    hit.transform.GetComponent<EnemyDestroy>().KillEnemy();
                }
            }
            
            MinusBullets(); //Resto una Bala del cargador

            anim.SetBool("isShoot", true);

            return true;
        }
        else
        {
            anim.SetBool("isShoot", false);
            if(bulletsRemaining <= 0 && Input.GetButtonDown("Fire1"))
            {
                SfxManager._sfxManager.PlaySoundEffect(emptyMagazineClip);
            }
            return false;
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

    public void StopDashAnim()
    {
        anim.SetBool("isDashing", false);
    }
}