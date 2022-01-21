using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingFloorController : MonoBehaviour
{
    //The moving floor moves infinitely in a defined direction, after the player gets in the trigger range

    [Tooltip("ReferenceObject that defines in what direction the block will move")]
    [SerializeField] GameObject destinationObject;
    [SerializeField] float playerDistanceTrigger = 100f;
    [SerializeField] float speedMultiplier = 2;

    float playerDistance;
    GameObject player;
    bool canMove;
    Vector3 direction;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        direction = (destinationObject.transform.position - transform.position).normalized;
    }

    private void Update()
    {
        playerDistance = Vector3.Distance(transform.position, player.transform.position);
        if(playerDistance < playerDistanceTrigger)
        {
            canMove = true;
        }
        MoveObject();
    }

    private void MoveObject()
    {
        if(canMove) 
            transform.position += direction * Time.deltaTime * speedMultiplier;
    }
}

//obtener direccion