using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour //Por ahora en desuso, solo se uso para cumplir un desafio
{
    private int lives; //Cantidad de vidas, sin uso actualmente
    public static GameManager singletonGameManager;
    private AudioSource audioSource;
    [NonSerialized] public int coinsGrabbed;
    [NonSerialized] public bool isInCinematic = true;
    [NonSerialized] public bool isTutorialFinished;

    [SerializeField] AudioClip onOneCoinGrabbedSfx;
    [SerializeField] AudioClip onALotOfCoinsGrabbedSfx;


    HUDController _hud;

    // Start is called before the first frame update
    void Start()
    {
        singletonGameManager = this;
        audioSource = GetComponent<AudioSource>();
        _hud = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDController>();
    }
    public void AddLives()
    {
        lives++;
    }
    public void RestLives()
    {
        lives--;
    }
    public void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
    public void StopMusic()
    {
        audioSource.Stop();
    }

    public AudioSource GetAudioSource()
    {
        return audioSource;
    }

    public void AddCoins(int value)
    {
        coinsGrabbed += value;

        if(value == 1)
        {
            audioSource.PlayOneShot(onOneCoinGrabbedSfx);
        }
        else
        {
            audioSource.PlayOneShot(onALotOfCoinsGrabbedSfx);
        }
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
        int previousCoins = PlayerPrefs.GetInt("ppCoins", 0);
        PlayerPrefs.SetInt("ppCoins", previousCoins + coinsGrabbed);
    }
}
