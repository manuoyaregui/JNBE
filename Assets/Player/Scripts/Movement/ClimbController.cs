using UnityEngine;

/// <summary>
/// Sistema de trepar (climbing) para el jugador. Detecta cuando el jugador está
/// cerca del borde de una plataforma y permite trepar para alcanzarla.
/// Se integra con JumpController para activar el trepar cuando se presiona el botón de salto.
/// </summary>
public class ClimbController : MonoBehaviour
{
    [Header("Climb Detection Settings")]
    [SerializeField] private float climbDetectionDistance = 4f; // Distancia para detectar el borde de la plataforma (NO USAR - deprecated)
    [SerializeField] private float climbDetectionHeightRatio = 6f; // Ratio de la altura del jugador para detectar (0.8 = 80% de la altura del jugador)
    [SerializeField] private float climbCheckForward = 4f; // Distancia hacia adelante para detectar paredes (m)
    [SerializeField] private float climbUpHeight = 4f; // Altura máxima que el jugador puede trepar hacia arriba (m)
    [SerializeField] private float climbSpeed = 5f; // Velocidad de trepar (m/s)
    [SerializeField] private bool autoClimb = true; // Si es true, trepa automáticamente al detectar borde. Si es false, requiere presionar Jump
    [SerializeField] private float autoClimbDistance = 4.5f; // (NO SE USA - el auto-trepar usa las mismas condiciones que el manual. Puedes eliminarlo si quieres)
    
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask climbableLayers = -1; // Capas que se pueden trepar
    
    private CharacterController characterController;
    private SurfaceDetector surfaceDetector;
    private GravityController gravityController;
    private PlayerController playerController;
    private GameObject footPoint;
    
    // Climb state
    private bool isClimbing = false;
    private bool canClimb = false;
    private Vector3 climbTargetPosition;
    private Vector3 climbDirection;
    private float climbProgress = 0f;
    private float initialClimbDistance = 0f;
    private float canClimbCooldown = 0f; // Tiempo que mantiene canClimb en true aunque las condiciones cambien ligeramente
    private const float CLIMB_COOLDOWN_TIME = 0.3f; // Segundos de cooldown
    
    /// <summary>
    /// Initialize climb controller with required dependencies
    /// </summary>
    public void Setup(CharacterController controller, SurfaceDetector detector, 
                     GravityController gravityCtrl, PlayerController playerCtrl,
                     GameObject footPointRef, LayerMask layers)
    {
        characterController = controller;
        surfaceDetector = detector;
        gravityController = gravityCtrl;
        playerController = playerCtrl;
        footPoint = footPointRef;
        climbableLayers = layers;
    }
    
    /// <summary>
    /// Detecta si hay un borde de plataforma cerca que se pueda trepar
    /// Si autoClimb está activado, intenta trepar automáticamente cuando está muy cerca
    /// </summary>
    public void DetectClimbableEdge()
    {
        // Reducir cooldown cada frame
        if (canClimbCooldown > 0f)
        {
            canClimbCooldown -= Time.deltaTime;
            if (canClimbCooldown <= 0f && canClimb)
            {
                canClimb = false;
            }
        }
        
        if (characterController == null || footPoint == null || surfaceDetector == null)
        {
            canClimb = false;
            canClimbCooldown = 0f;
            return;
        }
        
        // Obtener el tipo de superficie actual una sola vez
        SurfaceType currentSurface = surfaceDetector.GetCurrentSurfaceType();
        
        // Si ya tenemos canClimb activo y aún hay cooldown, mantenerlo (a menos que estemos en el suelo)
        if (canClimb && canClimbCooldown > 0f)
        {
            if (currentSurface == SurfaceType.Floor || currentSurface == SurfaceType.Slope)
            {
                canClimb = false;
                canClimbCooldown = 0f;
            }
            else
            {
                // Mantener canClimb activo durante el cooldown
                return;
            }
        }
        
        // Solo detectar si estamos en el aire (no en el suelo ni en una pendiente)
        // Permitir detección si estamos cayendo cerca del borde de una plataforma
        // Permitir trepar incluso si estamos en el aire cayendo hacia un borde
        // Solo bloquear si estamos claramente en el suelo caminando
        bool isOnGround = currentSurface == SurfaceType.Floor || currentSurface == SurfaceType.Slope;
        
        // Obtener velocidad vertical del jugador
        // Permite trepar tanto subiendo como cayendo, siempre que esté en el aire
        Vector3 gravityVec = gravityController != null ? gravityController.GetGravityVector() : Vector3.zero;
        
        if (isOnGround)
        {
            // Si está claramente en el suelo (no en el aire), no puede trepar
            canClimb = false;
            canClimbCooldown = 0f;
            return;
        }
        
        // Permitir trepar tanto cuando está subiendo (hacia una plataforma alta desde abajo)
        // como cuando está cayendo (desde arriba hacia una plataforma)
        // Solo bloquear si está subiendo o cayendo extremadamente rápido (probablemente no cerca de un borde)
        bool isMovingVeryFast = Mathf.Abs(gravityVec.y) > 15f;
        
        if (isMovingVeryFast)
        {
            // Si se mueve extremadamente rápido (saltos muy potentes o caídas muy rápidas), probablemente no está cerca de un borde
            // No bloquear completamente, solo advertir - dejar que la detección de pared decida
        }
        
        // Raycast hacia adelante desde la posición del jugador para detectar pared vertical
        // Usar múltiples puntos de altura a lo largo del cuerpo del jugador para mejor detección
        // Esto funciona sin importar qué tan alto esté el jugador en el aire
        Vector3 basePosition = transform.position;
        bool wallDetected = false;
        RaycastHit wallHit = new RaycastHit();
        
        // Calcular la altura de detección basada en la altura del CharacterController
        // Esto asegura que funcione sin importar la altura del jugador
        float playerHeight = characterController != null ? characterController.height : 2f;
        float centerOffset = characterController != null ? characterController.center.y : 0f;
        
        // Usar el valor tal cual (sin limitar) - dejar que el usuario experimente
        float detectionHeight = playerHeight * climbDetectionHeightRatio;
        
        float startHeight = -playerHeight * 0.4f + centerOffset; // Desde la parte baja del jugador
        float endHeight = playerHeight * 0.4f + centerOffset; // Hasta la parte alta
        
        // Usar el valor tal cual (sin limitar)
        float safeForward = climbCheckForward;
        
        
        // Probar desde diferentes alturas a lo largo del cuerpo del jugador
        int numChecks = Mathf.Max(3, Mathf.RoundToInt(detectionHeight / 0.2f)); // Al menos 3 checks, uno cada ~0.2m
        for (int i = 0; i <= numChecks; i++)
        {
            float heightOffset = Mathf.Lerp(startHeight, endHeight, (float)i / numChecks);
            Vector3 rayOrigin = basePosition + Vector3.up * heightOffset;
            Vector3 rayDirection = transform.forward;
            
            // Dibujar rayos solo ocasionalmente para no saturar
            if (Time.frameCount % 10 == 0)
            {
                Debug.DrawRay(rayOrigin, rayDirection * safeForward, Color.red, 0.2f);
            }
            
            if (Physics.Raycast(rayOrigin, rayDirection, out wallHit, 
                               safeForward, climbableLayers))
            {
                wallDetected = true;
                break;
            }
        }
        
        if (!wallDetected)
        {
            canClimb = false;
            return;
        }
        
        // Verificar que la pared sea vertical (ángulo cercano a 90 grados)
        // Hacer más permisivo: entre 65 y 115 grados
        float wallAngle = Vector3.Angle(Vector3.up, wallHit.normal);
        if (wallAngle < 65f || wallAngle > 115f)
        {
            if (canClimbCooldown <= 0f)
            {
                canClimb = false;
            }
            return;
        }
        
        // Verificar que haya espacio arriba para trepar
        Vector3 checkPosition = wallHit.point + wallHit.normal * 0.1f + Vector3.up * 0.2f;
        
        // Raycast hacia arriba para ver si hay espacio
        RaycastHit ceilingHit;
        bool hasCeiling = Physics.Raycast(checkPosition, Vector3.up, out ceilingHit, 
                                         climbUpHeight, climbableLayers);
        
        Debug.DrawRay(checkPosition, Vector3.up * climbUpHeight, hasCeiling ? Color.red : Color.green, 0.1f);
        
        if (hasCeiling)
        {
            canClimb = false;
            return;
        }
        
        // Verificar que haya una superficie plana arriba (piso de la plataforma)
        // Buscar el piso desde arriba del muro
        Vector3 topCheckPosition = wallHit.point + Vector3.up * climbUpHeight + 
                                   wallHit.normal * 0.3f;
        
        RaycastHit floorHit;
        bool hasPlatformTop = Physics.Raycast(topCheckPosition, Vector3.down, out floorHit, 
                                             climbUpHeight * 0.6f, climbableLayers);
        
        Debug.DrawRay(topCheckPosition, Vector3.down * (climbUpHeight * 0.6f), 
                     hasPlatformTop ? Color.green : Color.red, 0.1f);
        
        if (!hasPlatformTop)
        {
            canClimb = false;
            return;
        }
        
        // Verificar que la superficie superior sea plana (piso)
        float floorAngle = Vector3.Angle(Vector3.up, floorHit.normal);
        if (floorAngle > 30f)
        {
            canClimb = false;
            return;
        }
        
        // Calcular la posición objetivo (arriba de la plataforma)
        climbTargetPosition = floorHit.point + Vector3.up * (characterController.height * 0.5f + 0.1f);
        climbDirection = (climbTargetPosition - transform.position).normalized;
        
        // Verificar que el jugador esté lo suficientemente cerca en Y
        // Hacer más permisivo: permitir trepar si está a una distancia razonable
        // Permitir tanto si está abajo de la plataforma (subiendo) como arriba (cayendo)
        float verticalDistance = climbTargetPosition.y - transform.position.y;
        float verticalDistanceAbs = Mathf.Abs(verticalDistance);
        
        // Permitir trepar si:
        // 1. Está abajo de la plataforma y dentro del rango de trepar hacia arriba
        // 2. Está arriba de la plataforma pero muy cerca (cayendo hacia el borde)
        bool isBelowPlatform = verticalDistance > 0f; // Positivo significa que el target está arriba
        float maxClimbUp = climbUpHeight * 1.2f;
        float maxClimbDown = 0.5f; // Permitir trepar si está hasta 0.5m arriba del borde (cayendo)
        
        if (isBelowPlatform)
        {
            // Está abajo, debe estar dentro del rango para trepar hacia arriba
            if (verticalDistanceAbs > maxClimbUp || verticalDistanceAbs < 0.2f)
            {
                if (canClimbCooldown <= 0f)
                {
                    canClimb = false;
                }
                return;
            }
        }
        else
        {
            // Está arriba (cayendo), debe estar muy cerca del borde
            if (verticalDistanceAbs > maxClimbDown)
            {
                if (canClimbCooldown <= 0f)
                {
                    canClimb = false;
                }
                return;
            }
        }
        
        // Verificar que el jugador esté lo suficientemente cerca horizontalmente
        float horizontalDistance = Vector3.Distance(
            new Vector3(climbTargetPosition.x, transform.position.y, climbTargetPosition.z),
            transform.position);
        float maxHorizontalDist = climbCheckForward * 1.5f; // Margen razonable (1.5x la distancia de detección)
        
        if (horizontalDistance > maxHorizontalDist)
        {
            if (canClimbCooldown <= 0f)
            {
                canClimb = false;
            }
            return;
        }
        
        bool wasClimbable = canClimb;
        canClimb = true;
        canClimbCooldown = CLIMB_COOLDOWN_TIME; // Resetear cooldown cuando detectamos borde válido
        
        // Si el trepar automático está activado, intentar trepar automáticamente cuando se detecta un borde válido
        // Funciona EXACTAMENTE igual que el trepar manual: si canClimb es true, trepa
        if (autoClimb && !isClimbing && canClimb)
        {
            // Simplemente intentar trepar - sin condiciones adicionales
            // Si canClimb es true, significa que ya pasó todas las validaciones
            // (distancia, altura, ángulos, etc.), igual que el trepar manual
            TryStartClimb(); // Intentar iniciar trepar automáticamente (mismas condiciones que manual)
        }
    }
    
    /// <summary>
    /// Intenta iniciar el trepar si hay un borde detectado
    /// </summary>
    public bool TryStartClimb()
    {
        if (!canClimb || isClimbing)
        {
            return false;
        }
        
        // Iniciar trepar
        isClimbing = true;
        climbProgress = 0f;
        
        // Guardar distancia inicial para cálculo de progreso
        initialClimbDistance = Vector3.Distance(transform.position, climbTargetPosition);
        
        // Detener la gravedad temporalmente y resetear completamente
        if (gravityController != null)
        {
            gravityController.SetGravityEnabled(false);
            // Resetear completamente el vector de gravedad
            gravityController.SetGravityVector(Vector3.zero);
        }
        
        return true;
    }
    
    /// <summary>
    /// Actualiza el movimiento de trepar
    /// </summary>
    public void UpdateClimb()
    {
        if (!isClimbing || characterController == null)
        {
            return;
        }
        
        // Calcular progreso del trepar
        Vector3 currentDirection = (climbTargetPosition - transform.position);
        float distanceToTarget = currentDirection.magnitude;
        
        // Si llegamos al objetivo, terminar el trepar
        if (distanceToTarget < 0.2f)
        {
            FinishClimb();
            return;
        }
        
        // Si nos alejamos demasiado, cancelar el trepar
        if (distanceToTarget > initialClimbDistance * 1.5f && initialClimbDistance > 0.1f)
        {
            CancelClimb();
            return;
        }
        
        // Verificar si todavía hay una superficie válida para trepar
        // (por si algo cambió durante el trepar - hacer esto menos estricto durante el trepar)
        float safeForward = Mathf.Clamp(climbCheckForward, 0.2f, 2f);
        Vector3 checkPos = transform.position + transform.forward * 0.2f;
        RaycastHit wallCheck;
        bool hasWall = Physics.Raycast(checkPos, transform.forward, out wallCheck, safeForward * 1.5f, climbableLayers);
        
        if (!hasWall && climbProgress > 0.3f)
        {
            // Solo cancelar si estamos avanzados en el trepar y ya no hay pared
            // No cancelar, continuar hacia el objetivo
        }
        
        // Asegurar que la gravedad esté desactivada durante el trepar
        if (gravityController != null && gravityController.IsGravityEnabled())
        {
            gravityController.SetGravityEnabled(false);
            gravityController.SetGravityVector(Vector3.zero);
        }
        
        // Mover hacia el objetivo con movimiento suave
        Vector3 moveDirection = currentDirection.normalized;
        
        // Calcular velocidad de movimiento (m/s)
        float currentMoveSpeed = climbSpeed;
        
        // Aplicar movimiento con un componente más vertical al inicio
        Vector3 moveVector = moveDirection * currentMoveSpeed * Time.deltaTime;
        
        // Enfocar más el movimiento vertical al principio del trepar
        if (climbProgress < 0.6f)
        {
            // Asegurar que el movimiento vertical tenga más peso al inicio
            // Multiplicar solo el componente Y, no todo el vector
            moveVector.y = Mathf.Max(moveVector.y, moveDirection.y * currentMoveSpeed * Time.deltaTime * 1.5f);
        }
        
        // Limitar la velocidad para que no exceda la distancia restante
        if (moveVector.magnitude > distanceToTarget)
        {
            moveVector = moveDirection * distanceToTarget;
        }
        
        // Aplicar movimiento y verificar resultado
        CollisionFlags flags = characterController.Move(moveVector);
        
        // Verificar si el movimiento fue bloqueado
        
        // Actualizar progreso basado en la distancia inicial
        if (initialClimbDistance > 0.01f)
        {
            climbProgress = Mathf.Clamp01(1f - (distanceToTarget / initialClimbDistance));
        }
    }
    
    /// <summary>
    /// Termina el trepar y restaura la gravedad
    /// </summary>
    private void FinishClimb()
    {
        isClimbing = false;
        climbProgress = 0f;
        initialClimbDistance = 0f;
        
        // Restaurar gravedad gradualmente
        if (gravityController != null)
        {
            gravityController.SetGravityEnabled(true);
            // No resetear el vector inmediatamente, dejar que ActiveGravity lo haga
        }
    }
    
    /// <summary>
    /// Fuerza a detener el trepar (útil si el jugador es interrumpido)
    /// </summary>
    public void CancelClimb()
    {
        if (isClimbing)
        {
            isClimbing = false;
            climbProgress = 0f;
            initialClimbDistance = 0f;
            canClimb = false;
            canClimbCooldown = 0f;
            
            // Restaurar gravedad
            if (gravityController != null)
            {
                gravityController.SetGravityEnabled(true);
            }
        }
    }
    
    // ========== PUBLIC ACCESSORS ==========
    
    /// <summary>
    /// Indica si hay un borde trepable detectado
    /// </summary>
    public bool CanClimb() 
    {
        bool result = canClimb && !isClimbing;
        return result;
    }
    
    /// <summary>
    /// Indica si el jugador está actualmente trepando
    /// </summary>
    public bool IsClimbing() => isClimbing;
    
    /// <summary>
    /// Obtiene el progreso del trepar (0 a 1)
    /// </summary>
    public float GetClimbProgress() => climbProgress;
    
    /// <summary>
    /// Establece la distancia de detección de trepar
    /// </summary>
    public void SetClimbDetectionDistance(float distance)
    {
        climbDetectionDistance = distance;
    }
    
    /// <summary>
    /// Establece la velocidad de trepar
    /// </summary>
    public void SetClimbSpeed(float speed)
    {
        climbSpeed = speed;
    }
}

