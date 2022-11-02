using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketUIController : MonoBehaviour
{
    [SerializeField] private Text moveSpeedPriceText, jumpPriceText, livesPriceText, dashSpeedPriceText, dashTimePriceText, inertiaMinPriceText, inertiaMaxPriceText;
    [SerializeField] private Text coinsText;
    private int speedPrice, jumpPrice, livesPrice, dashSpeedPrice, dashTimePrice, inertiaMinPrice, inertiaMaxPrice;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per fram
    void Update()
    {
        ChangePrice();   
    }

    public void ChangePrice()
    {
        int speedLevel = PlayerPrefs.GetInt("ppMoveSpeedLevel",1);
        speedPrice = GetPrice(speedLevel);
        if(speedPrice == 10000000)
        {
            moveSpeedPriceText.text = "Max";
        }
        else
        {
            moveSpeedPriceText.text = speedPrice.ToString();
        }
        // speedPrice;

        int jumpLevel = PlayerPrefs.GetInt("ppJumpLevel",1);
        jumpPrice = GetPrice(jumpLevel);
        if(jumpPrice == 10000000)
        {
            jumpPriceText.text = "Max";
        }
        else
        {
            jumpPriceText.text = jumpPrice.ToString();
        }

        int dashSpeedLevel = PlayerPrefs.GetInt("ppDashSpeedLevel", 1);
        dashSpeedPrice = GetPrice(dashSpeedLevel);
        if(dashSpeedPrice == 10000000)
        {
            dashSpeedPriceText.text = "Max";
        }
        else
        {
            dashSpeedPriceText.text = dashSpeedPrice.ToString();
        }

        int dashTimeLevel = PlayerPrefs.GetInt("ppDashTimeLevel", 1);
        dashTimePrice = GetPrice(dashTimeLevel);
        if(dashTimePrice == 10000000)
        {
            dashTimePriceText.text = "Max";
        }
        else
        {
            dashTimePriceText.text = dashTimePrice.ToString();
        }

        int inertiaMinLevel = PlayerPrefs.GetInt("ppInertiaMinLevel", 1);
        inertiaMinPrice = GetPrice(inertiaMinLevel);
        if(inertiaMinPrice == 10000000)
        {
            inertiaMinPriceText.text = "Max";
        }
        else
        {
           inertiaMinPriceText.text = inertiaMinPrice.ToString();

        }

        int inertiaMaxLevel = PlayerPrefs.GetInt("ppInertiaMaxLevel", 1);
        inertiaMaxPrice = GetPrice(inertiaMaxLevel);
        if(inertiaMaxPrice == 10000000)
        {
            inertiaMaxPriceText.text = "Max";
        }
        else
        {
        inertiaMaxPriceText.text = inertiaMaxPrice.ToString();

        }

        int livesLevel = PlayerPrefs.GetInt("ppLivesLevel", 1);
        livesPrice = GetLivesPrice(livesLevel);
        if(livesPrice == 10000000)
        {
            livesPriceText.text = "Max";
        }
        else
        {
            livesPriceText.text = livesPrice.ToString();
        }

        int coins = PlayerPrefs.GetInt("ppCoins", 0);
        coinsText.text = coins.ToString();
    }

    public int GetPrice(int level)
    {
        int price = 25;
        switch (level)
        {
            case 1:
                {
                    price = 10;
                    break;
                }
            case 2:
                {
                    price = 25;
                    break;
                }
            case 3:
                {
                    price = 50;
                    break;
                }
            case 4:
                {
                    price = 100;
                    break;
                }
            case 5:
                {
                    price = 200;
                    break;
                }
            case 6:
                {
                    price = 500;
                    break;
                }
            case 7:
                {
                    price = 1000;
                    break;
                }
            case 8:
                {
                    price = 2000;
                    break;
                }

        }
        if (level > 8)
        {
            price = 10000000;
        }

        return price;
    }

    public int GetLivesPrice(int level)
    {
        if(level == 1)
        {
            livesPrice = 2000;
        }
        else
        {
            livesPrice = 10000000;
        }
        return livesPrice;
    }

    public void AddCoins()
    {
        int coins = PlayerPrefs.GetInt("ppCoins", 0);
        coins += 1000;
        PlayerPrefs.SetInt("ppCoins", coins);
    }
}
