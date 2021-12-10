using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowingFloorController : MonoBehaviour
{
    public UnityEvent<Vector3,float,float> onTouchWithThrowerUnityEvent;
    [SerializeField] Transform throwingdirection;
    [SerializeField] float throwSpeed;
    [SerializeField] float throwTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 direction = throwingdirection.position - other.transform.position;
            onTouchWithThrowerUnityEvent?.Invoke(direction,throwSpeed,throwTime);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 direction = throwingdirection.position - collision.transform.position;
            onTouchWithThrowerUnityEvent?.Invoke(direction, throwSpeed, throwTime);
        }
    }
}
