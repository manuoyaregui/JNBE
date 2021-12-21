using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICameraSensibilityController : MonoBehaviour
{
    [SerializeField] private Slider sensibilitySlider = null;
    [SerializeField] private Text sensibilityTextUI = null;
    [SerializeField] private Player playerSettings;

    public void SensibilitySlider(float sensibility)
    {
        sensibilityTextUI.text = "Sensibility " + sensibility.ToString("0");
    }

    public void SaveSensibility()
    {
        float sensivilityValue = sensibilitySlider.value;
        playerSettings.mouseSensibility = sensivilityValue;
        LoadValues();
        Debug.Log("Sensibility Guardado Correctamente");
    }

    void LoadValues()
    {
        float sensivilityValue = playerSettings.mouseSensibility;
        sensibilitySlider.value = sensivilityValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadValues();
    }
}
