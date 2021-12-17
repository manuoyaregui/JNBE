using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeleteHighScore : MonoBehaviour
{
    public Player playerSettings;
    private void Start()
    {
        uICurrentHS.text = "Current HighScore: " + playerSettings.highScore.ToString("0.0");
    }

    [SerializeField] private Text uICurrentHS = null;
    public void deleteHighScoreButton()
    {
        playerSettings.highScore = 0;
    }

}
