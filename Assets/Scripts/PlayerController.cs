using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public Player playerSettings;
    private int lives = 1; //Vidas del jugador
    private float mousesensitivity = 150; //sensibilidad del mouse
    private float moveSpeed = 1; //Velocidad de movimiento del jugador
    [SerializeField] GameObject playerBody; //no se usa, hay q ver de sacarlo quiza 
    [SerializeField] GameObject camera; // trabaja la rotacion de la camara
    private float gravity; // valor de la gravedad
    Vector3 gravityVector;
    private float rotationX = 0;
    [SerializeField] GameObject footPoint; // puntos de apoyo y colision
    [SerializeField] GameObject wallPointL, wallPointR;
    [SerializeField] LayerMask floor, wall,ramp; // capas para reconocer qué cosa es pared y qué cosa es piso
    private float jump = 5;
    private CharacterController characterController;
    private bool isInFloor;
    private bool isInWall;
    public bool isAlive;
    private bool dobleJump;
    private static float inertia;
    private float inertiaFOV = 50;
    private bool isInInertiaCharger;
    private bool isInTheRamp;
    [SerializeField] LayerMask inertiaChargerLayer;
    private float dashTime;
    private float dashSpeed;
    private float dashCD;
    private Vector3 move;
    public AudioClip JumpSound;
    public AudioClip DobleJumpSound;
    public AudioClip DashSound;
    public AudioClip killZoneDeath;
    [SerializeField] private ParticleSystem InertiaParticles;

    

    //eventos
    public static event Action<int> onLivesChange;
    [SerializeField] private UnityEvent OnDeathUnityEvent;

    private void Awake()
    {
        Application.targetFrameRate = 60; //Capear los fps en 60

    }
    // Start is called before the first frame update
    void Start()
    {
        //getSettings
        lives = playerSettings.lives;
        mousesensitivity = playerSettings.mouseSensibility;
        gravity = playerSettings.gravity;
        jump = playerSettings.jumpValue;
        dashSpeed = playerSettings.dashSpeed;
        dashTime = playerSettings.dashTime;
        moveSpeed = playerSettings.moveSpeed;
        ///////
        isAlive = true;
        onLivesChange?.Invoke(lives);
        dobleJump = true; //Activo el doble jump
        characterController = GetComponent<CharacterController>();
        ShotgunController.OnShotgunRecoil += DoShotgunRecoil;
        InertiaParticles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive && HUDController.isPause == false)
        {
            Jumpp();
            Dash();
            Mouse();
            Move();
            InertiaMove();
            GravityForce();
            ChangeFOV();
        }
        CheckShield();
        WallDetection();
        InerciaCharger();
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
        //lanzar evento
        onLivesChange?.Invoke(lives);
    }

    public void SetLives(int value)
    {
        lives = value;
    }

    private void Move() //Movimeinto del personaje
    {
        float moveX = Input.GetAxis("Horizontal"); //Imput horizontal

        float moveZ = Input.GetAxis("Vertical"); //Imput vertical

        move = transform.right * moveX + transform.forward * moveZ * inertia; //Creo un vector que contiene mi imput en x y z, y al movimiento en z lo mulplico por inercia

        characterController.Move(moveSpeed * Time.deltaTime * move); // Aplico el vector move en el character controller
    }
    private void Mouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * mousesensitivity * Time.fixedDeltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * mousesensitivity * Time.fixedDeltaTime;

        rotationX += mouseY;

        rotationX = Mathf.Clamp(rotationX, -90, 90); //Clampeo la rotacion del mouse en Y

        camera.transform.localRotation = Quaternion.Euler(rotationX * -1, 0, 0);

        transform.Rotate(Vector3.up * mouseX);

    }

    private void GravityForce() //Fuerza de gravedad artificial, es sumada con el tiempo
    {
        gravityVector.y += gravity;

        characterController.Move(gravityVector * Time.deltaTime);
    }

    private void CheckShield()
    {
        switch (lives)
        {
            case 0:
                isAlive = false;
                OnDeathUnityEvent?.Invoke();
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
        if ((Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f)) ||
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
        float CD = 1F;
        if (Time.time > dashCD && Input.GetButtonDown("Fire3"))
        {
            GameManager.singletonGameManager.PlaySound(DashSound);
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
    private void Jumpp()
    {
        isInFloor = Physics.CheckSphere(footPoint.transform.position, 0.2f, floor); // Una espera que controla colicion con el piso

        if (isInFloor && gravityVector.y < 0) //Si estoy en el piso disminuyo la gravedad
        {
            gravityVector.y = -2f;
        }
        //Debug.Log(gravityVector.y);

        if (Input.GetButtonDown("Jump") && isInFloor) //Si estoy en el piso y salto, aplico un impulso para arriba
        {
            GameManager.singletonGameManager.PlaySound(JumpSound);
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        if (Input.GetButtonDown("Jump") && (isInWall || isInInertiaCharger)) //Si estoy en la pared y salto, aplico un impuso para arriba
        {
            GameManager.singletonGameManager.PlaySound(JumpSound);
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
        }

        if (Input.GetButtonDown("Jump") && (isInFloor == false || isInWall == false || isInInertiaCharger == false) && dobleJump == true) //Para controlar el doble salto, si no estoy en el piso o la pared, y tengo el doble jump disponible aplico un impulso para arriba
        {
            GameManager.singletonGameManager.PlaySound(DobleJumpSound);
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
            dobleJump = false;
        }
        if ((isInFloor == true || isInWall == true || isInInertiaCharger == true) && dobleJump == false) //Si estoy en la pared o el piso y me gaste el doble salto lo vuelvo a activar
        {
            dobleJump = true;
        }
    }

    public void InertiaMove() //Modifico un flot que esta directamente relacionado con el movimiento
    {
        inertia = Mathf.Clamp(inertia, 1f, 1.5f); // Limitar los valores que puede tomar inertia

        if (!isInFloor)//Si no estoy en el piso aumento la inercia
        {
            inertia += 0.0025f;
        }
        else
        {
            inertia -= 0.03f; //Si estoy en el piso disminuyo la inercia
        }
        if(inertia >= 1.3f)
        {
            InertiaParticles.Play();
        }
        else
        {
            InertiaParticles.Pause();
        }
        Debug.Log("Inercia" + inertia);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("KillZone"))
        {
            lives = 0;
            isAlive = false;
            GameManager.singletonGameManager.PlaySound(killZoneDeath);
            onLivesChange?.Invoke(lives);

        }
    }

    public static float GetInertia()
    {
        return inertia;
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

    private void DoShotgunRecoil(float recoilTime, float recoilSpeed)
    {
        StartCoroutine(IEShRecoil(recoilTime,recoilSpeed));
    }

    IEnumerator IEShRecoil(float recoilTime, float recoilSpeed)
    {
        float startTime = Time.time;
        while (Time.time < startTime + recoilTime)
        {
            characterController.Move(-1 * recoilSpeed * Time.deltaTime * camera.transform.forward); // disparo en sentido contrario a la direccion de la camara
            yield return null;
        }
            gravityVector.y = -2f; // reinicio el vector gravedad, para q no caiga muy rapido
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
                StartCoroutine(IEThrowPlayer(throwDirection,rampCont.GetRampPower(),rampCont.GetRampTime()));
            }
            inertia = 1.5f;
            inertiaFOV += 2.5f;
            
        }
    }

    IEnumerator IEThrowPlayer(Vector3 ThrowDirection,float rampPower,float rampTime)
    {
        isthrowed = true;
        float startTime = Time.time;
        while (Time.time < startTime + rampTime)
        {
            Debug.Log("Acá está entrando pa");
            characterController.Move(rampPower * Time.deltaTime * ThrowDirection);
            yield return null;
        }
        gravityVector.y = -2f;
        isthrowed = false;
    }

    public void ReCheckMouseSensibility()
    {
        mousesensitivity = playerSettings.mouseSensibility;
    }

    private void OnDestroy()
    {
        ShotgunController.OnShotgunRecoil -= DoShotgunRecoil;
    }
}
