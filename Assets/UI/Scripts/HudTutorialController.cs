using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudTutorialController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Color taskColor;
    [SerializeField] private Color taskDoneColor;

    [SerializeField] private GameObject keyBindingsImage; 

    // Start is called before the first frame update
    void Start()
    {
        tutorialPanel.GetComponent<Image>().color = taskColor;
        keyBindingsImage.SetActive(true);
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

}
