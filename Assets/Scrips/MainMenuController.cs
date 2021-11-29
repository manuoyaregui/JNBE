using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum menuState { mainMenu,shop,custom,options,exit}

public class MainMenuController : MonoBehaviour
{
    public Canvas mainMenu,shop,custom,options,exit;
    public menuState currentState;
    // Start is called before the first frame update
    void Start()
    {
        currentState = menuState.mainMenu;
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
    public void SetMenuExit()
    {
        currentState = menuState.exit;
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
                exit.enabled = false;
                break;
            case menuState.shop:
                mainMenu.enabled = false;
                shop.enabled = true;
                custom.enabled = false;
                options.enabled = false;
                exit.enabled = false;
                break;
            case menuState.custom:
                mainMenu.enabled = false;
                shop.enabled = false;
                custom.enabled = true;
                options.enabled = false;
                exit.enabled = false;
                break;
            case menuState.options:
                mainMenu.enabled = false;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = true;
                exit.enabled = false;
                break;
            case menuState.exit:
                mainMenu.enabled = false;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = false;
                exit.enabled = true;
                break;
            default:
                mainMenu.enabled = true;
                shop.enabled = false;
                custom.enabled = false;
                options.enabled = false;
                exit.enabled = false;
                break;
        }
    }
    public void Play()
    {
        SceneManager.LoadScene("Main Scene");
    }
}
