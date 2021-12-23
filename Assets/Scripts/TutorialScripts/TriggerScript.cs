using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TriggerScript : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivateTutorialUnityEvent;
    //[SerializeField] private string message;
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main Scene");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Se activo el evento");
            OnActivateTutorialUnityEvent?.Invoke();
            Destroy(gameObject);
        }
    }
}
