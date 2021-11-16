using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int lives = 1;

    [SerializeField] float mousesensitivity = 150;

    [SerializeField] float moveSpeed = 1;

    [SerializeField] GameObject playerBody;

    [SerializeField] GameObject camera;

    CharacterController characterController;

    [SerializeField] float gravity = -9.81f;

    Vector3 gravityVector;

    private float rotationX = 0;

    [SerializeField] GameObject footPoint;

    [SerializeField] GameObject wallPointL, wallPointR;

    [SerializeField] LayerMask floor, wall;

    [SerializeField] float jump = 5;

    [SerializeField] private GameObject ShieldIndicator;

    private bool isInFloor;

    private bool isInWall;

    Rigidbody rb;

    private bool isAlive = true;

    private bool dobleJump;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
    // Start is called before the first frame update
    void Start()
    {
       dobleJump = true;
       rb = GetComponent<Rigidbody>();
       characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isAlive)
        {
            Jumpp();
            Dash();
            Mouse();
            Move();

        }
        CheckShield();
        WallDetection();
        //Move();
        GravityForce();


        if (isInWall && gravityVector.y < 0)
        {
            gravityVector.y = -2f;
        }

        Debug.Log(isInWall);

    }
    private void FixedUpdate()
    {

    }

    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal");

        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        characterController.Move(moveSpeed * Time.deltaTime * move);
    }
    private void Mouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mousesensitivity * Time.fixedDeltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity * Time.fixedDeltaTime;

        rotationX += mouseY;

        rotationX = Mathf.Clamp(rotationX,-90, 90);

        camera.transform.localRotation = Quaternion.Euler(rotationX*-1,0,0);

        transform.Rotate(Vector3.up * mouseX);

    }

    private void GravityForce()
    {
        gravityVector.y += gravity; 

        characterController.Move(gravityVector*Time.deltaTime);
    }

    private void CheckShield()
    {
        switch (lives)
        {
            case 0:
                isAlive = false;
                break;
            case 1:
                ShieldIndicator.SetActive(false);
                break;
            case 2:
                ShieldIndicator.SetActive(true);
                break;
            default:
                lives = 0;
                break;
        }
    }
    private void WallDetection()
    {
        RaycastHit hitL;
        RaycastHit hitR;

        if((Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f)) ||
        Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, 0.5f))
        {
            isInWall = true;
        }
        else
        {
            isInWall = false;
        }

       /* if ((Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f)) && Input.GetButtonDown("Jump"))
        {
            float move = Input.GetAxis("Jump");
            Vector3 moveRight = transform.right * 5 * move;
            characterController.Move(moveSpeed * Time.deltaTime * moveRight);
        }
        if ((Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f)) && Input.GetButtonDown("Jump"))
        {
            float move = Input.GetAxis("Jump");
            Vector3 moveLeft = transform.TransformDirection(Vector3.left) * -5 * move;
            characterController.Move(moveSpeed * Time.deltaTime * moveLeft);
        }
        */


    }

    private void Dash()
    {
        if (isInFloor && Input.GetButtonDown("Fire3"))
        {
            float moveX = Input.GetAxis("Horizontal");

            Vector3 move = transform.right * moveX + transform.forward * 1;

            characterController.Move(moveSpeed * Time.deltaTime * move * 5);

        }
    }
    private void Jumpp()
    {
        isInFloor = Physics.CheckSphere(footPoint.transform.position, 0.2f, floor);

        if (isInFloor && gravityVector.y < 0)
        {
            gravityVector.y = -2f;
        }
        //Debug.Log(gravityVector.y);

        if (Input.GetButtonDown("Jump") && isInFloor)
        {
             gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
             characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        if (Input.GetButtonDown("Jump") && isInWall)
        {
             gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
             characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        
        if(Input.GetButtonDown("Jump") && (isInFloor == false || isInWall == false) && dobleJump == true)
        {
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
            dobleJump = false;
        }
        if((isInFloor == true || isInWall == true) && dobleJump == false)
        {
            dobleJump = true;
        }
    }
}
