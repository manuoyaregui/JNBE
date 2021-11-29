using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] int lives = 1; //Vidas del jugador
    [SerializeField] float mousesensitivity = 150; //sensibilidad del mouse
    [SerializeField] float moveSpeed = 1; //Velocidad de movimiento del jugador
    [SerializeField] GameObject playerBody; //no se usa, hay q ver de sacarlo quiza 
    [SerializeField] GameObject camera; // trabaja la rotacion de la camara
    [SerializeField] float gravity = -9.81f; // valor de la gravedad
    Vector3 gravityVector;
    private float rotationX = 0;
    [SerializeField] GameObject footPoint; // puntos de apoyo y colision
    [SerializeField] GameObject wallPointL, wallPointR;
    [SerializeField] LayerMask floor, wall; // capas para reconocer qué cosa es pared y qué cosa es piso
    [SerializeField] float jump = 5;
    CharacterController characterController;
    private bool isInFloor;
    private bool isInWall;
    public bool isAlive = true;
    private bool dobleJump;
    private float inertia;

    private void Awake()
    {
        Application.targetFrameRate = 60; //Capear los fps en 60
    }
    // Start is called before the first frame update
    void Start()
    {
       dobleJump = true; //Activo el doble jump
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
            InertiaMove();
            GravityForce();
        }
        CheckShield();
        WallDetection();
        //Move();


        if (isInWall && gravityVector.y < 0)
        {
            gravityVector.y = -2f; //Si estoy en la pared disminuyo la fuerza de la gravedad
        }

        Debug.Log(isInWall);

    }
    public int getPlayerLives()
    {
        return lives;
    }
    public void MinusLives()
    {
        lives--;
    }

    public void SetLives(int value)
    {
        lives = value;
    }

    private void Move() //Movimeinto del personaje
    {
        float moveX = Input.GetAxis("Horizontal"); //Imput horizontal

        float moveZ = Input.GetAxis("Vertical"); //Imput vertical

        Vector3 move = transform.right * moveX + transform.forward * moveZ*inertia; //Creo un vector que contiene mi imput en x y z, y al movimiento en z lo mulplico por inercia

        characterController.Move(moveSpeed * Time.deltaTime * move); // Aplico el vector move en el character controller
    }
    private void Mouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mousesensitivity * Time.fixedDeltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity * Time.fixedDeltaTime;

        rotationX += mouseY;

        rotationX = Mathf.Clamp(rotationX,-90, 90); //Clampeo la rotacion del mouse en Y

        camera.transform.localRotation = Quaternion.Euler(rotationX*-1,0,0);

        transform.Rotate(Vector3.up * mouseX);

    }

    private void GravityForce() //Fuerza de gravedad artificial, es sumada con el tiempo
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
                //ShieldIndicator.SetActive(false); -- debe ser cambiado por un elemento de la interfaz
                break;
            case 2:
                //ShieldIndicator.SetActive(true); -- debe ser cambiado por un elemento de la interfaz
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
         

        //Raycast para detectar colicion con la pared
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

    private void Dash() //Dash para adelante apretando el shift, no esta del todo terminado
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
        isInFloor = Physics.CheckSphere(footPoint.transform.position, 0.2f, floor); // Una espera que controla colicion con el piso

        if (isInFloor && gravityVector.y < 0) //Si estoy en el piso disminuyo la graverdad
        {
            gravityVector.y = -2f;
        }
        //Debug.Log(gravityVector.y);

        if (Input.GetButtonDown("Jump") && isInFloor) //Si estoy en el piso y salto, aplico un impulso para arriba
        {
             gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
             characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        if (Input.GetButtonDown("Jump") && isInWall) //Si estoy en la pared y salto, aplico un impuso para arriba
        {
             gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
             characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        
        if(Input.GetButtonDown("Jump") && (isInFloor == false || isInWall == false) && dobleJump == true) //Para controlar el doble salto, si no estoy en el piso o la pared, y tengo el doble jump disponible aplico un impulso para arriba
        {
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
            dobleJump = false;
        }
        if((isInFloor == true || isInWall == true) && dobleJump == false) //Si estoy en la pared o el piso y me gaste el doble salto lo vuelvo a activar
        {
            dobleJump = true;
        }
    }

    public void InertiaMove() //Modifico un flot que esta directamente relacionado con el movimiento
    {
        inertia = Mathf.Clamp(inertia,1f, 1.5f); // Limitar los valores que puede tomar inertia

        if (!isInFloor)//Si no estoy en el piso aumento la inercia
        {
        inertia += 0.0025f;
        }
        else
        {
            inertia -= 0.03f; //Si estoy en el piso disminuyo la inercia
        }
        Debug.Log("Inercia"+inertia);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("KillZone"))
        {
            lives = 0;
            isAlive = false;
        }
    }

    public float GetInertia()
    {
        return this.inertia;
    }


}
