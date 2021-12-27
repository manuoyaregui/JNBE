using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fpsCounterController : MonoBehaviour
{
    [SerializeField] private Text fpsTextBox;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fpsTextBox.text = (1 / Time.deltaTime).ToString("0");
    }
}
