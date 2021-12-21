using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private Text volumeTextUI = null;

    public void VolumeSlider(float volume)
    {
        volumeTextUI.text = "Volume " + (volume * 10).ToString("0");
    }

    public void SaveVolumeButton()
    {
        float volumeValue = volumeSlider.value;
        PlayerPrefs.SetFloat("VolumeValue", volumeValue);
        LoadValues();
        Debug.Log("Volumen Guardado Correctamente");
    }

    void LoadValues()
    {
        float volumeValue = PlayerPrefs.GetFloat("VolumeValue");
        volumeSlider.value = volumeValue;
        AudioListener.volume = volumeValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadValues();
    }
}
