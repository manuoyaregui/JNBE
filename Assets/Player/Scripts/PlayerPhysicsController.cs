using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsController : MonoBehaviour
{
    [Header("Layer Detections")]
    [SerializeField] LayerMask floor; // capas para reconocer qué cosa es pared y qué cosa es piso
    [SerializeField] LayerMask wall;
    [SerializeField] LayerMask ramp;
    [SerializeField] LayerMask inertiaChargerLayer;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
