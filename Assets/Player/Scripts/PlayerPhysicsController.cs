using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ========== MODULE SETTINGS STRUCTURES ==========

[System.Serializable]
public class ClimbModuleSettings
{
    public float climbDetectionHeightRatio = 6f;
    public float climbCheckForward = 4f;
    public float climbUpHeight = 4f;
    public float climbSpeed = 5f;
    public bool autoClimb = true;
    public LayerMask climbableLayers = -1;
    public float maxClimbDuration = 3f;
    public float stuckDetectionTime = 0.3f;
    public float minProgressDistance = 0.05f;
}

[System.Serializable]
public class JumpModuleSettings
{
    public float jumpValue = 5f; // Default from JumpController, will be overridden by PlayerSettings if available
    public float gravity = -9.81f; // Default from JumpController, will be overridden by PlayerSettings if available
}

[System.Serializable]
public class DashModuleSettings
{
    public float dashSpeed = 20f; // Default from DashController, will be overridden by PlayerSettings if available
    public float dashTime = 0.3f; // Default from DashController, will be overridden by PlayerSettings if available
    public float dashCooldown = 1f; // Default from DashController
}

[System.Serializable]
public class PogoModuleSettings
{
    public float pogoDetectionRange = 6f;
    public float pogoDetectionAngleDegrees = 72f;
    public float pogoForceMultiplier = 0.8f;
    public float pogoDirectionMultiplier = 0.6f;
    public float pogoDurationMultiplier = 1.2f;
    public float pogoFixedAngle = 60f;
    public float pogoConsecutiveMultiplier = 1.2f;
    public float pogoConsecutiveWindow = 1.0f;
    public LayerMask pogoDetectionLayer = 256;
}

[System.Serializable]
public class GravityModuleSettings
{
    public float gravity = -9.81f; // Default from GravityController, will be overridden by PlayerSettings if available
    public float wallSlideGravity = -2f; // Default from GravityController
}

[System.Serializable]
public class InertiaModuleSettings
{
    public float inertiaIncreaseRate = 0.002f;
    public float inertiaDecreaseRate = 0.025f;
    public float highInertiaThreshold = 1.3f;
    public float inertiaLerpSpeed = 3f;
    public float inertiaMin = 1f;
    public float inertiaMax = 1.5f;
    public LayerMask inertiaChargerLayer;
    public float groundCheckRadius = 0.2f;
}

[System.Serializable]
public class CameraModuleSettings
{
    public float mouseSensibility = 2f; // Default from PlayerCameraController, will be overridden by PlayerSettings if available
    public float maxCameraTilt = 15f; // Default from PlayerCameraController
    public float fovIncreaseRate = 0.25f; // Default from PlayerCameraController
    public float fovDecreaseRate = 0.35f; // Default from PlayerCameraController
    public float minFOV = 55f; // Default from PlayerCameraController
    public float maxFOV = 80f; // Default from PlayerCameraController
    public float normalFOV = 65f; // Default from PlayerCameraController
}

[System.Serializable]
public class MovementModuleSettings
{
    public float moveSpeed = 5f; // Default from PlayerMovementController, will be overridden by PlayerSettings if available
    public bool useWASDControl = false; // Default from PlayerMovementController
}

[System.Serializable]
public class ExternalForceModuleSettings
{
    public float wallSlideGravity = -2f;
}

/// <summary>
/// ORQUESTADOR: Coordina todos los sistemas de física del jugador.
/// Este script ahora actúa como un coordinador que delega responsabilidades
/// a sistemas especializados más pequeños y manejables.
/// 
/// Refactorizado desde un archivo monolítico de 1090+ líneas a una arquitectura modular.
/// Sistema modular con checkboxes para habilitar/deshabilitar módulos individuales.
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
    
    // ========== CORE SYSTEMS (Always Active) ==========
    [Header("Core Systems (Always Active)")]
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float floorAngleThreshold = 45f;
    [SerializeField] private float wallAngleThreshold = 15f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    
    [Header("References")]
    [SerializeField] private GameObject GunCamera;
    [SerializeField] private GameObject footPoint;
    [SerializeField] private GameObject wallPointL, wallPointR;
    [SerializeField] private GameObject camera;
    
    // ========== MODULE ENABLE/DISABLE CHECKBOXES ==========
    [Header("Module Toggles")]
    [SerializeField] private bool enableGravity = true;
    [SerializeField] private bool enableInertia = true;
    [SerializeField] private bool enableJump = true;
    [SerializeField] private bool enableDash = true;
    [SerializeField] private bool enablePogo = true;
    [SerializeField] private bool enableClimb = true;
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private bool enableCamera = true;
    [SerializeField] private bool enableExternalForce = true;
    
    // ========== MODULE SETTINGS ==========
    [SerializeField] private GravityModuleSettings gravitySettings = new GravityModuleSettings();
    [SerializeField] private InertiaModuleSettings inertiaSettings = new InertiaModuleSettings();
    [SerializeField] private ExternalForceModuleSettings externalForceSettings = new ExternalForceModuleSettings();
    [SerializeField] private JumpModuleSettings jumpSettings = new JumpModuleSettings();
    [SerializeField] private DashModuleSettings dashSettings = new DashModuleSettings();
    [SerializeField] private PogoModuleSettings pogoSettings = new PogoModuleSettings();
    [SerializeField] private ClimbModuleSettings climbSettings = new ClimbModuleSettings();
    [SerializeField] private MovementModuleSettings movementSettings = new MovementModuleSettings();
    [SerializeField] private CameraModuleSettings cameraSettings = new CameraModuleSettings();
    
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
        
        // Override with values from PlayerController (original behavior)
        // This matches what was done before refactoring
        if (_MC_ != null && _MC_.playerSettings != null)
        {
            jumpSettings.jumpValue = _MC_.playerSettings.jumpValue;
            gravitySettings.gravity = _MC_.playerSettings.gravity;
            jumpSettings.gravity = _MC_.playerSettings.gravity; // Jump also uses gravity from playerSettings
            dashSettings.dashSpeed = _MC_.playerSettings.dashSpeed;
            dashSettings.dashTime = _MC_.playerSettings.dashTime;
            movementSettings.moveSpeed = _MC_.playerSettings.moveSpeed;
            cameraSettings.mouseSensibility = _MC_.playerSettings.mouseSensibility;
        }
        
        // Initialize specialized controllers
        InitializeControllers();
        
        // Subscribe to events
        ShotgunController.OnShotgunRecoil += DoShotgunRecoil;
        
        // Initialize inertia state
        if (_MC_ != null && inertiaController != null)
        {
            _MC_.PlayerInertiaAltered(InertiaController.GetInertia());
        }
    }
    
    /// <summary>
    /// Initialize all specialized controllers with their dependencies
    /// Only instantiates controllers if their enable checkbox is checked
    /// </summary>
    private void InitializeControllers()
    {
        // 1. SurfaceDetector (base for others - always active)
        surfaceDetector = gameObject.AddComponent<SurfaceDetector>();
        surfaceDetector.Setup(footPoint, wallPointL, wallPointR, collisionLayers,
                            floorAngleThreshold, wallAngleThreshold, groundCheckRadius, wallCheckDistance);
        
        // 2. GravityController
        if (enableGravity)
        {
            gravityController = gameObject.AddComponent<GravityController>();
            gravityController.Setup(characterController, surfaceDetector, 
                                  gravitySettings.gravity, gravitySettings.wallSlideGravity);
        }
        
        // 3. InertiaController
        if (enableInertia)
        {
            inertiaController = gameObject.AddComponent<InertiaController>();
            inertiaController.Setup(surfaceDetector, footPoint, _MC_, 
                                  inertiaSettings.inertiaIncreaseRate,
                                  inertiaSettings.inertiaDecreaseRate, 
                                  inertiaSettings.highInertiaThreshold, 
                                  inertiaSettings.inertiaLerpSpeed,
                                  inertiaSettings.inertiaMin, 
                                  inertiaSettings.inertiaMax, 
                                  inertiaSettings.inertiaChargerLayer, 
                                  inertiaSettings.groundCheckRadius);
        }
        
        // 4. DashController (needed before PogoController and JumpController uses it)
        if (enableDash)
        {
            dashController = gameObject.AddComponent<DashController>();
            dashController.Setup(characterController, mainCamera, _MC_, 
                               dashSettings.dashSpeed, dashSettings.dashTime, dashSettings.dashCooldown);
        }
        
        // 5. ExternalForceController (needed before PogoController)
        if (enableExternalForce)
        {
            externalForceController = gameObject.AddComponent<ExternalForceController>();
            externalForceController.Setup(characterController, gravityController, inertiaController,
                                        camera, externalForceSettings.wallSlideGravity);
        }
        
        // 6. PogoController (depends on DashController and ExternalForceController)
        if (enablePogo && dashController != null && externalForceController != null)
        {
            pogoController = gameObject.AddComponent<PogoController>();
            pogoController.Setup(mainCamera, characterController, dashController, null, // JumpController will be set after
                               externalForceController, surfaceDetector, _MC_, 
                               dashSettings.dashSpeed, dashSettings.dashTime);
            // Configure PogoController properties using reflection
            ConfigurePogoController(pogoController, pogoSettings);
        }
        else if (enablePogo)
        {
            Debug.LogWarning("PogoController requires DashController and ExternalForceController. Disabling Pogo.");
        }
        
        // 7. ClimbController (depends on SurfaceDetector and GravityController)
        if (enableClimb && gravityController != null)
        {
            climbController = gameObject.AddComponent<ClimbController>();
            climbController.Setup(characterController, surfaceDetector, gravityController,
                                _MC_, footPoint, climbSettings.climbableLayers);
            // Configure ClimbController properties using reflection
            ConfigureClimbController(climbController, climbSettings);
        }
        else if (enableClimb)
        {
            Debug.LogWarning("ClimbController requires GravityController. Disabling Climb.");
        }
        
        // 8. JumpController (depends on GravityController and optionally others)
        if (enableJump && gravityController != null)
        {
            jumpController = gameObject.AddComponent<JumpController>();
            jumpController.Setup(characterController, surfaceDetector, gravityController,
                              inertiaController, pogoController, _MC_, 
                              jumpSettings.jumpValue, jumpSettings.gravity, climbController);
        }
        else if (enableJump)
        {
            Debug.LogWarning("JumpController requires GravityController. Disabling Jump.");
        }
        
        // Update PogoController with JumpController reference if both are active
        if (pogoController != null && jumpController != null)
        {
            pogoController.Setup(mainCamera, characterController, dashController, jumpController,
                               externalForceController, surfaceDetector, _MC_, 
                               dashSettings.dashSpeed, dashSettings.dashTime);
        }
        
        // 9. PlayerMovementController
        if (enableMovement)
        {
            movementController = gameObject.AddComponent<PlayerMovementController>();
            movementController.Setup(characterController, inertiaController, camera, 
                                   movementSettings.moveSpeed, movementSettings.useWASDControl);
        }
        
        // 10. PlayerCameraController
        if (enableCamera)
        {
            cameraController = gameObject.AddComponent<PlayerCameraController>();
            cameraController.Setup(camera, GunCamera, mainCamera, surfaceDetector, inertiaController,
                                 cameraSettings.mouseSensibility, cameraSettings.maxCameraTilt, 
                                 cameraSettings.fovIncreaseRate, cameraSettings.fovDecreaseRate,
                                 cameraSettings.minFOV, cameraSettings.maxFOV, cameraSettings.normalFOV);
        }
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
    
    // ========== HELPER METHODS FOR CONFIGURATION ==========
    
    /// <summary>
    /// Configures ClimbController properties from settings using reflection
    /// </summary>
    private void ConfigureClimbController(ClimbController controller, ClimbModuleSettings settings)
    {
        var type = typeof(ClimbController);
        
        // Set climb detection settings
        var climbDetectionHeightRatioField = type.GetField("climbDetectionHeightRatio", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (climbDetectionHeightRatioField != null)
            climbDetectionHeightRatioField.SetValue(controller, settings.climbDetectionHeightRatio);
        
        var climbCheckForwardField = type.GetField("climbCheckForward", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (climbCheckForwardField != null)
            climbCheckForwardField.SetValue(controller, settings.climbCheckForward);
        
        var climbUpHeightField = type.GetField("climbUpHeight", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (climbUpHeightField != null)
            climbUpHeightField.SetValue(controller, settings.climbUpHeight);
        
        var climbSpeedField = type.GetField("climbSpeed", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (climbSpeedField != null)
            climbSpeedField.SetValue(controller, settings.climbSpeed);
        
        var autoClimbField = type.GetField("autoClimb", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (autoClimbField != null)
            autoClimbField.SetValue(controller, settings.autoClimb);
        
        // Set anti-stuck settings
        var maxClimbDurationField = type.GetField("maxClimbDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (maxClimbDurationField != null)
            maxClimbDurationField.SetValue(controller, settings.maxClimbDuration);
        
        var stuckDetectionTimeField = type.GetField("stuckDetectionTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (stuckDetectionTimeField != null)
            stuckDetectionTimeField.SetValue(controller, settings.stuckDetectionTime);
        
        var minProgressDistanceField = type.GetField("minProgressDistance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (minProgressDistanceField != null)
            minProgressDistanceField.SetValue(controller, settings.minProgressDistance);
    }
    
    /// <summary>
    /// Configures PogoController properties from settings using reflection
    /// </summary>
    private void ConfigurePogoController(PogoController controller, PogoModuleSettings settings)
    {
        var type = typeof(PogoController);
        
        // Set pogo mechanics
        var pogoDetectionRangeField = type.GetField("pogoDetectionRange", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoDetectionRangeField != null)
            pogoDetectionRangeField.SetValue(controller, settings.pogoDetectionRange);
        
        var pogoDetectionAngleDegreesField = type.GetField("pogoDetectionAngleDegrees", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoDetectionAngleDegreesField != null)
            pogoDetectionAngleDegreesField.SetValue(controller, settings.pogoDetectionAngleDegrees);
        
        var pogoForceMultiplierField = type.GetField("pogoForceMultiplier", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoForceMultiplierField != null)
            pogoForceMultiplierField.SetValue(controller, settings.pogoForceMultiplier);
        
        var pogoDirectionMultiplierField = type.GetField("pogoDirectionMultiplier", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoDirectionMultiplierField != null)
            pogoDirectionMultiplierField.SetValue(controller, settings.pogoDirectionMultiplier);
        
        var pogoDurationMultiplierField = type.GetField("pogoDurationMultiplier", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoDurationMultiplierField != null)
            pogoDurationMultiplierField.SetValue(controller, settings.pogoDurationMultiplier);
        
        var pogoFixedAngleField = type.GetField("pogoFixedAngle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoFixedAngleField != null)
            pogoFixedAngleField.SetValue(controller, settings.pogoFixedAngle);
        
        var pogoConsecutiveMultiplierField = type.GetField("pogoConsecutiveMultiplier", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoConsecutiveMultiplierField != null)
            pogoConsecutiveMultiplierField.SetValue(controller, settings.pogoConsecutiveMultiplier);
        
        var pogoConsecutiveWindowField = type.GetField("pogoConsecutiveWindow", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoConsecutiveWindowField != null)
            pogoConsecutiveWindowField.SetValue(controller, settings.pogoConsecutiveWindow);
        
        var pogoDetectionLayerField = type.GetField("pogoDetectionLayer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pogoDetectionLayerField != null)
            pogoDetectionLayerField.SetValue(controller, settings.pogoDetectionLayer);
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

