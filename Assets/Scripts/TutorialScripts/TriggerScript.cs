using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TriggerScript : MonoBehaviour
{
    [SerializeField] UnityEvent<string> OnActivateTutorialUnityEvent;
    [SerializeField] private string message;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Se activo el evento");
            OnActivateTutorialUnityEvent?.Invoke(message);
            Destroy(gameObject);
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main Scene");
    }
}
