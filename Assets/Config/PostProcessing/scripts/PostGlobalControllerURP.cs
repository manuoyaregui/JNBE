using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostGlobalControllerURP : MonoBehaviour
{
    private Volume globalVolume;
    private ChromaticAberration chromaticAberration;

    int randomIndex, buffer;

    [SerializeField] float lerpSpeed = .03f;
    [SerializeField] float chrAbSpeed = .1f;

    [Space][Tooltip("Enviroment Materials, weapon and arms")]
    [SerializeField] private Material[] primaryMaterials;

    [Space][Tooltip("Things that can damage the player")]
    [SerializeField] private Material[] secondaryMaterials;

    [Space][Tooltip("Things that benefits the player")]
    [SerializeField] private Material[] otherMaterials;

    [Space]
    [Tooltip("RunText")]
    [SerializeField] Material RunText;

    [Space]
    [SerializeField] private Palette[] palettes;

    private Palette currentPalette;

    float timeCounter;
    bool changeAction;

    void Start()
    {
        globalVolume = GetComponent<Volume>();

        if (globalVolume != null && globalVolume.profile != null)
        {
            if (!globalVolume.profile.TryGet(out chromaticAberration))
            {
                chromaticAberration = globalVolume.profile.Add<ChromaticAberration>(true);
            }
            chromaticAberration.intensity.value = 0f;
            chromaticAberration.active = true;
        }

        buffer = Random.Range(0, palettes.Length);
        ChangePalette();

        PlayerController.onInertiaChange += SetAberrationActive;
    }

    public void SetAberrationActive(float inertiaValue)
    {
        if (chromaticAberration == null) return;

        if (inertiaValue >= 1.49f && chromaticAberration.intensity.value < 1f)
        {
            chromaticAberration.intensity.value = Mathf.Min(1f, chromaticAberration.intensity.value + chrAbSpeed);
        }
        else if (inertiaValue < 1.49f)
        {
            chromaticAberration.intensity.value = Mathf.Max(0f, chromaticAberration.intensity.value - chrAbSpeed);
        }
    }

    private void Update()
    {
        if (changeAction)
        {
            if (timeCounter < 1)
            {
                ChangeMaterials();
                timeCounter += Time.deltaTime;
            }
            else
            {
                changeAction = false;
            }
        }
        else
        {
            timeCounter = 0;
        }
    }

    public void ChangePalette()
    {
        changeAction = true;
        GenerateRandomIndex();
        currentPalette = palettes[randomIndex];
        ChangeMaterials();
    }

    private void ChangeMaterials()
    {
        Color primaryColor = currentPalette.primary;
        Color secondaryColor = currentPalette.secondary;
        Color otherColor = currentPalette.terciary;

        foreach (Material material in primaryMaterials)
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.Lerp(material.GetColor("_EmissionColor"), primaryColor, lerpSpeed));
            }
        }

        foreach (Material material in secondaryMaterials)
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.Lerp(material.GetColor("_EmissionColor"), secondaryColor, lerpSpeed));
            }
        }

        if (otherMaterials.Length > 0)
        {
            foreach (Material material in otherMaterials)
            {
                if (material != null)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.Lerp(material.GetColor("_EmissionColor"), otherColor, lerpSpeed));
                }
            }
        }

        if (RunText != null)
        {
            RunText.color = otherColor;
        }
    }

    internal void resetMaterials()
    {
        foreach (Material material in primaryMaterials)
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.white);
            }
        }

        foreach (Material material in secondaryMaterials)
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.white);
            }
        }

        if (otherMaterials.Length > 0)
        {
            foreach (Material material in otherMaterials)
            {
                if (material != null)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.white);
                }
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

    private void OnApplicationQuit()
    {
        resetMaterials();
    }

    private void OnDestroy()
    {
        PlayerController.onInertiaChange -= SetAberrationActive;
    }
}
