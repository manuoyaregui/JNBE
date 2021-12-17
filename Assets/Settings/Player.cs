using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "PlayerSettings")]

public class Player : ScriptableObject
{
    public float mouseSensibility;
    public float moveSpeed;
    public int lives;
    public float gravity;
    public float jumpValue;
    public float dashTime;
    public float dashSpeed;
    public float highScore;
}
