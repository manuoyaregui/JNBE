using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveZone : MonoBehaviour
{
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
        if(other.tag == "Player")
        {
            LevelGenerator.comunicadorSape.AddLevelBlock(); //Al entrar en colicion con el player agrega un nuevo bloque
            LevelGenerator.comunicadorSape.RemoveOldestGameBlock(); //Al entrar en colicion con el player remueve un bloque viejo
        }
    }
}
