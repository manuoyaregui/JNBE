using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SkipCinematic : MonoBehaviour
{
    public PlayableDirector playable;
    private float time;
    public GameObject textSkip;
    public GameObject player;
    public bool wasSkiped;
    public GameObject camera;
    // Start is called before the first frame update
    void Start()
    {
        playable = GetComponent<PlayableDirector>();
        time = 7;
        wasSkiped = false;
        textSkip.SetActive(true);
    }

    // Update is called once per fram
    void Update()
    {
        SkipPlayable();
        time = Time.timeSinceLevelLoad;
    }
    private void SkipPlayable()
    {
        if(Input.GetButtonDown("Fire2") && wasSkiped == false && ! HUDController.isPause ) // Tiempo que dura la cinematic
        {
            this.gameObject.SetActive(false);
            wasSkiped = true;
            playable.Stop();
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
            camera.transform.rotation = Quaternion.Euler(0, 0, 0);

             // para que no entre mas esto en el juego

        }
        if( time > playable.duration || wasSkiped == true) // si termino la cinematica o fue salteada...
        {
            GameManager.singletonGameManager.isInCinematic = false;
            player.GetComponent<PlayerController>().resetCamera = true;
            textSkip.SetActive(false); //Apaga el textSkip
            Destroy(gameObject);
        }
        else if(time < playable.duration || wasSkiped == false)
        {
            GameManager.singletonGameManager.isInCinematic = true;

        }
    }
}
