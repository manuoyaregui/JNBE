using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class HUDController : MonoBehaviour
{
    public static bool isPause;
    //Valor de Score
    [SerializeField] private float scoreAddition;

    //Para las Balas
    [SerializeField] private Text textBullet;
    private GameObject gun;
    private ShootWeapon pistolScript;
    private int bulletsValue;
    [SerializeField] private GameObject plusHangdgunBullets;
    [SerializeField] private int showTime;


    //Para el escudo
    [SerializeField] private GameObject activeShield;
    private int playerLives;

    //Para el Score
    [SerializeField] private Text textScore;
    private int formula = 0;

    //Para el mensaje de fin de juego
    [SerializeField] private GameObject deathPanel;

    //Para menuPausa
    [SerializeField] private GameObject pauseMenu;

    //Eventos de Score
    [SerializeField] private int changeColorValue; //Cada cuanto cambio de color?
    [SerializeField] private UnityEvent<int> OnScoreSpecificValueUnityEvent;
    int extraValue = 0;
    public Player playerSettings;
    [SerializeField] private GameObject highScorePanel;

    //Eventos del Tutorial
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Color taskColor;
    [SerializeField] private Color taskDoneColor;

    private void Awake()
    {
        isPause = false;
        /*Con eventos*/
        PlayerController.onLivesChange += CheckShield;
        PlayerPickUpGuns.onGunChange += GetGun;
        PlayerPickUpGuns.OnExtraBullets += ShowPlusBulletsPannel;
        ShootWeapon.onBulletsChange += CheckBullets;
        ShielPUController.OnShieldPickedUp += CheckShield;
        LeaveZone.OnChangeGB += CheckScore;
        textBullet.text = "";

    }

    // Start is called before the first frame update
    void Start()
    {
        ResetScore();
        tutorialPanel.GetComponent<Image>().color = taskColor;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isPause == false)
        {
            EscapeButtonMenu();
        }
        CheckIfItPaused();
    }

    public void ToggleIsPause()
    {
        isPause = !isPause;
    }

    private void CheckIfItPaused()
    {
        if (isPause)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
    private void GetGun(GameObject gun)
    {
        this.gun = gun;
        pistolScript = this.gun.GetComponent<ShootWeapon>();
        bulletsValue = pistolScript.GetBulletsRemaining();
        CheckBullets(bulletsValue, this.gun);
    }

    private void CheckBullets(int bullets, GameObject gunAffected)
    {
        bulletsValue = bullets;
        textBullet.text = "" + bulletsValue;
    }
    private void ShowPlusBulletsPannel()
    {
        StartCoroutine(IEShowPlusBullets());
    }
    IEnumerator IEShowPlusBullets(){
        plusHangdgunBullets.SetActive(true);
        yield return new WaitForSeconds(showTime);
        plusHangdgunBullets.SetActive(false);
    }

    private void CheckShield(int lives)
    {
        playerLives = lives;
        switch (lives)
        {
            case 0:

                break;
            case 1:
                activeShield.GetComponent<Image>().color = new Color(255,255,255,0.6f);
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
            extraValue += (int)(scoreAddition * PlayerController.GetInertia());
            formula += (int)(scoreAddition * PlayerController.GetInertia());
            textScore.text = "SCORE = " + formula;
            if(extraValue >= changeColorValue)
            {
                extraValue = 0;
                OnScoreSpecificValueUnityEvent?.Invoke(formula);
            }
        }
    }

    private void ResetScore()
    {
        formula = 0;
        textScore.text = "SCORE: " + formula;
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
        if(formula > playerSettings.highScore)
        {
            playerSettings.highScore = formula;
            highScorePanel.SetActive(true);
        }
    }

    public void OnActivateTutorialMessageUnityEventHandler(string message)
    {
        StartCoroutine(IEcolorChange(message));
    }

    IEnumerator IEcolorChange(string message)
    {
        tutorialPanel.GetComponent<Image>().color = taskDoneColor;
        yield return new WaitForSeconds(1f);
        tutorialPanel.GetComponent<Image>().color = taskColor;
        tutorialText.text = message;
    }

    public void EscapeButtonMenu()
    {
        ToggleIsPause();

        if (isPause)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        PlayerController.onLivesChange -= CheckShield;
        PlayerPickUpGuns.onGunChange -= GetGun;
        PlayerPickUpGuns.OnExtraBullets -= ShowPlusBulletsPannel;
        ShootWeapon.onBulletsChange -= CheckBullets;
        ShielPUController.OnShieldPickedUp -= CheckShield;
        LeaveZone.OnChangeGB -= CheckScore;
    }

}
