using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager _sfxManager;
    private AudioSource audioSource;

    private void Awake()
    {
        _sfxManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlaySoundEffect(AudioClip sound)
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

    internal void playSoundEffect(AudioClip onOneCoinGrabbedSfx)
    {
        throw new NotImplementedException();
    }
}
