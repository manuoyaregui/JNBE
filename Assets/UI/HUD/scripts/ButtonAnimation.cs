using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler
{
    public Animator anim;
    private Button button;
    // Start is called before the first frame updat
    void Start()
    {
        anim = GetComponent<Animator>();
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void ButtonIsPresed()
    {
        anim.SetBool("isPressed", true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        anim.SetBool("isHover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.SetBool("isHover", false);
    }
}
