using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipTextGuide : MonoBehaviour
{
    public Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            canvas.enabled = false;
            HUDController.isPause = false;
            Destroy(gameObject);
        }
    }
}
