using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour //Por ahora en desuso, solo se uso para cumplir un desafio
{
    private int lives; //Cantidad de vidas, sin uso actualmente
    

    public static GameManager singletonGameManager;
    private SfxManager _sfx_;
    [SerializeField] private PostGlobalController _postProcc_;
    

    public static bool isPaused=false;

    [NonSerialized] public int coinsGrabbed;
    [NonSerialized] public bool isInCinematic = true;
    [NonSerialized] public bool isTutorialFinished;

    [SerializeField] AudioClip onOneCoinGrabbedSfx;
    [SerializeField] AudioClip onALotOfCoinsGrabbedSfx;


    HUDController _hud;

    // Start is called before the first frame update
    void Start()
    {
        UnPauseTheGame();
        singletonGameManager = this;
        _sfx_ = SfxManager._sfxManager;
        _hud = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDController>();

    }

    private void Update()
    {
        CheckIfItPaused();
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
        }
        else
        {
            Time.timeScale = 1;
            Cursor.visible = false;
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
        else Debug.Log("there is no HUD in the game");
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

        _hud.OnDeathHandler();
    }

    public void SceneIsGoingToReset()
    {
        _postProcc_.resetMaterials();
    }
}
