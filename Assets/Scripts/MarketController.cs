using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseMoveSpeed()
    {
        int coins = PlayerPrefs.GetInt("ppCoins", 0);
        float moveSpeed = PlayerPrefs.GetFloat("ppMoveSpeed", 20);
        moveSpeed = moveSpeed * 1.1f;
        PlayerPrefs.SetFloat("ppMoveSpeed", moveSpeed);

        int moveSpeedLevel = PlayerPrefs.GetInt("ppMoveSpeedLevel",1);
        switch(moveSpeedLevel)
        {
            case 1:
                {
                    coins -= 10;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 2:
                {
                    coins -= 25;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 3:
                {
                    coins -= 50;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 4:
                {
                    coins -= 100;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 5:
                {
                    coins -= 200;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 6:
                {
                    coins -= 500;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 7:
                {
                    coins -= 1000;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
            case 8:
                {
                    coins -= 2000;
                    PlayerPrefs.SetInt("ppCoins", coins);
                    break;
                }
        }
        moveSpeedLevel++;
        PlayerPrefs.GetInt("ppMoveSpeedLevel", moveSpeedLevel);
    }
}
