using UnityEngine;

/// <summary>
/// Sistema de movimiento horizontal del jugador. Maneja el movimiento automático
/// hacia adelante y el modo de control manual WASD (debug). Integra con
/// InertiaController para aplicar multiplicadores de inercia al movimiento.
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useWASDControl = false; // Activa controles WASD en lugar de movimiento automático
    
    private CharacterController characterController;
    private InertiaController inertiaController;
    private GameObject camera;
    private Vector3 move;
    
    /// <summary>
    /// Initialize movement controller with required dependencies
    /// </summary>
    public void Setup(CharacterController controller, InertiaController inertiaCtrl, 
                     GameObject cameraRef, float speed, bool wasdControl)
    {
        characterController = controller;
        inertiaController = inertiaCtrl;
        camera = cameraRef;
        moveSpeed = speed;
        useWASDControl = wasdControl;
    }
    
    /// <summary>
    /// Handles player movement input and applies movement to the character controller.
    /// Player always moves forward automatically in the direction they are looking.
    /// No manual movement controls - only camera direction controls movement.
    /// Forward movement is multiplied by inertia for enhanced speed when airborne.
    /// Debug mode: Can toggle between automatic forward movement and WASD controls.
    /// </summary>
    public void HandleMovement()
    {
        if (characterController == null) return;
        
        Vector3 inputDirection = Vector3.zero;
        
        if (useWASDControl)
        {
            // Manual WASD controls
            float moveX = Input.GetAxis("Horizontal"); // A/D
            float moveZ = Input.GetAxis("Vertical");   // W/S
            
            if (camera != null)
            {
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
        }
        else
        {
            // Automatic forward movement (original behavior)
            inputDirection = transform.forward;
        }
        
        // Movimiento multiplicado por inercia
        float inertia = InertiaController.GetInertia();
        move = inputDirection * inertia;
        
        // Aplico el vector move en el character controller
        characterController.Move(moveSpeed * Time.deltaTime * move);
    }
    
    /// <summary>
    /// Gets the current movement vector
    /// </summary>
    public Vector3 GetMoveVector() => move;
    
    /// <summary>
    /// Sets the movement speed
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    /// <summary>
    /// Toggles WASD control mode
    /// </summary>
    public void SetWASDControl(bool enabled)
    {
        useWASDControl = enabled;
    }
}

