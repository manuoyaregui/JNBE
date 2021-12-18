using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeleteHighScore : MonoBehaviour
{
    public Player playerSettings;
    [SerializeField] private Text uICurrentHS = null;
    private void Start()
    {
        uICurrentHS.text = "Current HighScore: " + playerSettings.highScore.ToString("0.0");
    }

    public void deleteHighScoreButton()
    {
        playerSettings.highScore = 0;
        uICurrentHS.text = "Current HighScore: " + playerSettings.highScore.ToString("0.0");
    }

}
