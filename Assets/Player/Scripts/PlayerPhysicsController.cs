using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using DG.Tweening;

/// <summary>
/// Handles all physics-related behavior for the player including movement, jumping, dashing,
/// wall detection, camera controls, and inertia system. This is the core physics controller
/// that manages player movement and environmental interactions.
/// </summary>
public class PlayerPhysicsController : MonoBehaviour
{
    PlayerController _MC_;


    [Header("Layer Detections")]
    [SerializeField] LayerMask floor; // capas para reconocer qu� cosa es pared y qu� cosa es piso
    [SerializeField] LayerMask wall;
    [SerializeField] LayerMask ramp;
    [SerializeField] LayerMask inertiaChargerLayer;

    [Header("RaycastReferences")]
    [SerializeField] GameObject GunCamera; //Camara aparte con las armas
    [SerializeField] GameObject footPoint; // puntos de apoyo y colision
    [SerializeField] GameObject wallPointL, wallPointR;

    [Header("References")]
    [SerializeField] GameObject camera; // trabaja la rotacion de la camara

    [Header("Physics Constants")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallSlideGravity = -2f;
    [SerializeField] private float inertiaIncreaseRate = 0.002f;
    [SerializeField] private float inertiaDecreaseRate = 0.025f;
    [SerializeField] private float highInertiaThreshold = 1.3f;
    [SerializeField] private float wallCheckInterval = 0.1f;
    [SerializeField] private float cameraRotationSpeed = 0.5f;
    [SerializeField] private float maxCameraTilt = 15f;
    [SerializeField] private float fovIncreaseRate = 0.25f;
    [SerializeField] private float fovDecreaseRate = 0.35f;
    [SerializeField] private float minFOV = 55f;
    [SerializeField] private float maxFOV = 80f;
    [SerializeField] private float normalFOV = 65f;
    [SerializeField] private float inertiaLerpSpeed = 3f;
    [SerializeField] private float fovLerpSpeed = 0.5f;

    CharacterController characterController;

    // Cached components for better performance
    private Camera mainCamera;
    private Camera gunCameraComponent;
    private float lastWallCheckTime;
    
    // DOTween references for better performance
    private Tween dashTween;
    private Tween recoilTween;
    private Tween throwTween;
    private Tween fovTween;
    private Tween cameraTiltTween;

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
    private float cameraTiltAngle = 0;

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
        // Cache components for better performance
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        gunCameraComponent = GunCamera.GetComponent<Camera>();

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
            HandleJump();
            HandleDash();

            HandleMouseInput();

            HandleMovement();
            UpdateInertia();
            ChangeFOV();
            HandleCameraTilt();
        }
        WallDetection();
        CheckInertiaCharger();


        if (isInWall && 
            gravityVector.y < 0)
        {
            gravityVector.y = wallSlideGravity; //Si estoy en la pared disminuyo la fuerza de la gravedad
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

    /// <summary>
    /// Handles player movement input and applies movement to the character controller.
    /// Combines horizontal and vertical input with inertia multiplier for forward movement.
    /// </summary>
    private void HandleMovement() //Movimiento del personaje
    {
        float moveX = Input.GetAxis("Horizontal"); //Imput horizontal

        float moveZ = Input.GetAxis("Vertical"); //Imput vertical

        move = transform.right * moveX + transform.forward * moveZ * inertia; //Creo un vector que contiene mi imput en x y z, y al movimiento en z lo mulplico por inercia

        characterController.Move(moveSpeed * Time.deltaTime * move); // Aplico el vector move en el character controller
    }

    /// <summary>
    /// Updates the inertia system based on player state. Increases inertia when airborne,
    /// decreases when grounded. Manages high inertia state and notifies other systems.
    /// Now with more gradual and controlled inertia changes.
    /// </summary>
    public void UpdateInertia() //Modifico un float que esta directamente relacionado con el movimiento
    {
        float targetInertia = inertia;
        
        if (!isInFloor)//Si no estoy en el piso aumento la inercia
        {
            targetInertia += inertiaIncreaseRate;
        }
        else
        {
            targetInertia -= inertiaDecreaseRate; //Si estoy en el piso disminuyo la inercia
        }
        
        // Clamp the target inertia
        targetInertia = Mathf.Clamp(targetInertia, inertiaMin, inertiaMax);
        
        // Smooth transition to target inertia
        inertia = Mathf.Lerp(inertia, targetInertia, Time.deltaTime * inertiaLerpSpeed);
        
        // Check for high inertia state changes
        bool wasHighInertia = inertia >= highInertiaThreshold;
        bool isHighInertia = inertia >= highInertiaThreshold;
        
        if (isHighInertia && !wasHighInertia)
        {
            _MC_.PlayerHaveHighInertia();
        }
        else if (!isHighInertia && wasHighInertia)
        {
            _MC_.PlayerDoesntHaveHighInertia();
        }
        
        _MC_.PlayerInertiaAltered(inertia);
    }




    /// <summary>
    /// Handles all jump-related input and physics including ground jumping, wall jumping, and double jumping.
    /// Manages jump state and applies appropriate vertical forces based on current player state.
    /// </summary>
    private void HandleJump()
    {
        isInFloor = Physics.CheckSphere(footPoint.transform.position, groundCheckRadius, floor); // Una espera que controla colicion con el piso

        if (isInFloor && gravityVector.y < 0) //Si estoy en el piso disminuyo la gravedad
        {
            gravityVector.y = wallSlideGravity;
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

    /// <summary>
    /// Handles dash input and execution. Allows the player to dash forward when moving.
    /// Includes cooldown management to prevent spam dashing. Now uses DOTween for better performance.
    /// </summary>
    private void HandleDash() //Dash para adelante apretando el shift, no esta del todo terminado
    {
        float CD = 1F;
        if (Time.time > dashCD &&
            Input.GetButtonDown("Fire3") && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)) // si presiono dash y me estoy moviendo, realizalo
        {
            _MC_.PlayerIsDashing();
            ExecuteDash();
            dashCD = Time.time + CD;
        }
    }
    
    /// <summary>
    /// Executes dash movement using DOTween for smooth and performant animation.
    /// </summary>
    private void ExecuteDash()
    {
        // Kill any existing dash tween
        dashTween?.Kill();
        
        // Calculate dash target position
        Vector3 dashTarget = transform.position + move * dashSpeed * dashTime;
        
        // Create smooth dash animation
        dashTween = transform.DOMove(dashTarget, dashTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = dashTarget;
                Vector3 movement = (nextPos - currentPos).normalized * dashSpeed * Time.deltaTime;
                characterController.Move(movement);
            });
    }




    /// <summary>
    /// Handles mouse input for camera rotation. Manages both horizontal (Y-axis) and vertical (X-axis) rotation
    /// with proper clamping to prevent over-rotation. Also handles camera reset functionality.
    /// </summary>
    private void HandleMouseInput()
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



    /// <summary>
    /// Dynamically adjusts camera FOV based on player state (wall sliding, ground contact, inertia).
    /// Creates a visual effect that enhances the sense of speed and movement using DOTween.
    /// Now with more gradual and controlled FOV changes that respond to inertia levels.
    /// </summary>
    public void ChangeFOV()
    {
        float targetFOV = normalFOV; // Start with normal FOV as base
        
        // Apply wall sliding FOV effect
        if (isInWall)
        {
            targetFOV += fovIncreaseRate * Time.deltaTime;
        }
        else if (isInFloor)
        {
            targetFOV -= fovDecreaseRate * Time.deltaTime;
        }
        
        // Apply inertia-based FOV effect
        if (inertia > 1.0f) // Only when inertia is above normal
        {
            float inertiaMultiplier = (inertia - 1.0f) / (inertiaMax - 1.0f); // Normalize to 0-1
            float inertiaFOVBoost = inertiaMultiplier * (maxFOV - normalFOV) * 0.5f; // Up to 50% of max boost
            targetFOV += inertiaFOVBoost;
        }
        
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
        
        // Only update if there's a significant change
        if (Mathf.Abs(targetFOV - inertiaFOV) > 0.1f)
        {
            // Kill existing FOV tween
            fovTween?.Kill();
            
            // Smooth FOV transition
            fovTween = DOTween.To(() => mainCamera.fieldOfView, 
                                  x => {
                                      mainCamera.fieldOfView = x;
                                      gunCameraComponent.fieldOfView = x;
                                      inertiaFOV = x;
                                  }, 
                                  targetFOV, 0.2f)
                              .SetEase(Ease.OutCubic);
        }
    }
    /// <summary>
    /// Handles camera tilt effect when wall sliding. Creates a dynamic camera rotation
    /// that responds to which wall the player is sliding on, enhancing immersion.
    /// </summary>
    public void HandleCameraTilt()
    {
        float targetTilt = 0f;
        
        if (isInWallLeft)
        {
            targetTilt = -maxCameraTilt;
        }
        else if (isInWallRight)
        {
            targetTilt = maxCameraTilt;
        }
        else
        {
            // Return to center when not on wall
            targetTilt = 0f;
        }
        
        // Smooth transition to target tilt
        cameraTiltAngle = Mathf.Lerp(cameraTiltAngle, targetTilt, Time.deltaTime * 3f);
        
        // Apply the rotation directly (absolute rotation)
        Vector3 currentRotation = camera.transform.eulerAngles;
        camera.transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, cameraTiltAngle);
    }


    public void CheckInertiaCharger()
    {
        RaycastHit hit;

        isInInertiaCharger = Physics.CheckSphere(footPoint.transform.position, groundCheckRadius, inertiaChargerLayer);

        if (isInInertiaCharger)
        {
            inertia = 1.5f;
            inertiaFOV += 2.5f;
        }
    }


    /// <summary>
    /// Detects wall collisions using raycasts from left and right wall points.
    /// Optimized to run at intervals rather than every frame for better performance.
    /// Updates wall state variables for camera tilt and movement systems.
    /// </summary>
    private void WallDetection()
    {
        // Only check walls at intervals to improve performance
        if (Time.time - lastWallCheckTime < wallCheckInterval) return;
        
        RaycastHit hitL;
        RaycastHit hitR;

        //Raycast para detectar colision con la pared
        if (
            (Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, wallCheckDistance, wall)) 
            ||
            Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, wallCheckDistance, wall))
        {
            isInWall = true;
        }
        else
        {
            isInWall = false;
        }

        //En Qu� pared estoy? Izq o Der
        if (Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), out hitL, wallCheckDistance, wall))
        {
            isInWallLeft = true;
        }
        else
        {
            isInWallLeft = false;
        }

        if (Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), out hitR, wallCheckDistance, wall))
        {
            isInWallRight = true;
        }
        else
        {
            isInWallRight = false;
        }
        
        lastWallCheckTime = Time.time;
    }



    /// <summary>
    /// Handles shotgun recoil using DOTween for smooth backward movement.
    /// </summary>
    private void DoShotgunRecoil(float recoilTime, float recoilSpeed)
    {
        ExecuteRecoil(recoilTime, recoilSpeed);
    }
    
    /// <summary>
    /// Executes recoil movement using DOTween for better performance and smoother animation.
    /// </summary>
    private void ExecuteRecoil(float recoilTime, float recoilSpeed)
    {
        // Kill any existing recoil tween
        recoilTween?.Kill();
        
        // Calculate recoil direction (opposite to camera forward)
        Vector3 recoilDirection = -camera.transform.forward;
        Vector3 recoilTarget = transform.position + recoilDirection * recoilSpeed * recoilTime;
        
        // Create smooth recoil animation
        recoilTween = transform.DOMove(recoilTarget, recoilTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = recoilTarget;
                Vector3 movement = (nextPos - currentPos).normalized * recoilSpeed * Time.deltaTime;
                characterController.Move(movement);
            })
            .OnComplete(() => {
                // Reset gravity vector after recoil
                gravityVector.y = wallSlideGravity;
            });
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




    bool isBeingThrown = false;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ramp"))
        {

            Vector3 throwDirection = hit.gameObject.transform.forward;
            RampController rampCont = hit.gameObject.GetComponent<RampController>();
            if (isBeingThrown == false)
            {
                ExecuteThrowPlayer(throwDirection, rampCont.GetRampPower(), rampCont.GetRampTime());
            }
            inertia = 1.5f;
            inertiaFOV += 2.5f;

        }


        if (hit.gameObject.CompareTag("FallingFloor"))
        {
            hit.gameObject.GetComponent<FallingFloorActivation>().ActivateFloor();
        }
    }

    /// <summary>
    /// Executes player throw using DOTween for smooth and performant animation.
    /// </summary>
    private void ExecuteThrowPlayer(Vector3 throwDirection, float rampPower, float rampTime)
    {
        // Kill any existing throw tween
        throwTween?.Kill();
        
        isBeingThrown = true;
        
        // Calculate throw target position
        Vector3 throwTarget = transform.position + throwDirection * rampPower * rampTime;
        
        // Create smooth throw animation
        throwTween = transform.DOMove(throwTarget, rampTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = throwTarget;
                Vector3 movement = (nextPos - currentPos).normalized * rampPower * Time.deltaTime;
                characterController.Move(movement);
            })
            .OnComplete(() => {
                // Reset gravity and throw state
                gravityVector.y = wallSlideGravity;
                isBeingThrown = false;
            });
    }




    private void OnDestroy()
    {
        ShotgunController.OnShotgunRecoil -= DoShotgunRecoil;
        
        // Kill all active tweens to prevent memory leaks
        dashTween?.Kill();
        recoilTween?.Kill();
        throwTween?.Kill();
        fovTween?.Kill();
        cameraTiltTween?.Kill();
    }

}
