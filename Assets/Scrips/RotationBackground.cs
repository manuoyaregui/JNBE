using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBackground : MonoBehaviour
{
    [SerializeField] GameObject player;
    private float rotatez;
    private float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        rotatez = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
        ChangePosition();
        MoveBackGround();
    }

    public void ChangePosition()
    {
        transform.position = new Vector3(transform.position.x, player.transform.position.y, player.transform.position.z);
    }
    public void Rotate()
    {
        transform.Rotate(0, 0, rotatez);
    }
    public void MoveBackGround()
    {
        if (Time.time > time) 
        {
            int ranNum = Random.Range(1, 16);
            Debug.Log("ram " + ranNum);
            switch (ranNum)
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

            time = Time.time + 5;

        }
    }
    
}
