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
        isPause = false;
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
        CheckIfItPaused();

        CheckCoins();
    }

    private void CheckCoins()
    {
        coinsText.text = GameManager.singletonGameManager.coinsGrabbed.ToString();
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
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.visible = false;
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

    public void SetScoreMultiplier(float inertiaValue)
    {
        if(inertiaValue >= 1.3f && inertiaValue < 1.49f) //Si estoy en estado de inercia
        {
            scoreMultiplier = 2; //duplica el score
            scoreMultiplierText.text = "X 2";
        }
        else if(inertiaValue >= 1.49f) //Si estoy en estado de "locura"
        {
            scoreMultiplier = 4; //cuadruplicalo
            scoreMultiplierText.text = "X 4";
        }
        else
        {
            scoreMultiplier = 1; //Sino no hagas nada
            scoreMultiplierText.text = "X 1";

        }
    }

    private void CheckScore() //Este metodo se llama cuando colisiono con el LeaveZone mediante un evento
    {
        if(playerLives > 0)
        {
            extraValue += (int)(scoreAddition * scoreMultiplier);
            formula += (int)(scoreAddition * scoreMultiplier);
            textScore.text = "SCORE = " + formula;
            if(extraValue >= changeColorValue)
            {
                extraValue = 0;
                OnScoreSpecificValueUnityEvent?.Invoke(formula);
            }
            if(formula > PlayerPrefs.GetFloat("HighScore", 0))
            {
                panelHighScoreAdvisor.SetActive(true);
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
        StartCoroutine(LoadLevel("Main Scene"));
    }

    public void GoToMainMenu()
    {
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

    public void OnDeathUnityEventHandler()
    {
        ToggleIsPause();
        deathPanel.SetActive(true);
        deathPanel.GetComponentsInChildren<Text>()[1].text = formula.ToString("0");
        coinsInDeathPanelText.text = PlayerPrefs.GetInt("ppCoins", 0).ToString();
        currentHighScoreText.text = "High Score: " + PlayerPrefs.GetFloat("HighScore", 0).ToString("0");
        if(formula > PlayerPrefs.GetFloat("HighScore", 0))
        {
            //playerSettings.highScore = formula;
            PlayerPrefs.SetFloat("HighScore", formula);
            highScorePanel.SetActive(true);
        }        
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

    public void SetInertiaBar(float inertia)
    {
        inertiaBar.value = inertia;
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
