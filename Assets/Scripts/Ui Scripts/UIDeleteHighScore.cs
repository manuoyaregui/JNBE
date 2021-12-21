using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeleteHighScore : MonoBehaviour
{
    [SerializeField] private Text uICurrentHS = null;
    private void Start()
    {
        uICurrentHS.text = "Current HighScore: " + PlayerPrefs.GetFloat("HighScore" , 0.0f);
    }

    public void deleteHighScoreButton()
    {
        PlayerPrefs.SetFloat("HighScore", 0);
        uICurrentHS.text = "Current HighScore: " + PlayerPrefs.GetFloat("HighScore" , 0.0f);
    }

}
