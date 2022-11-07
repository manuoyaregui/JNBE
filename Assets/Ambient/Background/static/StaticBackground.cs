using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBackground : MonoBehaviour
{
    public static StaticBackground comunicadorSape;
    [SerializeField] GameObject player;
    private float time = 0;
    public int ranNumber = 1;
    // Start is called before the first frame update
    private void Awake()
    {
        comunicadorSape = this;
        ranNumber = 1;
    }
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ChangePosition();
        GenerateRandonNumber();
    }
    public void ChangePosition()
    {
        transform.position = new Vector3(player.transform.position.x  , player.transform.position.y, player.transform.position.z - 25f);
    }
    public void GenerateRandonNumber()
    {
        if (Time.time > time)
        {
            ranNumber = Random.Range(1, 16);
            time = Time.time + 5;
        }
    }
}
