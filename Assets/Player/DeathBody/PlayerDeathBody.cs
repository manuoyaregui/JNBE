using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathBody : MonoBehaviour
{
    public AudioClip deathSound;
    public void ActivateAndMove(Transform playerTransform)
    {
        transform.position = playerTransform.position;
        gameObject.SetActive(true);
        SfxManager._sfxManager.PlaySoundEffect(deathSound);
    }

    public void SetSound(AudioClip clip)
    {
        deathSound = clip;
    }



}
