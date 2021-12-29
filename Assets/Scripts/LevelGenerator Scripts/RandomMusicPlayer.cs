using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RandomMusicPlayer : MonoBehaviour
{
    [SerializeField] List<AudioClip> listOfSongs;
    private int randomMusic;
    private AudioSource musicPlayer;

    // Start is called before the first frame update
    void Start()
    {
        randomMusic = Random.Range(0, listOfSongs.Capacity);
        musicPlayer = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (! musicPlayer.isPlaying)
        {
            randomMusic = Random.Range(0, listOfSongs.Capacity);
            musicPlayer.PlayOneShot(listOfSongs[randomMusic]);
        }
    }


}
