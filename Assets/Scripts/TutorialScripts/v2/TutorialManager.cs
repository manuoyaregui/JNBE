using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] TMP_Text tutorialTextBox;
    [SerializeField] TMP_Text continueMessage;
    [SerializeField] string firstMessage = "un texto";
    public static TutorialManager instance;


    bool firstTrigger;

    enum TutorialStates {nonMessage, showMessage };

    TutorialStates state = TutorialStates.nonMessage;


    private void Awake()
    {
        instance = this;
        tutorialTextBox.text = "";
        continueMessage.enabled = false;
    }

    private void Update()
    {
        if(!firstTrigger && !GameManager.singletonGameManager.isInCinematic) //si termino la cinematica lanza esto
        {
            Debug.Log("Entro aca");
            firstTrigger = true;
            ShowMessage(firstMessage);
        }

        if(state == TutorialStates.showMessage && Input.GetButtonDown("Fire2"))
        {
            HideMessage();
        }
    }

    public void ShowMessage(string message)
    {
        state = TutorialStates.showMessage;
        tutorialTextBox.text = message;
        continueMessage.enabled = true;

        HUDController.tutorialSlowDown = true;
    }
    
    public void HideMessage()
    {
        state = TutorialStates.nonMessage;
        tutorialTextBox.text = "";
        continueMessage.enabled = false;

        HUDController.tutorialSlowDown = false;
    }

    private void OnDestroy()
    {
        HUDController.tutorialSlowDown = false;
    }
}
