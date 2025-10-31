using UnityEngine;

/// <summary>
/// Sistema de inercia del jugador. Aumenta la inercia cuando está en el aire,
/// disminuye cuando está en el suelo. Gestiona el estado de alta inercia y
/// detecta cargadores de inercia especiales. Integra con SurfaceDetector y
/// notifica cambios de inercia al PlayerController.
/// </summary>
public class InertiaController : MonoBehaviour
{
    [Header("Inertia Settings")]
    [SerializeField] private float inertiaIncreaseRate = 0.002f;
    [SerializeField] private float inertiaDecreaseRate = 0.025f;
    [SerializeField] private float highInertiaThreshold = 1.3f;
    [SerializeField] private float inertiaLerpSpeed = 3f;
    [SerializeField] private float inertiaMin = 1f;
    [SerializeField] private float inertiaMax = 1.5f;
    
    [Header("Inertia Charger")]
    [SerializeField] private LayerMask inertiaChargerLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;
    
    private static float inertia = 1f;
    private float inertiaFOV = 50f;
    
    private SurfaceDetector surfaceDetector;
    private GameObject footPoint;
    private PlayerController playerController;
    
    private bool isInInertiaCharger;
    
    /// <summary>
    /// Initialize inertia controller with required dependencies
    /// </summary>
    public void Setup(SurfaceDetector detector, GameObject footPointRef, PlayerController controller,
                     float increaseRate, float decreaseRate, float threshold, float lerpSpeed,
                     float min, float max, LayerMask chargerLayer, float checkRadius)
    {
        surfaceDetector = detector;
        footPoint = footPointRef;
        playerController = controller;
        inertiaIncreaseRate = increaseRate;
        inertiaDecreaseRate = decreaseRate;
        highInertiaThreshold = threshold;
        inertiaLerpSpeed = lerpSpeed;
        inertiaMin = min;
        inertiaMax = max;
        inertiaChargerLayer = chargerLayer;
        groundCheckRadius = checkRadius;
    }
    
    /// <summary>
    /// Updates the inertia system based on player state. Increases inertia when airborne,
    /// decreases when grounded. Manages high inertia state and notifies other systems.
    /// Now with more gradual and controlled inertia changes.
    /// </summary>
    public void UpdateInertia()
    {
        if (surfaceDetector == null || playerController == null) return;
        
        float targetInertia = inertia;
        
        // Si no estoy en el piso aumento la inercia
        if (!surfaceDetector.IsInFloor)
        {
            targetInertia += inertiaIncreaseRate;
        }
        else
        {
            // Si estoy en el piso disminuyo la inercia
            targetInertia -= inertiaDecreaseRate;
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
            playerController.PlayerHaveHighInertia();
        }
        else if (!isHighInertia && wasHighInertia)
        {
            playerController.PlayerDoesntHaveHighInertia();
        }
        
        playerController.PlayerInertiaAltered(inertia);
    }
    
    /// <summary>
    /// Checks for inertia charger zones and applies their effects
    /// </summary>
    public void CheckInertiaCharger()
    {
        if (footPoint == null) return;
        
        isInInertiaCharger = Physics.CheckSphere(footPoint.transform.position, groundCheckRadius, inertiaChargerLayer);
        
        if (isInInertiaCharger)
        {
            inertia = 1.5f;
            inertiaFOV += 2.5f;
        }
    }
    
    /// <summary>
    /// Gets the current inertia value
    /// </summary>
    public static float GetInertia() => inertia;
    
    /// <summary>
    /// Sets the inertia value directly
    /// </summary>
    public void SetInertia(float value)
    {
        inertia = Mathf.Clamp(value, inertiaMin, inertiaMax);
    }
    
    /// <summary>
    /// Gets if player is in an inertia charger
    /// </summary>
    public bool IsInInertiaCharger() => isInInertiaCharger;
    
    /// <summary>
    /// Gets the inertia FOV value
    /// </summary>
    public float GetInertiaFOV() => inertiaFOV;
    
    /// <summary>
    /// Sets the inertia FOV value
    /// </summary>
    public void SetInertiaFOV(float value)
    {
        inertiaFOV = value;
    }
}

