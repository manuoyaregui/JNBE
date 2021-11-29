using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour //Por ahora en desuso, solo se uso para cumplir un desafio
{
    private int lives; //Cantidad de vidas, sin uso actualmente
    public static GameManager singletonGameManager;
    public enum typesOfGun {Handgun }; 
    // Start is called before the first frame update
    void Start()
    {
        singletonGameManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddLives()
    {
        lives++;
    }
    public void RestLives()
    {
        lives--;
    }
}
