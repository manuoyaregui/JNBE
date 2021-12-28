using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMusicPlayer : MonoBehaviour
{
    [SerializeField] List<AudioClip> listOfSongs;
    private int randomMusic;

    // Start is called before the first frame update
    void Start()
    {
        randomMusic = Random.Range(0, listOfSongs.Capacity);
    }

    // Update is called once per frame
    void Update()
    {
        if( ! GameManager.singletonGameManager.GetAudioSource().isPlaying)
        {
            randomMusic = Random.Range(0, listOfSongs.Capacity);
            GameManager.singletonGameManager.PlaySound(listOfSongs[randomMusic]);
        }

           
    }


}
