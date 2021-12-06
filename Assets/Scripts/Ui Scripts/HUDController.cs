using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{

    //Valor de Score
    [SerializeField] private float scoreAddition;

    //Para las Balas
    [SerializeField] private Text textBullet;
    private GameObject gun;
    private ShootWeapon pistolScript;
    private int bulletsValue;

    //Para el escudo
    [SerializeField] private GameObject activeShield;
    private int playerLives;

    //Para el Score
    [SerializeField] private Text textScore;
    private int formula = 0;

    //Para el mensaje de fin de juego
    [SerializeField] private GameObject deathPanel;

    private void Awake()
    {
        /*Con eventos*/
        PlayerController.onLivesChange += CheckShield;
        PlayerPickUpGuns.onGunChange += GetGun;
        ShootWeapon.onBulletsChange += CheckBullets;
        ShielPUController.OnShieldPickedUp += CheckShield;
        LeaveZone.OnChangeGB += CheckScore;
        textBullet.text = "";

    }

    // Start is called before the first frame update
    void Start()
    {
        
        ResetScore();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GetGun(GameObject gun)
    {
        this.gun = gun;
        pistolScript = this.gun.GetComponent<ShootWeapon>();
    }

    private void CheckBullets(int bullets)
    {
        bulletsValue = bullets;
        textBullet.text = "" + bulletsValue;
    }

    private void CheckShield(int lives)
    {
        playerLives = lives;
        switch (lives)
        {
            case 0:

                break;
            case 1:
                activeShield.GetComponent<Image>().color = Color.black;
                break;
            case 2:          
                activeShield.GetComponent<Image>().color = Color.green;
                break;
        }
    }

    private void CheckScore() //Este metodo se llama cuando colisiono con el LeaveZone mediante un evento
    {
        if(playerLives > 0)
        {
            formula += (int)(scoreAddition * PlayerController.GetInertia());
            textScore.text = "SCORE = " + formula;
        }
    }

    private void ResetScore()
    {
        formula = 0;
        textScore.text = "Score = " + formula;
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

    public void OnDeathUnityEventHandler()
    {
        deathPanel.SetActive(true);
        deathPanel.GetComponentsInChildren<Text>()[1].text = "" + textScore.text;
    }

    private void OnDestroy()
    {
        PlayerController.onLivesChange -= CheckShield;
        PlayerPickUpGuns.onGunChange -= GetGun;
        ShootWeapon.onBulletsChange -= CheckBullets;
        ShielPUController.OnShieldPickedUp -= CheckShield;
        LeaveZone.OnChangeGB -= CheckScore;
    }
}
