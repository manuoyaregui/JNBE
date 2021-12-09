using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostGlobalController : MonoBehaviour
{
    private PostProcessVolume globalVolume;
    [SerializeField] List<Color> Colores;

    // Start is called before the first frame update
    void Start()
    {
        globalVolume = GetComponent<PostProcessVolume>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGradentColor(int score)
    {
        Debug.Log("LLEGUÉ HASTA ACÁ");
        ColorGrading colorGr = globalVolume.profile.GetSetting<UnityEngine.Rendering.PostProcessing.ColorGrading>();
        var colorParameter = new UnityEngine.Rendering.PostProcessing.ColorParameter
        {
            value = Colores[Random.Range(0,Colores.Capacity)]
        };
        colorGr.colorFilter.Override(colorParameter);
    }
}
