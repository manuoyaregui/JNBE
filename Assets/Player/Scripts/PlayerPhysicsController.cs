using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Tipos de superficie detectados por el sistema de colisiones inteligente
/// </summary>
public enum SurfaceType
{
    None,       // No hay superficie detectada
    Floor,      // Superficie horizontal (piso)
    Wall,       // Superficie vertical (pared)
    Ceiling,    // Superficie superior
    Slope       // Superficie inclinada (entre piso y pared)
}

/// <summary>
/// Handles all physics-related behavior for the player including movement, jumping, dashing,
/// wall detection, camera controls, and inertia system. This is the core physics controller
/// that manages player movement and environmental interactions.
/// </summary>
public class PlayerPhysicsController : MonoBehaviour
{
    PlayerController _MC_;


    [Header("Collision Detection")]
    [SerializeField] LayerMask collisionLayers = -1; // Todas las capas por defecto
    [SerializeField] private float floorAngleThreshold = 45f; // Ángulo máximo para considerar superficie como piso (en grados)
    [SerializeField] private float wallAngleThreshold = 15f; // Ángulo máximo para considerar superficie como pared (en grados)
    [SerializeField] private LayerMask inertiaChargerLayer; // Mantener para elementos especiales

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
    [SerializeField] private float maxCameraTilt = 15f;
    [SerializeField] private float fovIncreaseRate = 0.25f;
    [SerializeField] private float fovDecreaseRate = 0.35f;
    [SerializeField] private float minFOV = 55f;
    [SerializeField] private float maxFOV = 80f;
    [SerializeField] private float normalFOV = 65f;
    [SerializeField] private float inertiaLerpSpeed = 3f;
    
    [Header("Debug Controls")]
    [SerializeField] private bool useWASDControl = false; // Activa controles WASD en lugar de movimiento automático
    
    [Header("Pogo Mechanics")]
    [SerializeField] private float pogoDetectionRange = 6f; // Raycast distance for detecting poggable elements
    [SerializeField] private float pogoDetectionAngleDegrees = 72f; // FOV detection angle in degrees (72 = ~0.3 dot product)
    [SerializeField] private float pogoForceMultiplier = 0.8f; // Multiplier for pogo force strength
    [SerializeField] private float pogoDirectionMultiplier = 0.6f; // Multiplies the entire bounce direction
    [SerializeField] private float pogoDurationMultiplier = 1.2f; // Multiplier for pogo duration
    [SerializeField] private float pogoFixedAngle = 60f; // Fixed bounce angle in degrees (always bounces at this angle)
    [SerializeField] private float pogoConsecutiveMultiplier = 1.2f; // Boost for consecutive pogos (momentum system)
    [SerializeField] private float pogoConsecutiveWindow = 1.0f; // Time window for consecutive pogos
    [SerializeField] private LayerMask pogoDetectionLayer = 256; // Layer mask for raycast detection (Layer 8 = "PoggableAreas")

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
    
    //For Smart Collision Detection
    private SurfaceType currentSurfaceType = SurfaceType.None;
    private Vector3 currentSurfaceNormal = Vector3.up;
    private float currentSurfaceAngle = 0f;

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
    
    //For Pogo
    private bool isPogoActive = false;
    private float lastPogoTime = 0f;
    private int consecutivePogos = 0;
    
    /// <summary>
    /// Converts degrees to dot product threshold for FOV detection.
    /// </summary>
    private float PogoDetectionAngleDotProduct => Mathf.Cos(pogoDetectionAngleDegrees * Mathf.Deg2Rad);


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
            CheckForPogo();

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
    /// Player always moves forward automatically in the direction they are looking.
    /// No manual movement controls - only camera direction controls movement.
    /// Forward movement is multiplied by inertia for enhanced speed when airborne.
    /// Debug mode: Can toggle between automatic forward movement and WASD controls.
    /// </summary>
    private void HandleMovement() //Movimiento del personaje
    {
        Vector3 inputDirection = Vector3.zero;
        
        if (useWASDControl)
        {
            // Manual WASD controls
            float moveX = Input.GetAxis("Horizontal"); // A/D
            float moveZ = Input.GetAxis("Vertical");   // W/S
            
            // Calculate movement direction relative to camera
            Vector3 cameraForward = camera.transform.forward;
            Vector3 cameraRight = camera.transform.right;
            
            // Flatten the vectors to the ground plane
            cameraForward.y = 0;
            cameraRight.y = 0;
            
            cameraForward = cameraForward.normalized;
            cameraRight = cameraRight.normalized;
            
            inputDirection = (cameraForward * moveZ + cameraRight * moveX).normalized;
        }
        else
        {
            // Automatic forward movement (original behavior)
            inputDirection = transform.forward;
        }
        
        move = inputDirection * inertia; // Movimiento multiplicado por inercia
        
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
    /// Now uses intelligent surface detection based on surface angles instead of layer masks.
    /// </summary>
    private void HandleJump()
    {
        // Detectar superficie usando el nuevo sistema inteligente
        DetectSurface();
        
        // Determinar si estamos en una superficie válida para saltar
        bool canJumpFromSurface = currentSurfaceType == SurfaceType.Floor || 
                                 currentSurfaceType == SurfaceType.Slope ||
                                 currentSurfaceType == SurfaceType.Wall ||
                                 isInInertiaCharger;

        // Aplicar gravedad reducida cuando estamos en una superficie horizontal
        if ((currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope) && gravityVector.y < 0)
        {
            gravityVector.y = wallSlideGravity;
            
            // Reset momentum system when touching ground
            if (consecutivePogos > 0)
            {
                consecutivePogos = 0;
                Debug.Log("Momentum reset - touched ground");
            }
        }

        // Salto desde piso o pendiente
        if (Input.GetButtonDown("Jump") && (currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope))
        {
            _MC_.PlayerIsJumping();
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
        }
        
        // Salto desde pared o cargador de inercia
        if (Input.GetButtonDown("Jump") && (currentSurfaceType == SurfaceType.Wall || isInInertiaCharger))
        {
            _MC_.PlayerIsJumping();
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
        }

        // Doble salto
        if (Input.GetButtonDown("Jump") && !canJumpFromSurface && doubleJump == true)
        {
            _MC_.PlayerIsDoubleJumping();
            gravityVector.y = Mathf.Sqrt(jump * -2 * gravity);
            characterController.Move(gravityVector * Time.fixedDeltaTime);
            doubleJump = false;
        }
        
        // Resetear doble salto cuando tocamos una superficie válida
        if (canJumpFromSurface && doubleJump == false)
        {
            doubleJump = true;
        }
    }

    /// <summary>
    /// Sistema inteligente de detección de superficies basado en ángulos.
    /// Elimina la dependencia de layers específicos y maneja casos complejos como bordes.
    /// </summary>
    private void DetectSurface()
    {
        // Detectar superficie debajo del player (para piso/pendiente)
        DetectGroundSurface();
        
        // Detectar superficies laterales (para paredes)
        DetectWallSurfaces();
        
        // Actualizar variables legacy para compatibilidad
        UpdateLegacyVariables();
    }
    
    /// <summary>
    /// Detecta superficies debajo del player (piso, pendientes, techos)
    /// </summary>
    private void DetectGroundSurface()
    {
        RaycastHit hit;
        Vector3 rayOrigin = footPoint.transform.position;
        Vector3 rayDirection = Vector3.down;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, groundCheckRadius + 0.1f, collisionLayers))
        {
            currentSurfaceNormal = hit.normal;
            currentSurfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            
            // Determinar tipo de superficie basado en el ángulo
            if (currentSurfaceAngle <= floorAngleThreshold)
            {
                currentSurfaceType = SurfaceType.Floor;
            }
            else if (currentSurfaceAngle <= 90f - wallAngleThreshold)
            {
                currentSurfaceType = SurfaceType.Slope;
            }
            else if (currentSurfaceAngle <= 90f + wallAngleThreshold)
            {
                currentSurfaceType = SurfaceType.Wall;
            }
            else
            {
                currentSurfaceType = SurfaceType.Ceiling;
            }
        }
        else
        {
            currentSurfaceType = SurfaceType.None;
            currentSurfaceNormal = Vector3.up;
            currentSurfaceAngle = 0f;
        }
    }
    
    /// <summary>
    /// Detecta superficies laterales para determinar si estamos en una pared
    /// </summary>
    private void DetectWallSurfaces()
    {
        RaycastHit hitL, hitR;
        bool leftWall = Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), 
                                      out hitL, wallCheckDistance, collisionLayers);
        bool rightWall = Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), 
                                       out hitR, wallCheckDistance, collisionLayers);
        
        // Determinar qué pared estamos tocando
        if (leftWall || rightWall)
        {
            // Si estamos tocando una pared lateral Y no hay superficie debajo, es una pared
            if (currentSurfaceType == SurfaceType.None || currentSurfaceType == SurfaceType.Ceiling)
            {
                currentSurfaceType = SurfaceType.Wall;
            }
            // Si hay superficie debajo, usar sistema de prioridades
            else if (currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope)
            {
                // Caso especial: estamos en un borde (tanto piso como pared)
                // Priorizar el piso si el ángulo es menor al threshold
                if (currentSurfaceAngle <= floorAngleThreshold)
                {
                    // Mantener como Floor, pero marcar que también hay pared
                    isInWallLeft = leftWall;
                    isInWallRight = rightWall;
                    return;
                }
                else
                {
                    // Si el ángulo es mayor, tratarlo como pared
                    currentSurfaceType = SurfaceType.Wall;
                }
            }
        }
        
        isInWallLeft = leftWall;
        isInWallRight = rightWall;
    }
    
    /// <summary>
    /// Actualiza las variables legacy para mantener compatibilidad con el código existente
    /// </summary>
    private void UpdateLegacyVariables()
    {
        // Actualizar isInFloor basado en el nuevo sistema
        isInFloor = (currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope);
        
        // Actualizar isInWall basado en el nuevo sistema
        isInWall = (currentSurfaceType == SurfaceType.Wall || isInWallLeft || isInWallRight);
    }
    
    // Métodos públicos para obtener información sobre la superficie actual
    public SurfaceType GetCurrentSurfaceType() => currentSurfaceType;
    public Vector3 GetCurrentSurfaceNormal() => currentSurfaceNormal;
    public float GetCurrentSurfaceAngle() => currentSurfaceAngle;
    public bool IsOnWalkableSurface() => currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope;
    public bool IsOnWallSurface() => currentSurfaceType == SurfaceType.Wall;

    /// <summary>
    /// Handles dash input and execution. Allows the player to dash in camera direction.
    /// Includes cooldown management to prevent spam dashing. Now uses DOTween for better performance.
    /// </summary>
    private void HandleDash() //Dash en dirección de la cámara apretando el shift
    {
        float CD = 1F;
        if (Time.time > dashCD && Input.GetButtonDown("Fire3")) // Solo requiere presionar el botón
        {
            _MC_.PlayerIsDashing();
            ExecuteDash();
            dashCD = Time.time + CD;
        }
    }
    
    /// <summary>
    /// Executes dash movement using DOTween for smooth and performant animation.
    /// Dash always goes in the direction the player is looking (camera direction).
    /// </summary>
    private void ExecuteDash()
    {
        // Kill any existing dash tween
        dashTween?.Kill();
        
        // Get camera forward direction (where player is looking)
        Vector3 dashDirection = mainCamera.transform.forward;
        
        // Calculate dash target position
        Vector3 dashTarget = transform.position + dashDirection * dashSpeed * dashTime;
        
        // Create smooth dash animation
        dashTween = transform.DOMove(dashTarget, dashTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = dashTarget;
                Vector3 movement = (nextPos - currentPos).normalized * dashSpeed * Time.deltaTime;
                characterController.Move(movement);
            })
            .OnComplete(() => {
                // Resetear la animación de dash cuando termine
                _MC_.StopDashAnimation();
            });
    }
    
    /// <summary>
    /// Checks for pogo opportunities when dashing towards poggable elements.
    /// Uses OverlapSphere to detect elements with "PoggableElement" tag within camera field of view.
    /// Now uses a wide field of view for easier pogo activation.
    /// </summary>
    private void CheckForPogo()
    {
        // Only check for pogo if we're actively dashing (dashTween is active)
        if (isPogoActive || dashTween == null || !dashTween.IsActive()) return;
        
        // Get camera forward direction (where player is looking)
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // Only horizontal direction
        cameraForward = cameraForward.normalized;
        
        // Find all poggable elements within detection range
        Collider[] poggableElements = Physics.OverlapSphere(transform.position, pogoDetectionRange, pogoDetectionLayer);
        
        Transform bestElement = null;
        float bestScore = -1f;
        
        foreach (Collider element in poggableElements)
        {
            if (element.CompareTag("PoggableElement"))
            {
                // Calculate direction from player to poggable element
                Vector3 directionToElement = (element.transform.position - transform.position).normalized;
                
                // Calculate angle between camera direction and element direction
                float dotProduct = Vector3.Dot(cameraForward, directionToElement);
                
                // Wide field of view detection - covers most of what the player can see
                // Uses configurable angle in degrees converted to dot product threshold
                if (dotProduct > PogoDetectionAngleDotProduct)
                {
                    // Calculate distance score (closer elements get higher score)
                    float distance = Vector3.Distance(transform.position, element.transform.position);
                    float distanceScore = 1f - (distance / pogoDetectionRange);
                    
                    // Calculate angle score (elements more centered get higher score)
                    float angleScore = dotProduct; // Use dot product directly as angle score
                    
                    // Combined score (weighted towards angle for better feel)
                    float totalScore = (angleScore * 0.6f) + (distanceScore * 0.4f);
                    
                    if (totalScore > bestScore)
                    {
                        bestScore = totalScore;
                        bestElement = element.transform;
                    }
                }
            }
        }
        
        // Execute pogo with the best element found
        if (bestElement != null)
        {
            ExecutePogo(cameraForward);
        }
    }
    
    /// <summary>
    /// Executes pogo bounce using a fixed angle of 60 degrees upward.
    /// Always bounces at the same angle regardless of element position or dash direction.
    /// Now includes momentum system for consecutive pogos.
    /// </summary>
    private void ExecutePogo(Vector3 dashDirection)
    {
        if (isPogoActive) return;
        
        isPogoActive = true;
        
        // Kill any existing dash before applying pogo
        dashTween?.Kill();
        
        // Resetear la animación de dash cuando se ejecuta pogo
        _MC_.StopDashAnimation();
        
        // Stop all horizontal movement immediately to prevent delay effect
        characterController.Move(Vector3.zero);
        
        // MOMENTUM SYSTEM: Always increment consecutive pogos (only resets when touching ground)
        consecutivePogos++;
        
        // Calculate momentum multiplier for consecutive pogos
        float momentumMultiplier = 1f;
        if (consecutivePogos > 1)
        {
            // Apply exponential boost: base_multiplier ^ (consecutive_pogos - 1)
            momentumMultiplier = Mathf.Pow(pogoConsecutiveMultiplier, consecutivePogos - 1);
        }
        
        // Create bounce direction with fixed angle
        Vector3 bounceDirection = dashDirection; // Start with dash direction for horizontal component
        bounceDirection.y = 0; // Reset Y to 0 first
        
        // Apply fixed bounce angle (60 degrees upward)
        float angleInRadians = pogoFixedAngle * Mathf.Deg2Rad;
        bounceDirection.y = Mathf.Tan(angleInRadians) * bounceDirection.magnitude;
        
        // Normalize the direction
        bounceDirection = bounceDirection.normalized;
        
        // Apply direction multiplier to the entire bounce direction
        bounceDirection *= pogoDirectionMultiplier;
        
        // Calculate pogo force with momentum multiplier
        float pogoForce = dashSpeed * pogoForceMultiplier * momentumMultiplier;
        float pogoDuration = dashTime * pogoDurationMultiplier;
        
        // Debug log for momentum system
        if (consecutivePogos > 1)
        {
            Debug.Log($"Consecutive Pogo #{consecutivePogos}! Force multiplier: {momentumMultiplier:F2}x");
        }
        
        // Use existing ExecuteThrowPlayer method
        ExecuteThrowPlayer(bounceDirection, pogoForce, pogoDuration);
        
        // Reset abilities on successful pogo
        ResetAbilitiesOnPogo();
        
        // Record the time of this pogo for momentum system
        lastPogoTime = Time.time;
        
        // Reset pogo active after duration
        StartCoroutine(ResetPogoActive(pogoDuration + 0.5f));
    }
    
    /// <summary>
    /// Resets pogo active state after a delay.
    /// </summary>
    private System.Collections.IEnumerator ResetPogoActive(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPogoActive = false;
    }
    
    
    /// <summary>
    /// Resets dash cooldown and double jump when pogo is successful.
    /// </summary>
    private void ResetAbilitiesOnPogo()
    {
        // Reset dash cooldown
        dashCD = 0f;
        
        // Reset double jump
        doubleJump = true;
        
        Debug.Log("Pogo successful! Dash and double jump reset.");
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
    /// Detects wall collisions using the new intelligent surface detection system.
    /// Now integrated into DetectSurface() method for better performance and accuracy.
    /// </summary>
    private void WallDetection()
    {
        // El nuevo sistema de detección ya maneja las paredes en DetectSurface()
        // Este método se mantiene para compatibilidad pero ahora es redundante
        // La detección se hace en DetectWallSurfaces() dentro de DetectSurface()
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

    /// <summary>
    /// Draws gizmos to visualize pogo detection ranges and mechanics.
    /// Shows detection sphere, field of view cone, bounce direction, and momentum visualization.
    /// Only visible in Scene View when object is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;
        
        // 1. Pogo Detection Range Sphere
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green
        Gizmos.DrawSphere(transform.position, pogoDetectionRange);
        
        // 2. Pogo Detection Range Wireframe
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pogoDetectionRange);
        
        // 3. Camera Direction and Field of View Cone
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // Only horizontal direction
        cameraForward = cameraForward.normalized;
        
        // Draw camera direction line
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + cameraForward * pogoDetectionRange);
        
        // Draw field of view cone (based on configurable angle in degrees)
        // Use the angle directly since it's already in degrees
        float fovAngle = pogoDetectionAngleDegrees;
        float halfFov = fovAngle * 0.5f;
        float coneLength = pogoDetectionRange;
        
        // Left edge of FOV cone
        Vector3 leftEdge = Quaternion.AngleAxis(-halfFov, Vector3.up) * cameraForward;
        Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + leftEdge * coneLength);
        
        // Right edge of FOV cone
        Vector3 rightEdge = Quaternion.AngleAxis(halfFov, Vector3.up) * cameraForward;
        Gizmos.DrawLine(transform.position, transform.position + rightEdge * coneLength);
        
        // Arc to show FOV cone
        DrawFOVArc(transform.position, cameraForward, halfFov, coneLength, 20);
        
        // 4. Pogo Bounce Direction Visualization (when dashing)
        if (dashTween != null && dashTween.IsActive())
        {
            // Calculate bounce direction with fixed angle
            Vector3 bounceDirection = cameraForward;
            bounceDirection.y = 0;
            float angleInRadians = pogoFixedAngle * Mathf.Deg2Rad;
            bounceDirection.y = Mathf.Tan(angleInRadians) * bounceDirection.magnitude;
            bounceDirection = bounceDirection.normalized;
            bounceDirection *= pogoDirectionMultiplier;
            
            // Draw bounce direction
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + bounceDirection * 3f);
            
            // Draw bounce angle indicator
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Vector3 bounceStart = transform.position;
            Vector3 bounceEnd = transform.position + bounceDirection * 3f;
            Vector3 bounceUp = Vector3.up * 2f;
            Gizmos.DrawLine(bounceStart, bounceStart + bounceUp);
            Gizmos.DrawLine(bounceStart + bounceUp, bounceEnd);
        }
        
        // 5. Consecutive Pogo Momentum Visualization
        if (consecutivePogos > 0)
        {
            // Draw momentum indicator
            float momentumSize = 0.5f + (consecutivePogos * 0.2f);
            Gizmos.color = new Color(1f, 1f, 0f, 0.8f); // Yellow for momentum
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, momentumSize);
            
            // Draw momentum text (this won't show in scene view, but helps with debugging)
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"Momentum: {consecutivePogos}x");
            #endif
        }
        
        // 6. Poggable Elements Detection Visualization
        Collider[] poggableElements = Physics.OverlapSphere(transform.position, pogoDetectionRange, pogoDetectionLayer);
        foreach (Collider element in poggableElements)
        {
            if (element.CompareTag("PoggableElement"))
            {
                // Calculate direction from player to poggable element
                Vector3 directionToElement = (element.transform.position - transform.position).normalized;
                
                // Calculate angle between camera direction and element direction
                float dotProduct = Vector3.Dot(cameraForward, directionToElement);
                
                // Color based on whether it's in FOV
                if (dotProduct > PogoDetectionAngleDotProduct)
                {
                    Gizmos.color = Color.green; // In FOV
                }
                else
                {
                    Gizmos.color = Color.gray; // Out of FOV
                }
                
                // Draw line to poggable element
                Gizmos.DrawLine(transform.position, element.transform.position);
                
                // Draw small sphere at element position
                Gizmos.DrawWireSphere(element.transform.position, 0.3f);
            }
        }
    }
    
    /// <summary>
    /// Helper method to draw FOV arc for better visualization.
    /// </summary>
    private void DrawFOVArc(Vector3 center, Vector3 forward, float halfAngle, float radius, int segments)
    {
        Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
        
        Vector3 previousPoint = center + Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (halfAngle * 2f * i / segments);
            Vector3 currentPoint = center + Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
            
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

}
