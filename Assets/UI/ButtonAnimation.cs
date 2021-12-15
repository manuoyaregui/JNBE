using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
   public Animator anim;
    // Start is called before the first frame updat
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ButtonIsPresed()
    {
        anim.SetBool("isPressed", true);
    }
}
