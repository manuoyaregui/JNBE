using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [Header("Sounds")]
    public AudioClip JumpSound;
    public AudioClip DoubleJumpSound;
    public AudioClip DashSound;
    public AudioClip killZoneDeath;

    internal SfxManager _sfx_;

    private void Start()
    {
        _sfx_ = SfxManager._sfxManager;
    }


    internal void jump()
    {
        if (JumpSound != null)
            _sfx_.PlaySoundEffect(JumpSound);
    }

    internal void killZoneDeathSound()
    {
        if(killZoneDeath != null)
            _sfx_.PlaySoundEffect(killZoneDeath) ;
    }

    internal void doubleJump()
    {
        if (DoubleJumpSound != null)
            _sfx_.PlaySoundEffect(DoubleJumpSound);
    }

    internal void dash()
    {
        if(DashSound != null)
            _sfx_.PlaySoundEffect(DashSound);
    }
}
