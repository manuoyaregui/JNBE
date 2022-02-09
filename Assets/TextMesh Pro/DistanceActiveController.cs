using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DistanceActiveController : MonoBehaviour
{
    GameObject player;
    [SerializeField] float spawnDistance = 10f;
    TMP_Text _text;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) <= spawnDistance)
        {
            _text.enabled = true;
        }
        else
        {
            _text.enabled = false;
        }
    }
}
