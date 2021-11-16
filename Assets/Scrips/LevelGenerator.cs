using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator comunicadorSape; // Variable para comunicarse entre scrips, tiene un nombre tecnico pero me olvide, asique le puse Sape

    public List<GameBlock> allTheGameBlocks = new List<GameBlock>(); // Sirve para tener los GameBlocks generados alcenados en Prefabs

    public Transform levelStartPoint; // Posicion de la cual voy a spawnear el primer gameblock

    public List<GameBlock> currentBlocks = new List<GameBlock>(); // Lista para almacenar los gameblokcs actuales de la escena


    // Start is called before the first frame update
    void Start()
    {
        comunicadorSape = this;
        GenerateInitialBlocks();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLevelBlock()
    {
        int random = Random.Range(0, allTheGameBlocks.Count);  // Genero un numero aleatorio entre el numero de gameblocks que tenga

        GameBlock currentBlock = (GameBlock)Instantiate(allTheGameBlocks[random]); // instancio un nuevo Gameblock

        currentBlock.transform.SetParent(this.transform, false); // El gameblock instaciado anteriormente va a ser padre del objeto principal "LevelGenerator"

        Vector3 spawnPosition = Vector3.zero;

        if(currentBlocks.Count == 0) // Si es el primer gameblock, se instancia en la posicion inicial
        {
            spawnPosition = levelStartPoint.position;
        }
        else // Si no es el primer gameblock, se va a instanciar la posicion endpoint, previamente definida en el prefab del gameblock
        {
            spawnPosition = currentBlocks[currentBlocks.Count - 1].endPoint.position;
        }

        // El spawnPoint da como resultado el centro geometrico del gameblock, por lo que se le resta con la posicion del startPoint con el fin de obtener la posicion deseada
        Vector3 correction = new Vector3(spawnPosition.x - currentBlock.startPoint.position.x,spawnPosition.y - currentBlock.startPoint.position.y, spawnPosition.z - currentBlock.startPoint.position.z);

        currentBlock.transform.position = correction; // Se modifica la posicion del gameblock

        currentBlocks.Add(currentBlock);  // Se agrega el gameblock a la lista
    }

    void GenerateInitialBlocks()
    {
        for(int i = 0; i < 5; i++)
        {
            AddLevelBlock(); //Agrego un level block por cada paso del bucle, en este caso 5
        }
    }

    public void RemoveOldestGameBlock()
    {
        GameBlock oldestBlock = currentBlocks[0]; // Elijo el primero Gameblock, va a ser siempre el que este en la posicion 0
        currentBlocks.Remove(oldestBlock); // Lo remuevo de la lista
        Destroy(oldestBlock.gameObject); // Lo destruyo
    }

    void DestroyAllGameBlocks()
    {
        while (currentBlocks.Count > 0) // Mientras alla gameblocks en la lista se van a ir removimiendo los gameblocks, sirve para cuando halla que reiniciar el nivel
        {
            RemoveOldestGameBlock();
        }
    }
}
