using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColorManager : MonoBehaviour
{
    [Range(0.01f, 1f)] [SerializeField] float speedTransition = .1f;
    [ColorUsage(true,true)][SerializeField] Color[] colors;
    [SerializeField] Material[] materials;
    [SerializeField] Light backgroundLight;

    int buffer, randomIndex;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ChangeColors", 0, 5);
    }

    // Update is called once per frame
    void Update()
    {
        backgroundLight.color = Color.Lerp(backgroundLight.color, colors[randomIndex], speedTransition);
        foreach(Material material in materials)
        {
            //material.SetColor("_EmissionColor", colors[randomIndex]);
            material.SetColor("_EmissionColor", Color.Lerp(material.GetColor("_EmissionColor"), colors[randomIndex], speedTransition));
        }
    }

    private void ChangeColors()
    {
        GenerateRandomIndex();
    }

    private void GenerateRandomIndex()
    {
        do
        {
            randomIndex = Random.Range(0, colors.Length);
        } while (buffer == randomIndex);

        buffer = randomIndex;
    }
}
