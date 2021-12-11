using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostGlobalController : MonoBehaviour
{
    private PostProcessVolume globalVolume;
    [SerializeField] List<Color> Colores;
    int random;
    int buffer;

    // Start is called before the first frame update
    void Start()
    {
        globalVolume = GetComponent<PostProcessVolume>();
        buffer = Random.Range(0, Colores.Capacity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGradentColor(int score)
    {
        do
        {
            random = Random.Range(0, Colores.Capacity);
        } while (buffer == random);

        buffer = random;
        
        ColorGrading colorGr = globalVolume.profile.GetSetting<UnityEngine.Rendering.PostProcessing.ColorGrading>();
        Color colorParameter = new UnityEngine.Rendering.PostProcessing.ColorParameter
        {
            value = Colores[random]
        };
        colorGr.colorFilter.Override(colorParameter);
    }
}
