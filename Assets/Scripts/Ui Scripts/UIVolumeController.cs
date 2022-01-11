using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Text volumeTextUI;

    public void VolumeSlider(float volumeValue)
    {
        Debug.Log("Entro a la funcion");
        volumeTextUI.text = "Volume " + (volumeValue * 100).ToString("0");
    }

    public void SaveVolumeButton()
    {
        float volumeValue = volumeSlider.value;
        PlayerPrefs.SetFloat("VolumeValue", volumeValue);
        LoadValues();
        Debug.Log("Volumen Guardado Correctamente");
    }

    public void LoadValues()
    {
        float volumeValue = PlayerPrefs.GetFloat("VolumeValue", .3f);
        volumeSlider.value = volumeValue;
        AudioListener.volume = volumeValue;
        VolumeSlider(volumeSlider.value);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadValues();
    }
}
