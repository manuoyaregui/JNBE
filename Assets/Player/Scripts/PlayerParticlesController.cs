using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticlesController : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem InertiaParticles;
    [SerializeField] private ParticleSystem shieldActivated;
    [SerializeField] private ParticleSystem shieldDisabled;
    [SerializeField] private ParticleSystem extraBulletParticles;

    private PlayerController _MC_;

    private void Awake()
    {
        _MC_ = GetComponent<PlayerController>();
    }


    // Start is called before the first frame update
    void Start()
    {
        InertiaParticles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void ShieldActivatedParticles()
    {
        shieldActivated.Play();
    }

    internal void ShieldDisabledParticles()
    {
        shieldDisabled.Play();
    }

    internal void ExtraBulletParticles()
    {
        extraBulletParticles.Play();
    }

    internal void PlayInertiaParticles()
    {
        InertiaParticles.Play();
    }

    internal void StopInertiaParticles()
    {
        InertiaParticles.Pause();
    }
}
