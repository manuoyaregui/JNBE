using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RampController : MonoBehaviour
{
    [SerializeField] float rampPower;
    [SerializeField] float rampTime;

    public float GetRampPower()
    {
        return rampPower;
    }
    public float GetRampTime()
    {
        return rampTime;
    }
}
