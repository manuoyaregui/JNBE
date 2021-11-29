using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    private GameObject player;

    //Para las Balas
    [SerializeField] private Text textBullet;
    [SerializeField] private GameObject gun;
    private ShootWeapon pistolScript;
    private int bulletsValue;

    //Para el escudo
    [SerializeField] private GameObject disabledShield, activeShield;
    private int playerShieldState;

    //Para el Score
    [SerializeField] private Text textScore;
    float formula = 0;

    //Para el mensaje de fin de juego
    [SerializeField] private GameObject deathPanel;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        textBullet.text = "";
        pistolScript = gun.GetComponent<ShootWeapon>();
        bulletsValue = pistolScript.GetBulletsRemaining();
        playerShieldState = player.GetComponent<PlayerController>().getPlayerLives();
        getGun();
    }

    // Update is called once per frame
    void Update()
    {
        getGun();
        CheckBullets();
        CheckShield();
        CheckScore();
    }

    private void getGun()
    {
        gun = player.GetComponent<PlayerPickUpGuns>().GetActiveGun();
        pistolScript = gun.GetComponent<ShootWeapon>();
    }

    private void CheckBullets()
    {
        bulletsValue = pistolScript.GetBulletsRemaining();
        textBullet.text = "" + bulletsValue;
    }

    private void CheckShield()
    {
        playerShieldState = player.GetComponent<PlayerController>().getPlayerLives();
        switch (playerShieldState)
        {
            case 0:
                deathPanel.SetActive(true);
                deathPanel.GetComponentsInChildren<Text>()[1].text = "" + textScore.text;
                break;
            case 1:
                disabledShield.SetActive(true);
                activeShield.SetActive(false);
                break;
            case 2:
                disabledShield.SetActive(false);
                activeShield.SetActive(true);
                break;
        }
    }

    private void CheckScore()
    {
        if(playerShieldState > 0)
        {
            formula += (int)(Time.time * player.GetComponent<PlayerController>().GetInertia()/2);
            textScore.text = "Score = " + formula;
        }
    }

    //Eventos de Buttons
    public void ResetScene()
    {
        SceneManager.LoadScene("Main Scene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
