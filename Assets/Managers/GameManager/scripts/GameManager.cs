using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour //Por ahora en desuso, solo se uso para cumplir un desafio
{
    private int lives; //Cantidad de vidas, sin uso actualmente
    

    public static GameManager singletonGameManager;
    private SfxManager _sfx_;
    private PlayerController _playerController;
    [SerializeField] private PostGlobalController _postProcc_;

    int pUScoreMultiplier = 1;
    

    public static bool isPaused=false;

    [NonSerialized] public int coinsGrabbed;
    [SerializeField] public bool isInCinematic = true; // Si está en true, bloquea el movimiento del player. Cambiar a false para permitir movimiento.
    [NonSerialized] public bool isTutorialFinished;
    
    [Header("Debug Settings")]
    [SerializeField] private bool disablePlayerShooting = false;

    [SerializeField] AudioClip onOneCoinGrabbedSfx;
    [SerializeField] AudioClip onALotOfCoinsGrabbedSfx;


    HUDController _hud;

    // Start is called before the first frame update
    void Start()
    {
        UnPauseTheGame();
        singletonGameManager = this;
        _sfx_ = SfxManager._sfxManager;
        _hud = GameObject.FindGameObjectWithTag("HUD")?.GetComponent<HUDController>();
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        
        // Asegurar que el cursor esté bloqueado al inicio del juego
        InitializeCursorState();
    }
    
    /// <summary>
    /// Inicializa el estado del cursor al comenzar el juego
    /// </summary>
    private void InitializeCursorState()
    {
        if (!isPaused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        CheckIfItPaused();
        TimerCubePU();
    }


    //flags
    [SerializeField] float PU_ScoreMultiplier_Duration = 5f;
    float currentTimerCubePU;
    private void TimerCubePU()
    {
        if(pUScoreMultiplier > 1)
        {
            if (currentTimerCubePU > 0)
            {
                currentTimerCubePU -= Time.deltaTime;
            }
            else
            {
                pUScoreMultiplier = 1;
                PUScoreMultiplierHasEnded();
            }
        }
    }

    private void PUScoreMultiplierHasEnded()
    {
        _hud.PlayerLostShieldPU(pUScoreMultiplier);
    }

    public void playerPickedUpASuperCube()
    {
        _playerController.PlayerGotAShield();

        pUScoreMultiplier *= 2;
        _hud.PlayerGotShieldPU(pUScoreMultiplier);

        currentTimerCubePU = PU_ScoreMultiplier_Duration;
    }

    public void PauseTheGame()
    {
        isPaused = true;
    }

    public void UnPauseTheGame()
    {
        isPaused = false;
    }

    public void ToggleIsPause()
    {
        isPaused = !isPaused;
    }

    private void CheckIfItPaused()
    {
        if (isPaused)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; // Desbloquear cursor en pausa
        }
        else
        {
            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // Bloquear cursor durante el juego
        }
    }

    

    public bool GetPausedStatus()
    {
        return isPaused;
    }


    public void AddLives()
    {
        lives++;
    }
    public void RestLives()
    {
        lives--;
    }
    

    public void AddCoins(int value)
    {
        /*coinsGrabbed += value;

        if(value <= 1)
        {
            _sfx_.playSoundEffect(onOneCoinGrabbedSfx);
        }
        else
        {
            _sfx_.playSoundEffect(onALotOfCoinsGrabbedSfx);
        }*/
    }

    public void EnemyKilledAction(int scoreValue)
    {
        if (_hud != null)
        {
            _hud.addScoreValue(scoreValue);
        }
        else
        {
            Debug.LogWarning("HUD not found! Score was not added. Make sure there's a GameObject with 'HUD' tag in the scene.");
        }
    }


    public int GetCoins()
    {
        return coinsGrabbed;
    }

    public void OnDeathUnityEventHandler()
    {
        PauseTheGame();
        int previousCoins = PlayerPrefs.GetInt("ppCoins", 0);
        PlayerPrefs.SetInt("ppCoins", previousCoins + coinsGrabbed);

        if (_hud != null)
        {
            _hud.OnDeathHandler();
        }
        else
        {
            Debug.LogWarning("HUD not found! OnDeathHandler() was not called. Make sure there's a GameObject with 'HUD' tag in the scene.");
        }
    }

    public void SceneIsGoingToReset()
    {
        _postProcc_.resetMaterials();
    }
    
    /// <summary>
    /// Verifica si los disparos del jugador están desactivados
    /// </summary>
    /// <returns>True si los disparos están desactivados, false si están activos</returns>
    public bool IsPlayerShootingDisabled()
    {
        return disablePlayerShooting;
    }
}
