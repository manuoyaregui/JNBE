using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public Player playerSettings;



    [Header("References")]
    [SerializeField] GameObject playerBody; //no se usa, hay q ver de sacarlo quiza 
    [SerializeField] GameObject camera; // trabaja la rotacion de la camara
    [SerializeField] GameObject GunCamera; //Camara aparte con las armas
    [SerializeField] GameObject footPoint; // puntos de apoyo y colision
    [SerializeField] GameObject wallPointL, wallPointR;

    [Header("Layer Detections")]
    [SerializeField] LayerMask floor; // capas para reconocer qué cosa es pared y qué cosa es piso
    [SerializeField] LayerMask wall;
    [SerializeField] LayerMask ramp;
    [SerializeField] LayerMask inertiaChargerLayer;

    [Header("Sounds")]
    public AudioClip JumpSound;
    public AudioClip DobleJumpSound;
    public AudioClip DashSound;
    public AudioClip killZoneDeath;

    [Header("Particles")]
    [SerializeField] private ParticleSystem InertiaParticles;
    [SerializeField] private ParticleSystem shieldActivated;
    [SerializeField] private ParticleSystem shieldDisabled;
    [SerializeField] private ParticleSystem extraBulletParticles;


    [Header("Tutorial")] [Tooltip("Dont use in main game")]
    [SerializeField] private LayerMask tutorialLayerDieZone;

    
    //eventos
    public static event Action<int> onLivesChange;
    public static event Action<float> onInertiaChange;
    [SerializeField] private UnityEvent OnDeathUnityEvent;

    private int lives = 1; //Vidas del jugador
    private float mousesensitivity = 150; //sensibilidad del mouse
    private float moveSpeed = 1; //Velocidad de movimiento del jugador
    private float gravity; // valor de la gravedad
    Vector3 gravityVector;
    private float rotationX = 0;
    private float jump = 5;
    private CharacterController characterController;

    // Flags
    public bool isAlive;
    private static float inertia;
    private bool isInFloor;
    private bool dobleJump;
    private bool isInWall;
    private float inertiaFOV = 50;
    private float inertiaMin;
    private float inertiaMax;
    private bool isInInertiaCharger;
    private bool isInTheRamp;
    private float dashTime;
    private float dashSpeed;
    private float dashCD;
    private bool isInWallLeft;
    private bool isInWallRight;
    public bool resetCamera = false;
    private bool toogleGravity;

    private Vector3 move;
    private float rotatezLeft = 0;


    private void Awake()
    {
        Application.targetFrameRate = 60; //Capear los fps en 60


        PlayerPickUpGuns.OnExtraBullets += ExtraBulletsPS;
    }
    // Start is called before the first frame updat
    void Start()
    {
        toogleGravity = true;
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
        inertia = 1;
        onInertiaChange?.Invoke(inertia);
        ShielPUController.OnShieldPickedUp += ActivateShieldParticleSystem;
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
            //GravityForce();
            ChangeFOV();
            RotateCamaraZ();
            camera.transform.Rotate(0, 0, rotatezLeft);
        }
        CheckShield();
        WallDetection();
        InerciaCharger();
        //Move();


        if (isInWall && gravityVector.y < 0)
        {
            gravityVector.y = -2f; //Si estoy en la pared disminuyo la fuerza de la graveda
        }


    }
    private void FixedUpdate()
    {
        RaycastGravity();
        ActiveGravity();
        if (isAlive && HUDController.isPause == false && toogleGravity)
        {
            GravityForce();
        }
    }


    public int getPlayerLives()
    {
        return lives;
    }
    public void MinusLives()
    {
        if(lives == 2)
        {
            DisableShieldParticleSystem();
        }

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

    private void GravityForce() //Fuerza de gravedad artificial, es sumada con el tiempo
    {
        gravityVector.y += gravity;

        characterController.Move(gravityVector * Time.deltaTime );
    }

    bool alreadyDeath;
    private void CheckShield()
    {
        switch (lives)
        {
            case 0:
                isAlive = false;
                if (!alreadyDeath)
                {
                    OnDeathUnityEvent?.Invoke();
                    alreadyDeath = true;
                }
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
        if ((Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f, wall)) ||
        Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, 0.5f, wall))
        {
            isInWall = true;
        }
        else
        {
            isInWall = false;
        }

        if(Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, 0.5f, wall))
        {
            isInWallLeft = true;
        }
        else
        {
            isInWallLeft = false;
        }

        if(Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, 0.5f, wall))
        {
            isInWallRight = true;
        }
        else
        {
            isInWallRight = false;
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
    [SerializeField] 
    public void InertiaMove() //Modifico un flot que esta directamente relacionado con el movimiento
    {
        inertia = Mathf.Clamp(inertia, 1f, 1.5f); // Limitar los valores que puede tomar inertia

        if ( ! isInFloor)//Si no estoy en el piso aumento la inercia
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
        onInertiaChange?.Invoke(inertia);
    }
    private bool alreadyIn;
    private void OnTriggerStay(Collider other)
    {
        
        if (other.gameObject.CompareTag("KillZone"))
        {
            lives = 0;
            isAlive = false;
            if (alreadyIn == false)
            {
                GameManager.singletonGameManager.PlaySound(killZoneDeath);
                alreadyIn = true;
            }
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
        GunCamera.GetComponent<Camera>().fieldOfView = inertiaFOV;
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
            gravityVector.y = -2f; // reinicio el vector gravedad, para q no caiga muy rapid
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

        
        if (hit.gameObject.CompareTag("FallingFloor"))
        {
            hit.gameObject.GetComponent<FallingFloorActivation>().ActivateFloor();
        }
    }

    IEnumerator IEThrowPlayer(Vector3 ThrowDirection,float rampPower,float rampTime)
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

    public void ReCheckMouseSensibility()
    {
        mousesensitivity = playerSettings.mouseSensibility;
    }

    private void OnDestroy()
    {
        ShotgunController.OnShotgunRecoil -= DoShotgunRecoil;
        ShielPUController.OnShieldPickedUp -= ActivateShieldParticleSystem;
        PlayerPickUpGuns.OnExtraBullets -= ExtraBulletsPS;
    }

    public void RotateCamaraZ()
    {
        rotatezLeft = Mathf.Clamp(rotatezLeft, -15, 15);

        if (isInWallLeft)
        {
            rotatezLeft -= 1f;
        }
        if(isInWallRight)
        {
            rotatezLeft += 1f;
        }
        if(!isInWall && rotatezLeft < 0)
        {
            rotatezLeft = Mathf.Clamp(rotatezLeft, -15, 0);
            rotatezLeft += 1f;
        }
        if(!isInWall && rotatezLeft > 0)
        {
            rotatezLeft = Mathf.Clamp(rotatezLeft, 0, 15);
            rotatezLeft -= 1f;
        }
    }
    bool checkGravity;

    public void RaycastGravity()
    {
        RaycastHit hit;
        checkGravity = Physics.Raycast(footPoint.transform.position, transform.TransformDirection(Vector3.down), out hit, 0.5f, tutorialLayerDieZone);
        if (Physics.Raycast(footPoint.transform.position,transform.TransformDirection(Vector3.down), out hit, 0.5f, tutorialLayerDieZone))
        {
            Debug.Log("La gravedad esta " + toogleGravity);
            toogleGravity = false;            
            GetComponent<CheckPointController>().MovePlayer();
        }
    }
    public void ActiveGravity()
    {
        if(isInFloor || isInWall)
        {
            toogleGravity = true;
        }
    }
    /*public void MoveToCheckpoint()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("NO ENTIENDO UNA MIERDA QUE PASA");
            this.transform.position = new Vector3(100, 100, 100);
        }
    }*/

    public void ActivateShieldParticleSystem(int value)
    {
        if(lives == 1) //Sólo si no tenía el escudo activar las particulas
            shieldActivated.Play();
    }

    public void DisableShieldParticleSystem()
    {
        shieldDisabled.Play();
    }

    public void ExtraBulletsPS() 
    {
        extraBulletParticles.Play();
    }
    
}
