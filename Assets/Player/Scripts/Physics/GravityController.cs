using UnityEngine;

/// <summary>
/// Sistema de gravedad para el jugador. Maneja la fuerza de gravedad artificial
/// y su aplicación al CharacterController. Integra con SurfaceDetector para
/// aplicar gravedad reducida cuando el jugador está en paredes o pisos.
/// </summary>
public class GravityController : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float wallSlideGravity = -2f;
    
    private CharacterController characterController;
    private SurfaceDetector surfaceDetector;
    
    // Gravity state
    private Vector3 gravityVector;
    private bool toggleGravity = true;
    
    /// <summary>
    /// Initialize gravity controller with required dependencies
    /// </summary>
    public void Setup(CharacterController controller, SurfaceDetector detector, float gravityValue, float wallSlideValue)
    {
        characterController = controller;
        surfaceDetector = detector;
        gravity = gravityValue;
        wallSlideGravity = wallSlideValue;
    }
    
    /// <summary>
    /// Fuerza de gravedad artificial, es sumada con el tiempo
    /// </summary>
    public void GravityForce()
    {
        if (characterController == null) return;
        
        gravityVector.y += gravity;
        characterController.Move(gravityVector * Time.deltaTime);
    }
    
    /// <summary>
    /// Activa la gravedad cuando el jugador está en piso o pared
    /// </summary>
    public void ActiveGravity()
    {
        if (surfaceDetector == null) return;
        
        // Solo reactivar gravedad si no está desactivada manualmente
        // (evitar que se reactive durante acciones especiales como trepar)
        if (!toggleGravity)
        {
            return; // Si está desactivada manualmente, no reactivarla automáticamente
        }
        
        if (surfaceDetector.IsInFloor || surfaceDetector.IsInWall)
        {
            toggleGravity = true;
        }
    }
    
    /// <summary>
    /// Aplica gravedad reducida cuando se está deslizando por una pared
    /// </summary>
    public void ApplyWallSlideGravity()
    {
        if (surfaceDetector != null && surfaceDetector.IsInWall && gravityVector.y < 0)
        {
            gravityVector.y = wallSlideGravity;
        }
    }
    
    /// <summary>
    /// Establece la gravedad directamente (útil para saltos y movimientos especiales)
    /// </summary>
    public void SetGravityVector(Vector3 newGravity)
    {
        gravityVector = newGravity;
    }
    
    /// <summary>
    /// Obtiene el vector de gravedad actual
    /// </summary>
    public Vector3 GetGravityVector() => gravityVector;
    
    /// <summary>
    /// Obtiene si la gravedad está activada
    /// </summary>
    public bool IsGravityEnabled() => toggleGravity;
    
    /// <summary>
    /// Establece si la gravedad está activada
    /// </summary>
    public void SetGravityEnabled(bool enabled)
    {
        toggleGravity = enabled;
    }
    
    /// <summary>
    /// Actualiza el valor de gravedad
    /// </summary>
    public void SetGravity(float newGravity)
    {
        gravity = newGravity;
    }
    
    /// <summary>
    /// Resetea el vector de gravedad al valor de wall slide gravity
    /// </summary>
    public void ResetGravityVector()
    {
        gravityVector.y = wallSlideGravity;
    }
}

