using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    //Este es el script central del player, contiene los stats
    internal PlayerPhysicsController _physics;
    internal PlayerParticlesController _particles;
    internal PlayerSoundController _sound;

    [Header("Settings")]
    public Player playerSettings;


    [Header("Sounds")]
    public AudioClip JumpSound;
    public AudioClip DobleJumpSound;
    public AudioClip DashSound;
    public AudioClip killZoneDeath;

    [Header("Particles")]
    [SerializeField] private ParticleSystem InertiaParticles;
    [SerializeField] private ParticleSystem shieldActivated;
    [SerializeField] private ParticleSystem shieldDisabled;
    [SerializeField] private ParticleSystem extraBulletParticles;


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
        //Esto NO debería estar acá (no deberia estar directamente jajaja)
        Application.targetFrameRate = 60; //Capear los fps en 60


        PlayerPickUpGuns.OnExtraBullets += ExtraBulletsPS; //Evento
        ShielPUController.OnShieldPickedUp += ActivateShieldParticleSystem;
    }

    void Start()
    {
        _physics = GetComponent<PlayerPhysicsController>();
        //_particles = GetComponent<PlayerParticlesController>();


        //getSettings
        lives = playerSettings.lives;
        isAlive = true;
        onLivesChange?.Invoke(lives);
        InertiaParticles.Stop();
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
            DisableShieldParticleSystem();
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
        ShielPUController.OnShieldPickedUp -= ActivateShieldParticleSystem;
        PlayerPickUpGuns.OnExtraBullets -= ExtraBulletsPS;
    }

    public void ActivateShieldParticleSystem(int value)
    {
        if(lives == 1) //Sólo si no tenía el escudo activar las particulas
            shieldActivated.Play();
    }

    public void DisableShieldParticleSystem()
    {
        shieldDisabled.Play();
    }

    public void ExtraBulletsPS() 
    {
        extraBulletParticles.Play();
    }


    //Actions Called By other Scripts


    bool alreadyInKillZone = false;
    public void PlayerTouchedTheKillZone()
    {
        lives = 0;
        isAlive = false;

        if (alreadyInKillZone == false)
        {
            GameManager.singletonGameManager.PlaySound(killZoneDeath);
            alreadyInKillZone = true;
        }

        onLivesChange?.Invoke(lives);
    }


    public void PlayerInertiaAltered(float value)
    {
        onInertiaChange?.Invoke(value);
    }
    public void PlayerHasHighInertia()
    {
        InertiaParticles.Play();
    }
    public void PlayerDoesntHasHighInertia()
    {
        InertiaParticles.Pause();
    }


    public void PlayerIsJumping()
    {
        if(JumpSound != null)
            GameManager.singletonGameManager.PlaySound(JumpSound);
    }

    public void PlayerIsDoubleJumping()
    {
        if (DobleJumpSound != null)
            GameManager.singletonGameManager.PlaySound(DobleJumpSound);
    }

    public void PlayerIsDashing()
    {
        GameManager.singletonGameManager.PlaySound(DashSound);
        pistolAnim.SetBool("isDashing", true);
        shotgunAnim.SetBool("isDashing", true);
    }

}
