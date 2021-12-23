using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private List<Transform> checkpoints;
    private int currentCheckpoint = 0;
    [SerializeField] private string KillZoneTagName;
    private PlayerController scriptController;
    bool isDead = false;


    // Start is called before the first frame update
    void Start()
    {
        scriptController = GetComponent<PlayerController>();
    }

    public void MovePlayer() //entraCuandoElPlayerTocaElKillZone
    {
        transform.position = checkpoints[currentCheckpoint].position + new Vector3(0,2,0); // Lo subo un poco asi no se traba en el gameobject
        scriptController.isAlive = true;
        scriptController.SetLives(1);
    }

    public void CheckPointReached(int checkpointNumber)
    {
        currentCheckpoint = checkpointNumber;
        if (currentCheckpoint > checkpoints.Capacity)
        {
            currentCheckpoint = checkpoints.Capacity;
        }
    }
}
