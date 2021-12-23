using System.Collections.Generic;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    [SerializeField] private List<Transform> checkpoints;
    private int currentCheckpoint = 0;
    private PlayerController scriptController;


    // Start is called before the first frame update
    void Start()
    {
        scriptController = GetComponent<PlayerController>();
    }

    public void MovePlayer() //entraCuandoElPlayerTocaElKillZone
    {
        scriptController.SetLives(1);
        scriptController.isAlive = true;
        transform.position = checkpoints[currentCheckpoint].position + new Vector3(0,2,0); // Lo subo un poco asi no se traba en el gameobject
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
