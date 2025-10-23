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
    [SerializeField] Animator playerModelAnim;

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
        Debug.Log("PlayerIsDashing called");
        
        _particles.DashParticles();

        _sounds.dash();

        // Asegurar que los animadores de las armas estén asignados y establecer el parámetro
        if (pistolAnim != null)
        {
            Debug.Log("Setting pistolAnim isDashing to true");
            pistolAnim.SetBool("isDashing", true);
            // Forzar la actualización del animador
            pistolAnim.Update(0);
        }
        else
        {
            Debug.LogWarning("pistolAnim is null! Make sure to assign it in the inspector.");
        }
        
        if (shotgunAnim != null)
        {
            Debug.Log("Setting shotgunAnim isDashing to true");
            shotgunAnim.SetBool("isDashing", true);
            // Forzar la actualización del animador
            shotgunAnim.Update(0);
        }
        else
        {
            Debug.LogWarning("shotgunAnim is null! Make sure to assign it in the inspector.");
        }
        
        // Controlar la animación del modelo del jugador
        if (playerModelAnim != null)
        {
            Debug.Log("Setting playerModelAnim isDashing to true");
            playerModelAnim.SetBool("isDashing", true);
            // Forzar la actualización del animador
            playerModelAnim.Update(0);
        }
        else
        {
            Debug.LogWarning("playerModelAnim is null! Make sure to assign it in the inspector.");
        }
    }

    public void PlayerGotAShield()
    {
        _particles.ShieldActivatedParticles();
        lives = 2;
    }
    public void PlayerLostAShield()
    {
        _particles.ShieldDisabledParticles();
    }

    public void ExtraBulletsPS()
    {
        _particles.ExtraBulletParticles();
    }

    /// <summary>
    /// Resetea la animación de dash en todos los animadores
    /// </summary>
    public void StopDashAnimation()
    {
        Debug.Log("StopDashAnimation called");
        
        // Resetear animadores de las armas
        if (pistolAnim != null)
        {
            Debug.Log("Setting pistolAnim isDashing to false");
            pistolAnim.SetBool("isDashing", false);
            // Forzar la actualización del animador
            pistolAnim.Update(0);
        }
        else
        {
            Debug.LogWarning("pistolAnim is null in StopDashAnimation!");
        }
        
        if (shotgunAnim != null)
        {
            Debug.Log("Setting shotgunAnim isDashing to false");
            shotgunAnim.SetBool("isDashing", false);
            // Forzar la actualización del animador
            shotgunAnim.Update(0);
        }
        else
        {
            Debug.LogWarning("shotgunAnim is null in StopDashAnimation!");
        }
        
        // Resetear la animación del modelo del jugador
        if (playerModelAnim != null)
        {
            Debug.Log("Setting playerModelAnim isDashing to false");
            playerModelAnim.SetBool("isDashing", false);
            // Forzar la actualización del animador
            playerModelAnim.Update(0);
        }
        else
        {
            Debug.LogWarning("playerModelAnim is null in StopDashAnimation!");
        }
    }


}
