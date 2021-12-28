using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour //Por ahora en desuso, solo se uso para cumplir un desafio
{
    private int lives; //Cantidad de vidas, sin uso actualmente
    public static GameManager singletonGameManager;
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        singletonGameManager = this;
        audioSource = GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        
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
}
