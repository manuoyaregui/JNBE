using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class HUDController : MonoBehaviour
{
    
    //Valor de Score
    [SerializeField] private float scoreAddition;
    private int scoreMultiplier;
    [SerializeField] private Text scoreMultiplierText;


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

    //para la inercia
    [SerializeField] private Slider inertiaBar;

    //Para el Score
    [SerializeField] private Text textScore;
    [SerializeField] private Text currentHighScoreText;
    [SerializeField] private GameObject panelHighScoreAdvisor;
    private int formula = 0;

    //Para el mensaje de fin de juego
    [SerializeField] private GameObject deathPanel;

    //Para menuPausa
    [SerializeField] private GameObject pauseMenu;

    //Eventos de Score
    [SerializeField] private int changeColorValue; //Cada cuanto cambio de color?
    [SerializeField] private UnityEvent<int> OnScoreSpecificValueUnityEvent;
    int extraValue = 0;
    [SerializeField] private GameObject highScorePanel;

    //Check Coins
    [SerializeField] Text coinsText;
    [SerializeField] Text coinsInDeathPanelText;

    //Transiciones
    [SerializeField] private Animator transition;
    [SerializeField] private GameObject loadingScreen;

    private void Awake()
    {
        /*Con eventos*/
        PlayerController.onLivesChange += CheckShield;
        PlayerPickUpGuns.onGunChange += GetGun;
        PlayerPickUpGuns.OnExtraBullets += ShowPlusBulletsPannel;
        PlayerController.onInertiaChange += SetInertiaBar;
        PlayerController.onInertiaChange += SetScoreMultiplier;
        ShootWeapon.onBulletsChange += CheckBullets;
        ShielPUController.OnShieldPickedUp += CheckShield;
        LeaveZone.OnChangeGB += CheckScore;
        textBullet.text = "";
    }

    // Start is called before the first frame update
    void Start()
    {
        
        ResetScore();
        scoreMultiplier = 1;
        loadingScreen.SetActive(true);
        panelHighScoreAdvisor.SetActive(false);
       
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscapeButtonMenu();
        }

        CheckCoins();
    }

    private void CheckCoins()
    {
        coinsText.text = GameManager.singletonGameManager.coinsGrabbed.ToString();
    }

    public static bool tutorialSlowDown;
    
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

    public void SetScoreMultiplier(float inertiaValue)
    {
        if(inertiaValue >= 1.3f && inertiaValue < 1.49f) //Si estoy en estado de inercia
        {
            scoreMultiplier = 2; //duplica el score
        }
        else if(inertiaValue >= 1.49f) //Si estoy en estado de "locura"
        {
            scoreMultiplier = 4; //cuadruplicalo
        }
        else
        {
            scoreMultiplier = 1; //Sino no hagas nada
        }

        scoreMultiplierText.text = "X " + scoreMultiplier;
    }
    private void CheckScore() //Este metodo se llama cuando colisiono con el LeaveZone mediante un evento
    {
        if(playerLives > 0)
        {
            extraValue += (int)(scoreAddition * scoreMultiplier);
            formula += (int)(scoreAddition * scoreMultiplier);
            if(extraValue >= changeColorValue)
            {
                extraValue = 0;
                OnScoreSpecificValueUnityEvent?.Invoke(formula);
            }
            UpdateScorePanel();
        }
    }

    private void UpdateScorePanel()
    {
        textScore.text = "SCORE = " + formula;
        if (formula > PlayerPrefs.GetFloat("HighScore", 0))
        {
            panelHighScoreAdvisor.SetActive(true);
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
        UnPauseGame();

        StartCoroutine(LoadLevel("Main Scene"));
    }

    public void ResetTutorialScene()
    {
        if (GameManager.singletonGameManager.isTutorialFinished) ResetScene();
        else StartCoroutine(LoadLevel("Tutorial V2"));
    }

    public void GoToMainMenu()
    {
        UnPauseGame();
        StartCoroutine(LoadLevel("MainMenu"));
    }

    IEnumerator LoadLevel(string levelName)
    {
        //Inicio el fadeIn
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(1f);
        //y cargo la escena
        SceneManager.LoadScene(levelName);
    }

    public void OnDeathHandler()
    {
        deathPanel.SetActive(true);
        deathPanel.GetComponentsInChildren<Text>()[1].text = formula.ToString("0");

        int previousCoins = PlayerPrefs.GetInt("ppCoins", 0);
        PlayerPrefs.SetInt("ppCoins", previousCoins + GameManager.singletonGameManager.GetCoins());
        coinsInDeathPanelText.text = PlayerPrefs.GetInt("ppCoins", 0).ToString();
        
        currentHighScoreText.text = "High Score: " + PlayerPrefs.GetFloat("HighScore", 0).ToString("0");
        if(formula > PlayerPrefs.GetFloat("HighScore", 0))
        {
            PlayerPrefs.SetFloat("HighScore", formula);
            highScorePanel.SetActive(true);
        }        
    }


    public void addScoreValue(int value)
    {
        formula += value * scoreMultiplier;
        UpdateScorePanel();
    }
  
    public void EscapeButtonMenu()
    {
        GameManager.singletonGameManager.ToggleIsPause();

        if ( GameManager.singletonGameManager.GetPausedStatus() )
            pauseMenu.SetActive(true);
        else
            pauseMenu.SetActive(false);
    }

    public void SetInertiaBar(float inertia)
    {
        inertiaBar.value = inertia;
    }

    public void UnPauseGame()
    {
        GameManager.singletonGameManager.UnPauseTheGame();
    }

    public void PauseGame()
    {
        GameManager.singletonGameManager.PauseTheGame();
    }

    private void OnDestroy()
    {
        PlayerController.onLivesChange -= CheckShield;
        PlayerController.onInertiaChange -= SetInertiaBar;
        PlayerPickUpGuns.onGunChange -= GetGun;
        PlayerPickUpGuns.OnExtraBullets -= ShowPlusBulletsPannel;
        PlayerController.onInertiaChange -= SetScoreMultiplier;
        ShootWeapon.onBulletsChange -= CheckBullets;
        ShielPUController.OnShieldPickedUp -= CheckShield;
        LeaveZone.OnChangeGB -= CheckScore;
    }

}
