using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostGlobalController : MonoBehaviour
{
    private PostProcessVolume globalVolume;
    int randomIndex,buffer;
    ChromaticAberration crazyEffect;
    [SerializeField] List<Color> Colores;

    // Start is called before the first frame update
    void Start()
    {
        globalVolume = GetComponent<PostProcessVolume>();
        buffer = Random.Range(0, palettes.Length);
        ChangePalette();
        crazyEffect = globalVolume.profile.GetSetting<UnityEngine.Rendering.PostProcessing.ChromaticAberration>();
        crazyEffect.active = false;
        PlayerController.onInertiaChange += SetAberrationActive;
        //ChangeGradentColor();
    }

    /*public void ChangeGradentColor()
    {
        GenerateRandomIndex();
        
        ColorGrading colorGr = globalVolume.profile.GetSetting<UnityEngine.Rendering.PostProcessing.ColorGrading>();
        Color colorParameter = new UnityEngine.Rendering.PostProcessing.ColorParameter
        {
            value = Colores[randomIndex]
        };
        colorGr.colorFilter.Override(colorParameter);
    }*/

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

    private void GenerateRandomIndex()
    {
        do
        {
            randomIndex = Random.Range(0, palettes.Length);
        } while (buffer == randomIndex);

        /*if (randomIndex == palettes.Length) 
            randomIndex -= 1;*/

        buffer = randomIndex;
    }






    [Space][Tooltip("Here goes the materials that´s going to be painted with the primaryColor")]
    [SerializeField] private Material[] primaryMaterials;

    [Space][Tooltip("Here goes the materials that´s going to be painted with the secondaryColor")]
    [SerializeField] private Material[] secondaryMaterials;

    [Space][Tooltip("Here goes the materials that´s going to be painted with the otherColor")]
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

    /*public void SetPaletteValues(int index, Palette subClass)
    {
        // Perform any validation checks here.
        palettes[index] = subClass;
    }
    public Palette GetPaletteByIndex(int index)
    {
        // Perform any validation checks here.
        return palettes[index];
    }*/
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

