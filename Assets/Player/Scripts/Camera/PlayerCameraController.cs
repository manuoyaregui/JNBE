using UnityEngine;
using DG.Tweening;

/// <summary>
/// Sistema de control de cámara del jugador. Maneja rotación del mouse,
/// ajuste dinámico de FOV basado en estado del jugador (deslizamiento en pared,
/// contacto con el suelo, inercia), y efecto de tilt de cámara al deslizar por paredes.
/// Usa DOTween para transiciones suaves de FOV.
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensibility = 2f;
    [SerializeField] private float maxCameraTilt = 15f;
    
    [Header("FOV Settings")]
    [SerializeField] private float fovIncreaseRate = 0.25f;
    [SerializeField] private float fovDecreaseRate = 0.35f;
    [SerializeField] private float minFOV = 55f;
    [SerializeField] private float maxFOV = 80f;
    [SerializeField] private float normalFOV = 65f;
    
    private GameObject camera;
    private GameObject gunCamera;
    private Camera mainCamera;
    private Camera gunCameraComponent;
    private SurfaceDetector surfaceDetector;
    private InertiaController inertiaController;
    
    private float rotationX = 0f;
    private float cameraTiltAngle = 0f;
    private float inertiaFOV = 50f;
    private bool resetCamera = false;
    
    private Tween fovTween;
    private Tween cameraTiltTween;
    
    /// <summary>
    /// Initialize camera controller with required dependencies
    /// </summary>
    public void Setup(GameObject cameraRef, GameObject gunCameraRef, Camera mainCam,
                     SurfaceDetector detector, InertiaController inertiaCtrl,
                     float sensibility, float tilt, float fovIncRate, float fovDecRate,
                     float minFov, float maxFov, float normFov)
    {
        camera = cameraRef;
        gunCamera = gunCameraRef;
        mainCamera = mainCam;
        surfaceDetector = detector;
        inertiaController = inertiaCtrl;
        mouseSensibility = sensibility;
        maxCameraTilt = tilt;
        fovIncreaseRate = fovIncRate;
        fovDecreaseRate = fovDecRate;
        minFOV = minFov;
        maxFOV = maxFov;
        normalFOV = normFov;
        
        if (gunCamera != null)
        {
            gunCameraComponent = gunCamera.GetComponent<Camera>();
        }
    }
    
    /// <summary>
    /// Handles mouse input for camera rotation. Manages both horizontal (Y-axis) and vertical (X-axis) rotation
    /// with proper clamping to prevent over-rotation. Also handles camera reset functionality.
    /// </summary>
    public void HandleMouseInput()
    {
        if (camera == null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensibility * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensibility * Time.fixedDeltaTime;

        if (resetCamera == true)
        {
            rotationX = 0;
            resetCamera = false;
        }

        rotationX += mouseY;
        rotationX = Mathf.Clamp(rotationX, -90, 90); // Clampeo la rotacion del mouse en Y

        camera.transform.localRotation = Quaternion.Euler(rotationX * -1, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
    
    /// <summary>
    /// Dynamically adjusts camera FOV based on player state (wall sliding, ground contact, inertia).
    /// Creates a visual effect that enhances the sense of speed and movement using DOTween.
    /// Now with more gradual and controlled FOV changes that respond to inertia levels.
    /// </summary>
    public void ChangeFOV()
    {
        if (mainCamera == null || surfaceDetector == null || inertiaController == null) return;
        
        float targetFOV = normalFOV; // Start with normal FOV as base
        
        // Apply wall sliding FOV effect
        if (surfaceDetector.IsInWall)
        {
            targetFOV += fovIncreaseRate * Time.deltaTime;
        }
        else if (surfaceDetector.IsInFloor)
        {
            targetFOV -= fovDecreaseRate * Time.deltaTime;
        }
        
        // Apply inertia-based FOV effect
        float inertia = InertiaController.GetInertia();
        if (inertia > 1.0f) // Only when inertia is above normal
        {
            float inertiaMax = inertiaController.GetInertiaFOV(); // Get from inertia controller if needed
            float inertiaMultiplier = (inertia - 1.0f) / (1.5f - 1.0f); // Normalize to 0-1 (assuming max 1.5)
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
                                      if (mainCamera != null)
                                      {
                                          mainCamera.fieldOfView = x;
                                      }
                                      if (gunCameraComponent != null)
                                      {
                                          gunCameraComponent.fieldOfView = x;
                                      }
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
        if (camera == null || surfaceDetector == null) return;
        
        float targetTilt = 0f;
        
        if (surfaceDetector.IsInWallLeft)
        {
            targetTilt = -maxCameraTilt;
        }
        else if (surfaceDetector.IsInWallRight)
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
    
    /// <summary>
    /// Changes mouse sensibility
    /// </summary>
    public void ChangeMouseSensibility(float value)
    {
        mouseSensibility = value;
    }
    
    /// <summary>
    /// Resets camera rotation
    /// </summary>
    public void ResetCamera()
    {
        resetCamera = true;
    }
    
    /// <summary>
    /// Cleanup on destroy
    /// </summary>
    private void OnDestroy()
    {
        fovTween?.Kill();
        cameraTiltTween?.Kill();
    }
}

