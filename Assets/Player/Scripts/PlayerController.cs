using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    //Este es el script central del player, contiene los stats
    private PlayerPhysicsController _physics;
    private PlayerParticlesController _particles;
    private PlayerSoundController _sounds;
    

    [Header("Settings")]
    public Player playerSettings;


    [Header("Sounds")]
    public AudioClip JumpSound;
    public AudioClip DobleJumpSound;
    public AudioClip DashSound;
    public AudioClip killZoneDeath;

    [SerializeField] Animator pistolAnim;
    [SerializeField] Animator shotgunAnim;

    //eventos
    public static event Action<int> onLivesChange;
    public static event Action<float> onInertiaChange;
    [SerializeField] private UnityEvent OnDeathUnityEvent;

    private int lives = 1; //Vidas del jugador

    // Flags
    public bool isAlive;
    private static float inertia;
    public bool resetCamera = false;


    public bool IsPlayerAlive()
    {
        return isAlive;
    }

    private void Awake()
    {


        PlayerPickUpGuns.OnExtraBullets += ExtraBulletsPS; //Evento
        ShielPUController.OnShieldPickedUp += PlayerGotAShield;
    }

    void Start()
    {
        _physics = GetComponent<PlayerPhysicsController>();
        _particles = GetComponent<PlayerParticlesController>();
        _sounds = GetComponent<PlayerSoundController>();


        //getSettings
        lives = playerSettings.lives;
        isAlive = true;
        onLivesChange?.Invoke(lives);
        onInertiaChange?.Invoke(inertia);

    }

    // Update is called once per frame
    void Update()
    {
        CheckShield();
    }


    public int getPlayerLives()
    {
        return lives;
    }
    public void MinusLives()
    {
        if(lives == 2)
        {
            PlayerLostAShield();
        }

        lives--;
        //lanzar evento
        onLivesChange?.Invoke(lives);
    }

    public void SetLives(int value)
    {
        lives = value;
    }


    bool alreadyDeath;
    private void CheckShield()
    {
        switch (lives)
        {
            case 0:
                isAlive = false;
                if (!alreadyDeath)
                {
                    OnDeathUnityEvent?.Invoke();
                    alreadyDeath = true;
                }
                break;
            case 1:
                //ShieldIndicator.SetActive(false); -- debe ser cambiado por un elemento de la interfaz
                break;
            case 2:
                //ShieldIndicator.SetActive(true); -- debe ser cambiado por un elemento de la interfaz
                break;
            default:
                lives = 0;
                break;
        }
    }

    

    public static float GetInertia()
    {
        return inertia;
    }

    public void ReCheckMouseSensibility()
    {
        _physics.ChangeMousesensibility(playerSettings.mouseSensibility);
    }

    private void OnDestroy()
    {
        ShielPUController.OnShieldPickedUp -= PlayerGotAShield;
        PlayerPickUpGuns.OnExtraBullets -= ExtraBulletsPS;
    }

    


    //Actions Called By other Scripts


    bool alreadyInKillZone = false;
    public void PlayerTouchedTheKillZone()
    {
        lives = 0;
        isAlive = false;

        if (alreadyInKillZone == false)
        {
            _sounds.killZoneDeathSound();
            
            alreadyInKillZone = true;
        }

        onLivesChange?.Invoke(lives);
    }


    public void PlayerInertiaAltered(float value)
    {
        onInertiaChange?.Invoke(value);
    }
    public void PlayerHaveHighInertia()
    {
        _particles.PlayInertiaParticles();
    }
    public void PlayerDoesntHaveHighInertia()
    {
        _particles.StopInertiaParticles();
    }


    public void PlayerIsJumping()
    {
        _sounds.jump();
    }
    public void PlayerIsDoubleJumping()
    {
        _sounds.doubleJump();
        
    }
    public void PlayerIsDashing()
    {
        _particles.DashParticles();

        _sounds.dash();

        pistolAnim.SetBool("isDashing", true);
        shotgunAnim.SetBool("isDashing", true);
    }

    public void PlayerGotAShield(int value)
    {
        if (lives == 1) //Sólo si no tenía el escudo activar las particulas
        {
            _particles.ShieldActivatedParticles();
            lives = value;
        }

    }
    public void PlayerLostAShield()
    {
        _particles.ShieldDisabledParticles();
    }

    public void ExtraBulletsPS()
    {
        _particles.ExtraBulletParticles();
    }


}
