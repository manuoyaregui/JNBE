using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public enum menuState { mainMenu,shop,custom,options,exit}

public class MainMenuController : MonoBehaviour
{
    public Canvas mainMenu,shop,custom,options;
    public menuState currentState;
    [SerializeField] private GameObject wannaPlayTutorialPanel;


    [SerializeField] private Animator transitionMainMenu;
    [SerializeField] private Animator transitionOptions;
    [SerializeField] private GameObject fadeScreenMainCanvas;
    [SerializeField] private GameObject fadeScreenOptions;

    private void Awake()
    {
        Cursor.visible = true;
        Time.timeScale = 1; //Por alguna razon al pasar de una escena desde el DeathPanel El main menu se tilda
        fadeScreenMainCanvas.SetActive(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        currentState = menuState.mainMenu;
        wannaPlayTutorialPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentState);
        ChangeCanvas();
    }
    public void SetMainMenu()
    {
        currentState = menuState.mainMenu;
    }
    public void SetMenuShop()
    {
        currentState = menuState.shop;
    }
    public void SetMenuCustom()
    {
        currentState = menuState.custom;
    }
    public void SetMenuOptions()
    {
        currentState = menuState.options;
    }
    public void ChangeCanvas()
    {
        switch (currentState)
        {
            case menuState.mainMenu:
                mainMenu.enabled = true;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = false;
                break;
            case menuState.shop:
                mainMenu.enabled = false;
                shop.enabled = true;
                custom.enabled = false;
                options.enabled = false;
                break;
            case menuState.custom:
                mainMenu.enabled = false;
                shop.enabled = false;
                custom.enabled = true;
                options.enabled = false;
                break;
            case menuState.options:
                mainMenu.enabled = false;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = true;
                break;
            case menuState.exit:
                mainMenu.enabled = false;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = false;
                break;
            default:
                mainMenu.enabled = true;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = false;    
                break;
        }
    }
    public void Play(bool isAlreadyAsked)
    {
        if(PlayerPrefs.GetFloat("HighScore" , 0.0f) == 0 && isAlreadyAsked == false)
        {
            wannaPlayTutorialPanel.SetActive(true);
        }
        else
        {
            StartCoroutine(LoadLevel("Main Scene",transitionMainMenu));
        }
    }

    IEnumerator LoadLevel(string levelName, Animator transition) 
    {
        Cursor.visible = false;
        //Inicio el fadeIn
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(1);
        //y cargo la escena
        SceneManager.LoadScene(levelName);
    }

    public void PlayTutorial(int dondeMeEncuentro)
    {
        switch (dondeMeEncuentro) // depende de donde entro lanzo la transicion
        {
            case 0: StartCoroutine(LoadLevel("Tutorial", transitionMainMenu));
                break;
            case 1: StartCoroutine(LoadLevel("Tutorial", transitionOptions));
                break;
            default: StartCoroutine(LoadLevel("Tutorial", transitionMainMenu));
                break;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
