using UnityEngine;

/// <summary>
/// Sistema de salto del jugador. Maneja saltos normales desde piso/pendiente,
/// saltos desde paredes, y doble salto. Integra con SurfaceDetector para
/// determinar superficies válidas y con GravityController para aplicar
/// la fuerza de salto correcta.
/// </summary>
public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpValue = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    private CharacterController characterController;
    private SurfaceDetector surfaceDetector;
    private GravityController gravityController;
    private InertiaController inertiaController;
    private PogoController pogoController;
    private PlayerController playerController;
    
    private bool doubleJump = true;
    
    /// <summary>
    /// Initialize jump controller with required dependencies
    /// </summary>
    public void Setup(CharacterController controller, SurfaceDetector detector, 
                     GravityController gravityCtrl, InertiaController inertiaCtrl,
                     PogoController pogoCtrl, PlayerController playerCtrl,
                     float jump, float grav)
    {
        characterController = controller;
        surfaceDetector = detector;
        gravityController = gravityCtrl;
        inertiaController = inertiaCtrl;
        pogoController = pogoCtrl;
        playerController = playerCtrl;
        jumpValue = jump;
        gravity = grav;
    }
    
    /// <summary>
    /// Handles all jump-related input and physics including ground jumping, wall jumping, and double jumping.
    /// Now uses intelligent surface detection based on surface angles instead of layer masks.
    /// </summary>
    public void HandleJump()
    {
        if (surfaceDetector == null || gravityController == null || characterController == null) return;
        
        // Detectar superficie usando el nuevo sistema inteligente
        surfaceDetector.DetectSurface();
        
        // Determinar si estamos en una superficie válida para saltar
        bool canJumpFromSurface = surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Floor || 
                                 surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Slope ||
                                 surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Wall ||
                                 (inertiaController != null && inertiaController.IsInInertiaCharger());

        // Aplicar gravedad reducida cuando estamos en una superficie horizontal
        if ((surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Floor || 
             surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Slope) && 
            gravityController.GetGravityVector().y < 0)
        {
            gravityController.SetGravityVector(new Vector3(0, -2f, 0));
            
            // Reset momentum system when touching ground
            if (pogoController != null && pogoController.GetConsecutivePogos() > 0)
            {
                pogoController.ResetConsecutivePogos();
                Debug.Log("Momentum reset - touched ground");
            }
        }

        // Salto desde piso o pendiente
        if (Input.GetButtonDown("Jump") && 
            (surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Floor || 
             surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Slope))
        {
            ExecuteJump();
        }
        
        // Salto desde pared o cargador de inercia
        if (Input.GetButtonDown("Jump") && 
            (surfaceDetector.GetCurrentSurfaceType() == SurfaceType.Wall || 
             (inertiaController != null && inertiaController.IsInInertiaCharger())))
        {
            ExecuteJump();
        }

        // Doble salto
        if (Input.GetButtonDown("Jump") && !canJumpFromSurface && doubleJump == true)
        {
            if (playerController != null)
            {
                playerController.PlayerIsDoubleJumping();
            }
            ExecuteJump();
            doubleJump = false;
        }
        
        // Resetear doble salto cuando tocamos una superficie válida
        if (canJumpFromSurface && doubleJump == false)
        {
            doubleJump = true;
        }
    }
    
    /// <summary>
    /// Ejecuta un salto aplicando la fuerza de salto al vector de gravedad
    /// </summary>
    private void ExecuteJump()
    {
        if (playerController != null)
        {
            playerController.PlayerIsJumping();
        }
        
        Vector3 gravityVec = gravityController.GetGravityVector();
        gravityVec.y = Mathf.Sqrt(jumpValue * -2 * gravity);
        gravityController.SetGravityVector(gravityVec);
        
        characterController.Move(gravityVec * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// Resetea el doble salto (útil para resetear desde otros sistemas como pogo)
    /// </summary>
    public void ResetDoubleJump()
    {
        doubleJump = true;
    }
    
    /// <summary>
    /// Obtiene si el doble salto está disponible
    /// </summary>
    public bool IsDoubleJumpAvailable() => doubleJump;
}

