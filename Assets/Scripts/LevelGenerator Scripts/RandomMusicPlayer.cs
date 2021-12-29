using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if(! SceneManager.GetActiveScene().Equals(SceneManager.GetSceneByName("MainMenu"))) // si no estoy en el menu principal que llame al gamemanager
        {
            if(!GameManager.singletonGameManager.GetAudioSource().isPlaying)
            {
                randomMusic = Random.Range(0, listOfSongs.Capacity);
                GameManager.singletonGameManager.PlaySound(listOfSongs[randomMusic]);

            }
        }
        else //para main menu
        {
            if ( !GetComponent<AudioSource>().isPlaying)
            {
                randomMusic = Random.Range(0, listOfSongs.Capacity);
                GetComponent<AudioSource>().PlayOneShot(listOfSongs[randomMusic]);
            }
        }
    }


}
