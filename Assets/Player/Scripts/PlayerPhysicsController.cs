using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerPhysicsController : MonoBehaviour
{
    PlayerController _MC_;


    [Header("Layer Detections")]
    [SerializeField] LayerMask floor; // capas para reconocer qué cosa es pared y qué cosa es piso
    [SerializeField] LayerMask wall;
    [SerializeField] LayerMask ramp;
    [SerializeField] LayerMask inertiaChargerLayer;

    [Header("RaycastReferences")]
    [SerializeField] GameObject GunCamera; //Camara aparte con las armas
    [SerializeField] GameObject footPoint; // puntos de apoyo y colision
    [SerializeField] GameObject wallPointL, wallPointR;

    [Header("References")]
    [SerializeField] GameObject camera; // trabaja la rotacion de la camara

    CharacterController characterController;


    /// Variables ///

    //For Gravity
    private bool isInFloor;
    float gravity;
    bool toggleGravity = true;
    Vector3 gravityVector;

    //For Wall_Detection
    private bool isInWall;
    private bool isInWallLeft;
    private bool isInWallRight;

    //For Camera
    float mouseSensibility;
    private float rotationX = 0;

    public bool resetCamera = false;
    private float rotatezLeft = 0;

    //For Move
    float moveSpeed;
    private Vector3 move;

    //For Inertia
    private float inertiaFOV = 50;
    float inertiaMin = 1f, 
        inertiaMax = 1.5f;
    private static float inertia = 1;

    //For Dash
    float dashSpeed;
    float dashTime;
    private float dashCD;

    //For Jump
    float jump;
    bool doubleJump = true;

    //ForCollisionDetection
    private bool isInTheRamp;
    private bool isInInertiaCharger;


    private void Awake()
    {
        _MC_ = GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {

        characterController = GetComponent<CharacterController>();

        //getSettings
        mouseSensibility = _MC_.playerSettings.mouseSensibility;
        gravity = _MC_.playerSettings.gravity;
        jump = _MC_.playerSettings.jumpValue;
        dashSpeed = _MC_.playerSettings.dashSpeed;
        dashTime = _MC_.playerSettings.dashTime;
        moveSpeed = _MC_.playerSettings.moveSpeed;

        ShotgunController.OnShotgunRecoil += DoShotgunRecoil;
        
        _MC_.PlayerInertiaAltered(inertia);
    }

    // Update is called once per frame
    void Update()
    {
        if (_MC_.IsPlayerAlive() &&
            !GameManager.singletonGameManager.GetPausedStatus() &&
            !GameManager.singletonGameManager.isInCinematic)
        {
            Jumpp();
            Dash();

            Mouse();

            Move();
            InertiaMove();
            ChangeFOV();
            RotateCamaraZ();
            camera.transform.Rotate(0, 0, rotatezLeft);
        }
        WallDetection();
        InerciaCharger();


        if (isInWall && 
            gravityVector.y < 0)
        {
            gravityVector.y = -2f; //Si estoy en la pared disminuyo la fuerza de la graveda
        }
    }

    private void FixedUpdate()
    {
        ActiveGravity();
        if (!GameManager.singletonGameManager.GetPausedStatus() &&
            toggleGravity)
        {
            GravityForce();
        }
    }

    private void Move() //Movimeinto del personaje
    {
        float moveX = Input.GetAxis("Horizontal"); //Imput horizontal

        float moveZ = Input.GetAxis("Vertical"); //Imput vertical

        move = transform.right * moveX + transform.forward * moveZ * inertia; //Creo un vector que contiene mi imput en x y z, y al movimiento en z lo mulplico por inercia

        characterController.Move(moveSpeed * Time.deltaTime * move); // Aplico el vector move en el character controller
    }

    public void InertiaMove() //Modifico un flot que esta directamente relacionado con el movimiento
    {
        inertia = Mathf.Clamp(inertia, inertiaMin, inertiaMax); // Limitar los valores que puede tomar inertia

        if (!isInFloor)//Si no estoy en el piso aumento la inercia
        {
            inertia += 0.0025f;
        }
        else
        {
            inertia -= 0.03f; //Si estoy en el piso disminuyo la inercia
        }
        if (inertia >= 1.3f)
        {
            _MC_.PlayerHaveHighInertia();
        }
        else
        {
            _MC_.PlayerDoesntHaveHighInertia();
        }
        _MC_.PlayerInertiaAltered(inertia);
    }




    private void Jumpp()
    {
        isInFloor = Physics.CheckSphere(footPoint.transform.position, 0.2f, floor); // Una espera que controla colicion con el piso

        if (isInFloor && gravityVector.y < 0) //Si estoy en el piso disminuyo la gravedad
        {
            gravityVector.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isInFloor) //Si estoy en el piso y salto, aplico un impulso para arriba
        {
            _MC_.PlayerIsJumping();
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        if (Input.GetButtonDown("Jump") && (isInWall || isInInertiaCharger)) //Si estoy en la pared y salto, aplico un impuso para arriba
        {
            _MC_.PlayerIsJumping();
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
        }

        if (Input.GetButtonDown("Jump") && 
            (isInFloor == false || isInWall == false || isInInertiaCharger == false) && 
            doubleJump == true) //Para controlar el doble salto, si no estoy en el piso o la pared, y tengo el doble jump disponible aplico un impulso para arriba
        {
            _MC_.PlayerIsDoubleJumping();
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
            doubleJump = false;
        }
        if ((isInFloor == true || isInWall == true || isInInertiaCharger == true) && doubleJump == false) //Si estoy en la pared o el piso y me gaste el doble salto lo vuelvo a activar
        {
            doubleJump = true;
        }
    }

    private void Dash() //Dash para adelante apretando el shift, no esta del todo terminado
    {
        float CD = 1F;
        if (Time.time > dashCD &&
            Input.GetButtonDown("Fire3") && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)) // si presiono dash y me estoy moviendo, realizalo
        {
            _MC_.PlayerIsDashing();
            StartCoroutine(IDash());
            dashCD = Time.time + CD;
        }
    }
    IEnumerator IDash()
    {
        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            characterController.Move(dashSpeed * Time.deltaTime * move);
            yield return null;
        }
    }




    private void Mouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensibility * Time.fixedDeltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensibility * Time.fixedDeltaTime;

        if (resetCamera == true)
        {
            rotationX = 0;
            resetCamera = false;
        }

        rotationX += mouseY;

        rotationX = Mathf.Clamp(rotationX, -90, 90); //Clampeo la rotacion del mouse en Y

        camera.transform.localRotation = Quaternion.Euler(rotationX * -1, 0, 0);

        transform.Rotate(Vector3.up * mouseX);

    }

    public void ChangeMousesensibility(float value)
    {
        mouseSensibility = value;
    }

    private void GravityForce() //Fuerza de gravedad artificial, es sumada con el tiempo
    {
        gravityVector.y += gravity;

        characterController.Move(gravityVector * Time.deltaTime);
    }
    
    public void ActiveGravity()
    {
        if (isInFloor || isInWall)
        {
            toggleGravity = true;
        }
    }



    public void ChangeFOV()
    {

        inertiaFOV = Mathf.Clamp(inertiaFOV, 50f, 75f);

        if (isInWall)
        {
            inertiaFOV += 0.35f;
        }
        if (isInFloor)
        {
            inertiaFOV -= 0.5f;
        }

        Camera.main.fieldOfView = inertiaFOV;
        GunCamera.GetComponent<Camera>().fieldOfView = inertiaFOV;
    }
    public void RotateCamaraZ()
    {
        rotatezLeft = Mathf.Clamp(rotatezLeft, -15, 15);

        if (isInWallLeft)
        {
            rotatezLeft -= 1f;
        }
        if (isInWallRight)
        {
            rotatezLeft += 1f;
        }
        if (!isInWall && rotatezLeft < 0)
        {
            rotatezLeft = Mathf.Clamp(rotatezLeft, -15, 0);
            rotatezLeft += 1f;
        }
        if (!isInWall && rotatezLeft > 0)
        {
            rotatezLeft = Mathf.Clamp(rotatezLeft, 0, 15);
            rotatezLeft -= 1f;
        }
    }


    public void InerciaCharger()
    {
        RaycastHit hit;

        isInInertiaCharger = Physics.CheckSphere(footPoint.transform.position, 0.2f, inertiaChargerLayer);

        if (isInInertiaCharger)
        {
            inertia = 1.5f;
            inertiaFOV += 2.5f;
        }
    }


    private void WallDetection()
    {
        RaycastHit hitL;
        RaycastHit hitR;

        //Raycast para detectar colision con la pared
        if (
            (Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f, wall)) 
            ||
            Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, 0.5f, wall))
        {
            isInWall = true;
        }
        else
        {
            isInWall = false;
        }

        //En Qué pared estoy? Izq o Der
        if (Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f, wall))
        {
            isInWallLeft = true;
        }
        else
        {
            isInWallLeft = false;
        }

        if (Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, 0.5f, wall))
        {
            isInWallRight = true;
        }
        else
        {
            isInWallRight = false;
        }
    }



    private void DoShotgunRecoil(float recoilTime, float recoilSpeed)
    {
        StartCoroutine(IEShRecoil(recoilTime, recoilSpeed));
    }
    IEnumerator IEShRecoil(float recoilTime, float recoilSpeed)
    {
        float startTime = Time.time;
        while (Time.time < startTime + recoilTime)
        {
            characterController.Move(-1 * recoilSpeed * Time.deltaTime * camera.transform.forward); // disparo en sentido contrario a la direccion de la camara
            yield return null;
        }
        gravityVector.y = -2f; // reinicio el vector gravedad, para q no caiga muy rapid
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("tutorialFinished"))
        {
            GameManager.singletonGameManager.isTutorialFinished = true;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("KillZone"))
        {
            _MC_.PlayerTouchedTheKillZone();
        }
    }




    bool isthrowed = false;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ramp"))
        {

            Vector3 throwDirection = hit.gameObject.transform.forward;
            RampController rampCont = hit.gameObject.GetComponent<RampController>();
            if (isthrowed == false)
            {
                StartCoroutine(IEThrowPlayer(throwDirection, rampCont.GetRampPower(), rampCont.GetRampTime()));
            }
            inertia = 1.5f;
            inertiaFOV += 2.5f;

        }


        if (hit.gameObject.CompareTag("FallingFloor"))
        {
            hit.gameObject.GetComponent<FallingFloorActivation>().ActivateFloor();
        }
    }

    IEnumerator IEThrowPlayer(Vector3 ThrowDirection, float rampPower, float rampTime)
    {
        isthrowed = true;
        float startTime = Time.time;
        while (Time.time < startTime + rampTime)
        {
            characterController.Move(rampPower * Time.deltaTime * ThrowDirection);
            yield return null;
        }
        gravityVector.y = -2f;
        isthrowed = false;
    }




    private void OnDestroy()
    {
        ShotgunController.OnShotgunRecoil -= DoShotgunRecoil;
    }

}
