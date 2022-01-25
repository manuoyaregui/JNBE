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
        float moveSpeed = PlayerPrefs.GetFloat("ppMoveSpeed", 20);
        moveSpeed = moveSpeed * 1.1f;
        PlayerPrefs.SetFloat("ppMoveSpeed", moveSpeed);

        int moveSpeedLevel = PlayerPrefs.GetInt("ppMoveSpeedLevel",1);
        moveSpeedLevel++;
        PlayerPrefs.GetInt("ppMoveSpeedLevel", moveSpeedLevel);
        #region
        /*switch (moveSpeedLevel)
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
        }*/
        #endregion

    }
    public void IncreaseJump()
    {
        float jump = PlayerPrefs.GetFloat("ppJump",150f);
        jump = jump * 1.1f;
        PlayerPrefs.SetFloat("ppJump", jump);

        int jumpLevel = PlayerPrefs.GetInt("ppJumpLevel", 1);
        jumpLevel++;
        PlayerPrefs.SetInt("ppJumpLevel", jumpLevel);
    }
    public void IncreaseDashSpeed()
    {
        float dashSpeed = PlayerPrefs.GetFloat("ppDashSpeed", 25f);
        dashSpeed = dashSpeed * 1.1f;
        PlayerPrefs.SetFloat("ppDashSpeed", dashSpeed);

        int dashSpeedLevel = PlayerPrefs.GetInt("ppDashSpeedLevel", 1);
        dashSpeedLevel++;
        PlayerPrefs.SetInt("ppDashSpeedLevel", dashSpeedLevel);
    }
    public void IncreaseDashTime()
    {
        float dashTime = PlayerPrefs.GetFloat("ppDashTime",0.3f);
        dashTime = dashTime * 1.1f;
        PlayerPrefs.SetFloat("ppDashTime", dashTime);

        int dashTimeLevel = PlayerPrefs.GetInt("ppDashTimeLevel", 1);
        dashTimeLevel++;
        PlayerPrefs.SetInt("ppDashTimeLevel", dashTimeLevel);
    }
    public void IncreaseInertiaMin()
    {
        float inertiaMin = PlayerPrefs.GetFloat("ppInertiaMin", 1);
        inertiaMin = inertiaMin * 1.1f;
        PlayerPrefs.SetFloat("ppInertiaMin", inertiaMin);

        int inertiaMinLevel = PlayerPrefs.GetInt("ppInertiaMinLevel", 1);
        inertiaMinLevel++;
        PlayerPrefs.SetInt("ppInertiaMinLevel", inertiaMinLevel);
    }
    public void IncreaseInertiaMax()
    {
        float inertiaMax = PlayerPrefs.GetFloat("ppInertiaMax", 1.5f);
        inertiaMax = inertiaMax * 1.1f;
        PlayerPrefs.SetFloat("ppInertiaMax", inertiaMax);

        int inertiaMaxLevel = PlayerPrefs.GetInt("ppInertiaMaxLevel", 1);
        inertiaMaxLevel++;
        PlayerPrefs.SetInt("ppInertiaMaxLevel", inertiaMaxLevel);
    }
    /*
     * idSkill:
     * 1 = moveSpeed
     * 2 = jump
     * 3 = dashSpeed
     * 4 = dashTime
     * 5 = inertiaMin
     * 6 = inertiaMax
     * 7 = lives
     * */

    public void CheckCoins(string skillTypeLevel, int idSkill)
    {
        int level = PlayerPrefs.GetInt(skillTypeLevel, 1);
        int coins = PlayerPrefs.GetInt("ppCoins", 0);

        int coinsNeeded = 10;


        switch (level)
        {
            case 1:
                {
                    coinsNeeded = 10;
                    break;
                }
            case 2:
                {
                    coinsNeeded = 25;
                    break;
                }
            case 3:
                {
                    coinsNeeded = 50;
                    break;
                }
            case 4:
                {
                    coinsNeeded = 100;
                    break;
                }
            case 5:
                {
                    coinsNeeded = 200;
                    break;
                }
            case 6:
                {
                    coinsNeeded = 500;
                    break;
                }
            case 7:
                {
                    coinsNeeded = 1000;
                    break;
                }
            case 8:
                {
                    coinsNeeded = 2000;
                    break;
                }
        }
        if(level > 8)
        {
            coinsNeeded = 10000000;
        }

        if(coins > coinsNeeded)
        {
            switch (idSkill)
            {
                case 1:
                    {
                        IncreaseMoveSpeed();
                        break;
                    }
                case 2:
                    {
                    IncreaseJump();
                        break;
                    }
                case 3:
                    {
                    IncreaseDashSpeed();
                    break;
                    }
                case 4:
                    {
                        IncreaseDashTime();
                        break;
                    }
                case 5:
                    {
                        IncreaseInertiaMin();
                        break;
                    }
                case 6:
                    {
                        IncreaseInertiaMax();
                        break;
                    }
            }
            coins = coins - coinsNeeded;
            PlayerPrefs.SetInt("ppCoins", coins);
        }
    }
    public void IncreaseLives()
    {
        int coins = PlayerPrefs.GetInt("ppCoins", 0);
        int livesLevel = PlayerPrefs.GetInt("ppLivesLevel", 1);

        if(livesLevel == 1)
        {
            PlayerPrefs.SetInt("ppLives", 2);
            PlayerPrefs.SetInt("ppLivesLevel", 2);
            coins -= 1000;
            PlayerPrefs.SetInt("ppCoins", coins);
        }
        
    }
}
