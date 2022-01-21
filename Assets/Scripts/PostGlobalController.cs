using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostGlobalController : MonoBehaviour
{
    private PostProcessVolume globalVolume;
    int randomIndex,buffer;
    ChromaticAberration crazyEffect;

    // Start is called before the first frame update
    void Start()
    {
        globalVolume = GetComponent<PostProcessVolume>();
        buffer = Random.Range(0, palettes.Length);
        ChangePalette();
        crazyEffect = globalVolume.profile.GetSetting<ChromaticAberration>();
        crazyEffect.active = false;
        PlayerController.onInertiaChange += SetAberrationActive;
    }

    public void SetAberrationActive(float inertiaValue)
    {
        if(inertiaValue >= 1.49f)
        {
            crazyEffect.active = true;
        }
        else
        {
            crazyEffect.active = false;
        }
    }

    /*
    float timeCounter;
    bool changeAction;
    private void Update()
    {
        if (changeAction)
        {

        }
        else
        {
            timeCounter
        }
    }*/

    [Space][Tooltip("Enviroment Materials, weapon and arms")]
    [SerializeField] private Material[] primaryMaterials;

    [Space][Tooltip("Things that can damage the player")]
    [SerializeField] private Material[] secondaryMaterials;

    [Space][Tooltip("Things that benefits the player")]
    [SerializeField] private Material[] otherMaterials;

    [Space]
    [SerializeField] private Palette[] palettes;


    private Palette currentPalette;

    public void ChangePalette()
    {
        GenerateRandomIndex();
        currentPalette = palettes[randomIndex];
        ChangeMaterials();
    }

    private void ChangeMaterials()
    {
        //Get the colors from the palette
        Color primaryColor = currentPalette.primary , 
            secondaryColor = currentPalette.secondary , 
            otherColor = currentPalette.terciary;

        //Aplly the colors to the respective materials
        foreach( Material material in primaryMaterials)
        {
            material.SetColor("_EmissionColor", primaryColor);
        }

        foreach (Material material in secondaryMaterials)
        {
            material.SetColor("_EmissionColor", secondaryColor);
        }

        if(otherMaterials.Length > 0)
        {
            foreach (Material material in otherMaterials)
            {
                material.SetColor("_EmissionColor", otherColor);
            }
        }
    }

    private void GenerateRandomIndex()
    {
        do
        {
            randomIndex = Random.Range(0, palettes.Length);
        } while (buffer == randomIndex);
        buffer = randomIndex;
    }

    private void OnDestroy()
    {
        PlayerController.onInertiaChange -= SetAberrationActive;
    }

}

[System.Serializable]
public class Palette
{
    [ColorUsage(true, true)] public Color primary;
    [ColorUsage(true, true)] public Color secondary;
    [ColorUsage(true, true)] public Color terciary;
}


/*
    1.Generar un index aleatorio
    2.obtener una paleta a partir de ese index
    3.almacenar los colores en variables aparte
    4.aplicar los colores a los distintos materiales
    */

