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
/// ORQUESTADOR: Coordina todos los sistemas de física del jugador.
/// Este script ahora actúa como un coordinador que delega responsabilidades
/// a sistemas especializados más pequeños y manejables.
/// 
/// Refactorizado desde un archivo monolítico de 1090+ líneas a una arquitectura modular.
/// </summary>
public class PlayerPhysicsController : MonoBehaviour
{
    private PlayerController _MC_;
    private CharacterController characterController;
    private Camera mainCamera;
    private Camera gunCameraComponent;
    
    // Specialized Controllers
    private SurfaceDetector surfaceDetector;
    private GravityController gravityController;
    private InertiaController inertiaController;
    private JumpController jumpController;
    private DashController dashController;
    private PogoController pogoController;
    private PlayerMovementController movementController;
    private PlayerCameraController cameraController;
    private ExternalForceController externalForceController;
    private ClimbController climbController;
    
    // Serialized fields (keep for Inspector compatibility)
    [Header("Collision Detection")]
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float floorAngleThreshold = 45f;
    [SerializeField] private float wallAngleThreshold = 15f;
    [SerializeField] private LayerMask inertiaChargerLayer;
    
    [Header("RaycastReferences")]
    [SerializeField] private GameObject GunCamera;
    [SerializeField] private GameObject footPoint;
    [SerializeField] private GameObject wallPointL, wallPointR;
    
    [Header("References")]
    [SerializeField] private GameObject camera;
    
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
    [SerializeField] private float inertiaMin = 1f;
    [SerializeField] private float inertiaMax = 1.5f;
    
    [Header("Debug Controls")]
    [SerializeField] private bool useWASDControl = false;
    
    [Header("Pogo Mechanics")]
    [SerializeField] private float pogoDetectionRange = 6f;
    [SerializeField] private float pogoDetectionAngleDegrees = 72f;
    [SerializeField] private float pogoForceMultiplier = 0.8f;
    [SerializeField] private float pogoDirectionMultiplier = 0.6f;
    [SerializeField] private float pogoDurationMultiplier = 1.2f;
    [SerializeField] private float pogoFixedAngle = 60f;
    [SerializeField] private float pogoConsecutiveMultiplier = 1.2f;
    [SerializeField] private float pogoConsecutiveWindow = 1.0f;
    [SerializeField] private LayerMask pogoDetectionLayer = 256;
    
    [Header("Legacy Public Access")]
    public bool resetCamera = false;
    
    private void Awake()
    {
        _MC_ = GetComponent<PlayerController>();
    }
    
    void Start()
    {
        // Cache components
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        if (GunCamera != null)
        {
            gunCameraComponent = GunCamera.GetComponent<Camera>();
        }
        
        // Get settings from PlayerController
        float mouseSensibility = _MC_.playerSettings.mouseSensibility;
        float gravity = _MC_.playerSettings.gravity;
        float jump = _MC_.playerSettings.jumpValue;
        float dashSpeed = _MC_.playerSettings.dashSpeed;
        float dashTime = _MC_.playerSettings.dashTime;
        float moveSpeed = _MC_.playerSettings.moveSpeed;
        
        // Initialize specialized controllers
        InitializeControllers(mouseSensibility, gravity, jump, dashSpeed, dashTime, moveSpeed);
        
        // Subscribe to events
        ShotgunController.OnShotgunRecoil += DoShotgunRecoil;
        
        // Initialize inertia state
        if (_MC_ != null)
        {
            _MC_.PlayerInertiaAltered(InertiaController.GetInertia());
        }
    }
    
    /// <summary>
    /// Initialize all specialized controllers with their dependencies
    /// </summary>
    private void InitializeControllers(float mouseSens, float grav, float jumpVal, 
                                     float dashSpd, float dashTm, float moveSpd)
    {
        // 1. SurfaceDetector (base for others)
        surfaceDetector = gameObject.AddComponent<SurfaceDetector>();
        surfaceDetector.Setup(footPoint, wallPointL, wallPointR, collisionLayers,
                            floorAngleThreshold, wallAngleThreshold, groundCheckRadius, wallCheckDistance);
        
        // 2. GravityController
        gravityController = gameObject.AddComponent<GravityController>();
        gravityController.Setup(characterController, surfaceDetector, grav, wallSlideGravity);
        
        // 3. InertiaController
        inertiaController = gameObject.AddComponent<InertiaController>();
        inertiaController.Setup(surfaceDetector, footPoint, _MC_, inertiaIncreaseRate,
                              inertiaDecreaseRate, highInertiaThreshold, inertiaLerpSpeed,
                              inertiaMin, inertiaMax, inertiaChargerLayer, groundCheckRadius);
        
        // 4. DashController (needed before PogoController and JumpController uses it)
        dashController = gameObject.AddComponent<DashController>();
        dashController.Setup(characterController, mainCamera, _MC_, dashSpd, dashTm, 1f);
        
        // 5. ExternalForceController (needed before PogoController)
        externalForceController = gameObject.AddComponent<ExternalForceController>();
        externalForceController.Setup(characterController, gravityController, inertiaController,
                                    camera, wallSlideGravity);
        
        // 6. PogoController (depends on DashController and ExternalForceController)
        pogoController = gameObject.AddComponent<PogoController>();
        pogoController.Setup(mainCamera, characterController, dashController, null, // JumpController will be set after
                           externalForceController, surfaceDetector, _MC_, dashSpd, dashTm);
        
        // 7. ClimbController (depends on SurfaceDetector and GravityController)
        climbController = gameObject.AddComponent<ClimbController>();
        climbController.Setup(characterController, surfaceDetector, gravityController,
                            _MC_, footPoint, collisionLayers);
        
        // 8. JumpController (depends on all above, including ClimbController)
        jumpController = gameObject.AddComponent<JumpController>();
        jumpController.Setup(characterController, surfaceDetector, gravityController,
                           inertiaController, pogoController, _MC_, jumpVal, grav, climbController);
        
        // Update PogoController with JumpController reference
        pogoController.Setup(mainCamera, characterController, dashController, jumpController,
                           externalForceController, surfaceDetector, _MC_, dashSpd, dashTm);
        
        // 9. PlayerMovementController
        movementController = gameObject.AddComponent<PlayerMovementController>();
        movementController.Setup(characterController, inertiaController, camera, moveSpd, useWASDControl);
        
        // 10. PlayerCameraController
        cameraController = gameObject.AddComponent<PlayerCameraController>();
        cameraController.Setup(camera, GunCamera, mainCamera, surfaceDetector, inertiaController,
                             mouseSens, maxCameraTilt, fovIncreaseRate, fovDecreaseRate,
                             minFOV, maxFOV, normalFOV);
    }
    
    void Update()
    {
        if (_MC_.IsPlayerAlive() &&
            !GameManager.singletonGameManager.GetPausedStatus() &&
            !GameManager.singletonGameManager.isInCinematic)
        {
            // Delegate to specialized controllers
            if (jumpController != null) jumpController.HandleJump();
            if (dashController != null) dashController.HandleDash();
            if (pogoController != null) pogoController.CheckForPogo();
            if (cameraController != null) cameraController.HandleMouseInput();
            if (movementController != null && (climbController == null || !climbController.IsClimbing()))
            {
                // Solo mover si no estamos trepando
                movementController.HandleMovement();
            }
            if (inertiaController != null) inertiaController.UpdateInertia();
            if (cameraController != null) cameraController.ChangeFOV();
            if (cameraController != null) cameraController.HandleCameraTilt();
            
            // Actualizar trepar si está activo
            if (climbController != null)
            {
                if (climbController.IsClimbing())
                {
                    climbController.UpdateClimb();
                }
                else
                {
                    // Detectar bordes trepables cuando no estamos trepando
                    climbController.DetectClimbableEdge();
                }
            }
            else
            {
                // Log ocasional si climbController es null (solo una vez cada muchos frames)
                if (Time.frameCount % 300 == 0)
                {
                    Debug.LogWarning("[PlayerPhysicsController] Update: climbController es null!");
                }
            }
        }
        
        // Surface detection always runs
        if (surfaceDetector != null) surfaceDetector.DetectSurface();
        if (inertiaController != null) inertiaController.CheckInertiaCharger();
        
        // Apply wall slide gravity (pero no si estamos trepando)
        if (surfaceDetector != null && surfaceDetector.IsInWall && gravityController != null)
        {
            if (climbController == null || !climbController.IsClimbing())
            {
                gravityController.ApplyWallSlideGravity();
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (gravityController != null)
        {
            gravityController.ActiveGravity();
            if (!GameManager.singletonGameManager.GetPausedStatus() &&
                gravityController.IsGravityEnabled())
            {
                // No aplicar gravedad si estamos trepando
                if (climbController == null || !climbController.IsClimbing())
                {
                    gravityController.GravityForce();
                }
            }
        }
    }
    
    // ========== PUBLIC METHODS FOR COMPATIBILITY ==========
    
    public void UpdateInertia()
    {
        if (inertiaController != null)
        {
            inertiaController.UpdateInertia();
        }
    }
    
    public SurfaceType GetCurrentSurfaceType()
    {
        return surfaceDetector != null ? surfaceDetector.GetCurrentSurfaceType() : SurfaceType.None;
    }
    
    public Vector3 GetCurrentSurfaceNormal()
    {
        return surfaceDetector != null ? surfaceDetector.GetCurrentSurfaceNormal() : Vector3.up;
    }
    
    public float GetCurrentSurfaceAngle()
    {
        return surfaceDetector != null ? surfaceDetector.GetCurrentSurfaceAngle() : 0f;
    }
    
    public bool IsOnWalkableSurface()
    {
        return surfaceDetector != null ? surfaceDetector.IsOnWalkableSurface() : false;
    }
    
    public bool IsOnWallSurface()
    {
        return surfaceDetector != null ? surfaceDetector.IsOnWallSurface() : false;
    }
    
    public void ChangeMousesensibility(float value)
    {
        if (cameraController != null)
        {
            cameraController.ChangeMouseSensibility(value);
        }
    }
    
    public void ActiveGravity()
    {
        if (gravityController != null)
        {
            gravityController.ActiveGravity();
        }
    }
    
    public void ChangeFOV()
    {
        if (cameraController != null)
        {
            cameraController.ChangeFOV();
        }
    }
    
    public void HandleCameraTilt()
    {
        if (cameraController != null)
        {
            cameraController.HandleCameraTilt();
        }
    }
    
    public void CheckInertiaCharger()
    {
        if (inertiaController != null)
        {
            inertiaController.CheckInertiaCharger();
        }
    }
    
    private void WallDetection()
    {
        // Now handled by SurfaceDetector - kept for compatibility
    }
    
    // ========== EVENT HANDLERS ==========
    
    private void DoShotgunRecoil(float recoilTime, float recoilSpeed)
    {
        if (externalForceController != null)
        {
            externalForceController.ExecuteRecoil(recoilTime, recoilSpeed);
        }
    }
    
    // ========== UNITY CALLBACKS ==========
    
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
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Ramp"))
        {
            Vector3 throwDirection = hit.gameObject.transform.forward;
            RampController rampCont = hit.gameObject.GetComponent<RampController>();
            if (externalForceController != null && !externalForceController.IsBeingThrown())
            {
                externalForceController.HandleRampCollision(throwDirection, 
                                                          rampCont.GetRampPower(), 
                                                          rampCont.GetRampTime());
            }
        }
        
        if (hit.gameObject.CompareTag("FallingFloor"))
        {
            hit.gameObject.GetComponent<FallingFloorActivation>().ActivateFloor();
        }
    }
    
    private void OnDestroy()
    {
        ShotgunController.OnShotgunRecoil -= DoShotgunRecoil;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Gizmos are now handled by PogoController
        // This method is kept for compatibility but can delegate if needed
    }
}

