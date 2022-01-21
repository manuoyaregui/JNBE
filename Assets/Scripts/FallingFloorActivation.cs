using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingFloorActivation : MonoBehaviour
{
    [SerializeField] float timeAfterHittingTheFallingFloor = 5f;

    public void ActivateFloor()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        Destroy(gameObject, timeAfterHittingTheFallingFloor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FallingFloor"))
        {
            ActivateFloor();
        }
    }

}
