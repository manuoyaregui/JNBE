using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMarc : MonoBehaviour
{
    int ranNumber = 1;
    float rotatez = 1;
    public float rotateDelay;
    private float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        ranNumber = StaticBackground.comunicadorSape.ranNumber;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetRandomNumber();
        Rotate();
        RotateSwitch();
    }
    public void Rotate()
    {
        transform.Rotate(0, 0, rotatez);
    }
    public void GetRandomNumber()
    {
        ranNumber = StaticBackground.comunicadorSape.ranNumber;
    }
    public void RotateSwitch()
    {

        if (Time.time > time)
        {
            switch (ranNumber)
            {
                case 1:
                    rotatez = 0.6f;
                    break;
                case 2:
                    rotatez = -0.6f;
                    break;
                case 3:
                    rotatez = 0;
                    break;
                case 4:
                    rotatez = 1.5f;
                    break;
                case 5:
                    rotatez = -1.5f;
                    break;
                case 6:
                    rotatez = 0.8f;
                    break;
                case 7:
                    rotatez = -0.8f;
                    break;
                case 8:
                    rotatez = 0.2f;
                    break;
                case 9:
                    rotatez = -0.2f;
                    break;
                case 10:
                    rotatez = 0.1f;
                    break;
                case 11:
                    rotatez = -0.1f;
                    break;
                case 12:
                    rotatez = 0f;
                    break;
                case 13:
                    rotatez = 2.5f;
                    break;
                case 14:
                    rotatez = -2.5f;
                    break;
                case 15:
                    rotatez = 3f;
                    break;
                case 16:
                    rotatez = -3f;
                    break;
                default:
                    rotatez = 1;
                    break;
            }

            time = Time.time + 3.5f + rotateDelay;

        }
    }
}
