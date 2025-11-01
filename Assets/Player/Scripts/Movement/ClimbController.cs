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
        
        Debug.Log($"[ClimbController] Setup completado - Layers: {climbableLayers}, Forward: {climbCheckForward}, Height: {climbUpHeight}");
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
                Debug.Log("[ClimbController] DetectClimbableEdge: Cooldown expirado, reseteando canClimb");
                canClimb = false;
            }
        }
        
        if (characterController == null || footPoint == null || surfaceDetector == null)
        {
            if (canClimb) Debug.Log("[ClimbController] DetectClimbableEdge: Componentes null");
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
                Debug.Log("[ClimbController] DetectClimbableEdge: En suelo, cancelando canClimb activo");
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
            if (canClimb) Debug.Log($"[ClimbController] DetectClimbableEdge: En suelo/pendiente ({currentSurface}), gravity.y: {gravityVec.y}");
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
            if (canClimb) Debug.Log($"[ClimbController] DetectClimbableEdge: Movimiento vertical muy rápido ({gravityVec.y}), puede no estar cerca del borde");
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
        
        // Log informativo si el ratio es muy alto o muy bajo (solo para ayudar a entender)
        if (climbDetectionHeightRatio > 2f || climbDetectionHeightRatio < 0.1f)
        {
            if (Time.frameCount % 60 == 0) // Solo cada segundo aprox
            {
                Debug.Log($"[ClimbController] INFO: climbDetectionHeightRatio={climbDetectionHeightRatio} → detectando en altura={detectionHeight}m (jugador={playerHeight}m). Valores típicos: 0.5-1.0");
            }
        }
        float startHeight = -playerHeight * 0.4f + centerOffset; // Desde la parte baja del jugador
        float endHeight = playerHeight * 0.4f + centerOffset; // Hasta la parte alta
        
        // Usar el valor tal cual (sin limitar)
        float safeForward = climbCheckForward;
        
        // Log informativo si la distancia es muy grande
        if (climbCheckForward > 3f)
        {
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[ClimbController] INFO: climbCheckForward={climbCheckForward}m es muy grande. Puede detectar paredes lejanas que no son el borde que quieres trepar. Valores típicos: 0.3-1.0m");
            }
        }
        
        // Probar desde diferentes alturas a lo largo del cuerpo del jugador
        int numChecks = Mathf.Max(3, Mathf.RoundToInt(detectionHeight / 0.2f)); // Al menos 3 checks, uno cada ~0.2m
        
        if (Time.frameCount % 60 == 0 && numChecks > 20)
        {
            Debug.Log($"[ClimbController] INFO: Realizando {numChecks} raycasts (puede ser costoso). Altura detección: {detectionHeight}m, Ratio: {climbDetectionHeightRatio}");
        }
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
                Debug.Log($"[ClimbController] DetectClimbableEdge: Pared detectada a altura relativa {heightOffset:F2}m (desde base), distancia: {wallHit.distance}, normal: {wallHit.normal}");
                break;
            }
        }
        
        if (!wallDetected)
        {
            if (canClimb) Debug.Log("[ClimbController] DetectClimbableEdge: No se detectó pared");
            canClimb = false;
            return;
        }
        
        // Verificar que la pared sea vertical (ángulo cercano a 90 grados)
        // Hacer más permisivo: entre 65 y 115 grados
        float wallAngle = Vector3.Angle(Vector3.up, wallHit.normal);
        if (wallAngle < 65f || wallAngle > 115f)
        {
            if (canClimb && canClimbCooldown <= 0f) Debug.Log($"[ClimbController] DetectClimbableEdge: Ángulo de pared no válido ({wallAngle}°)");
            if (canClimbCooldown <= 0f)
            {
                canClimb = false;
            }
            return;
        }
        
        Debug.Log($"[ClimbController] DetectClimbableEdge: Pared válida detectada - Ángulo: {wallAngle}°, Distancia: {wallHit.distance}m");
        
        // Verificar que haya espacio arriba para trepar
        Vector3 checkPosition = wallHit.point + wallHit.normal * 0.1f + Vector3.up * 0.2f;
        
        // Raycast hacia arriba para ver si hay espacio
        RaycastHit ceilingHit;
        bool hasCeiling = Physics.Raycast(checkPosition, Vector3.up, out ceilingHit, 
                                         climbUpHeight, climbableLayers);
        
        Debug.DrawRay(checkPosition, Vector3.up * climbUpHeight, hasCeiling ? Color.red : Color.green, 0.1f);
        
        if (hasCeiling)
        {
            if (canClimb) Debug.Log($"[ClimbController] DetectClimbableEdge: Techo detectado a {ceilingHit.distance}m");
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
            if (canClimb) Debug.Log("[ClimbController] DetectClimbableEdge: No se encontró piso de plataforma arriba");
            canClimb = false;
            return;
        }
        
        // Verificar que la superficie superior sea plana (piso)
        float floorAngle = Vector3.Angle(Vector3.up, floorHit.normal);
        if (floorAngle > 30f)
        {
            if (canClimb) Debug.Log($"[ClimbController] DetectClimbableEdge: Superficie superior muy inclinada ({floorAngle}°)");
            canClimb = false;
            return;
        }
        
        Debug.Log($"[ClimbController] DetectClimbableEdge: Superficie superior válida - Ángulo: {floorAngle}°");
        
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
                if (canClimb && canClimbCooldown <= 0f) Debug.Log($"[ClimbController] DetectClimbableEdge: Distancia vertical hacia arriba fuera de rango ({verticalDistanceAbs}m, max: {maxClimbUp}m)");
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
                if (canClimb && canClimbCooldown <= 0f) Debug.Log($"[ClimbController] DetectClimbableEdge: Distancia vertical hacia abajo fuera de rango ({verticalDistanceAbs}m, max: {maxClimbDown}m - cayendo desde muy arriba)");
                if (canClimbCooldown <= 0f)
                {
                    canClimb = false;
                }
                return;
            }
        }
        
        Debug.Log($"[ClimbController] DetectClimbableEdge: Distancia vertical válida - {verticalDistanceAbs:F2}m ({(!isBelowPlatform ? "arriba/cayendo" : "abajo/subiendo")})");
        
        // Verificar que el jugador esté lo suficientemente cerca horizontalmente
        float horizontalDistance = Vector3.Distance(
            new Vector3(climbTargetPosition.x, transform.position.y, climbTargetPosition.z),
            transform.position);
        float maxHorizontalDist = climbCheckForward * 1.5f; // Margen razonable (1.5x la distancia de detección)
        
        // Log informativo si la distancia horizontal máxima es muy grande
        if (maxHorizontalDist > 5f)
        {
            if (canClimb && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[ClimbController] INFO: maxHorizontalDist={maxHorizontalDist}m (climbCheckForward={climbCheckForward}m * 1.5). Con valores grandes puede detectar bordes muy lejanos.");
            }
        }
        if (horizontalDistance > maxHorizontalDist)
        {
            if (canClimb && canClimbCooldown <= 0f) Debug.Log($"[ClimbController] DetectClimbableEdge: Distancia horizontal muy lejana ({horizontalDistance}m, max: {maxHorizontalDist}m)");
            if (canClimbCooldown <= 0f)
            {
                canClimb = false;
            }
            return;
        }
        
        bool wasClimbable = canClimb;
        canClimb = true;
        canClimbCooldown = CLIMB_COOLDOWN_TIME; // Resetear cooldown cuando detectamos borde válido
        
        if (!wasClimbable)
        {
            Debug.Log($"[ClimbController] DetectClimbableEdge: ✓ BORDE TREPABLE DETECTADO! Pos: {transform.position}, Target: {climbTargetPosition}, Dist V: {verticalDistance}m, Dist H: {horizontalDistance}m, Cooldown: {canClimbCooldown}s");
        }
        
        // Si el trepar automático está activado, intentar trepar automáticamente cuando se detecta un borde válido
        // Funciona EXACTAMENTE igual que el trepar manual: si canClimb es true, trepa
        if (autoClimb && !isClimbing && canClimb)
        {
            // Simplemente intentar trepar - sin condiciones adicionales
            // Si canClimb es true, significa que ya pasó todas las validaciones
            // (distancia, altura, ángulos, etc.), igual que el trepar manual
            Debug.Log($"[ClimbController] DetectClimbableEdge: Auto-trepar activado! canClimb={canClimb}");
            TryStartClimb(); // Intentar iniciar trepar automáticamente (mismas condiciones que manual)
        }
    }
    
    /// <summary>
    /// Intenta iniciar el trepar si hay un borde detectado
    /// </summary>
    public bool TryStartClimb()
    {
        Debug.Log($"[ClimbController] TryStartClimb llamado - canClimb: {canClimb}, isClimbing: {isClimbing}");
        
        if (!canClimb || isClimbing)
        {
            Debug.Log($"[ClimbController] TryStartClimb: No se puede iniciar - canClimb: {canClimb}, isClimbing: {isClimbing}");
            return false;
        }
        
        // Iniciar trepar
        isClimbing = true;
        climbProgress = 0f;
        
        // Guardar distancia inicial para cálculo de progreso
        initialClimbDistance = Vector3.Distance(transform.position, climbTargetPosition);
        
        Debug.Log($"<color=green><size=16>✓ TREPAR INICIADO!</size></color> Distancia inicial: {initialClimbDistance}m | Target: {climbTargetPosition}", this);
        
        // Detener la gravedad temporalmente y resetear completamente
        if (gravityController != null)
        {
            gravityController.SetGravityEnabled(false);
            // Resetear completamente el vector de gravedad
            gravityController.SetGravityVector(Vector3.zero);
            Debug.Log("[ClimbController] TryStartClimb: Gravedad desactivada y vector reseteado");
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
            if (isClimbing && characterController == null)
            {
                Debug.LogWarning("[ClimbController] UpdateClimb: CharacterController es null durante trepar!");
            }
            return;
        }
        
        // Calcular progreso del trepar
        Vector3 currentDirection = (climbTargetPosition - transform.position);
        float distanceToTarget = currentDirection.magnitude;
        
        // Log cada cierto tiempo para verificar progreso (más visible)
        if (Time.frameCount % 10 == 0)
        {
            Debug.Log($"<color=cyan>[TREPANDO]</color> Distancia: {distanceToTarget:F2}m | Progreso: {climbProgress:P0} | Pos: {transform.position} | Target: {climbTargetPosition}", this);
        }
        
        // Si llegamos al objetivo, terminar el trepar
        if (distanceToTarget < 0.2f)
        {
            Debug.Log($"<color=green><size=16>✓ OBJETIVO ALCANZADO!</size></color> Distancia final: {distanceToTarget}m", this);
            FinishClimb();
            return;
        }
        
        // Si nos alejamos demasiado, cancelar el trepar
        if (distanceToTarget > initialClimbDistance * 1.5f && initialClimbDistance > 0.1f)
        {
            Debug.LogWarning($"[ClimbController] UpdateClimb: Nos alejamos demasiado! Distancia: {distanceToTarget}m, Inicial: {initialClimbDistance}m");
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
            Debug.LogWarning("[ClimbController] UpdateClimb: Ya no hay pared delante durante trepar avanzado");
            // No cancelar, continuar hacia el objetivo
        }
        
        // Asegurar que la gravedad esté desactivada durante el trepar
        if (gravityController != null && gravityController.IsGravityEnabled())
        {
            Debug.LogWarning("[ClimbController] UpdateClimb: Gravedad estaba activada, desactivando...");
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
        
        // Log de movimiento aplicado (solo cuando hay cambios significativos o problemas)
        if (Time.frameCount % 15 == 0 || moveVector.magnitude < 0.001f) // Menos frecuente pero más visible
        {
            string flagInfo = "";
            if ((flags & CollisionFlags.Sides) != 0) flagInfo += " [Colisión lateral]";
            if ((flags & CollisionFlags.Above) != 0) flagInfo += " [Colisión arriba]";
            if ((flags & CollisionFlags.Below) != 0) flagInfo += " [Colisión abajo]";
            
            if (moveVector.magnitude < 0.001f)
            {
                Debug.LogWarning($"<color=yellow>[TREPANDO] Movimiento muy pequeño: {moveVector.magnitude:F5}m{flagInfo}</color>", this);
            }
            else
            {
                Debug.Log($"<color=cyan>[TREPANDO] Movimiento: {moveVector.magnitude:F3}m/frame{flagInfo}</color>", this);
            }
        }
        
        // Verificar si el movimiento fue bloqueado
        if ((flags & CollisionFlags.Sides) != 0 && climbProgress < 0.3f)
        {
            Debug.LogWarning($"[ClimbController] UpdateClimb: Colisión lateral detectada durante inicio del trepar. Flags: {flags}");
        }
        
        // Actualizar progreso basado en la distancia inicial
        if (initialClimbDistance > 0.01f)
        {
            climbProgress = Mathf.Clamp01(1f - (distanceToTarget / initialClimbDistance));
        }
        else
        {
            Debug.LogWarning("[ClimbController] UpdateClimb: initialClimbDistance es muy pequeño o cero!");
        }
    }
    
    /// <summary>
    /// Termina el trepar y restaura la gravedad
    /// </summary>
    private void FinishClimb()
    {
        Debug.Log("[ClimbController] FinishClimb: Finalizando trepar y restaurando gravedad");
        isClimbing = false;
        climbProgress = 0f;
        initialClimbDistance = 0f;
        
        // Restaurar gravedad gradualmente
        if (gravityController != null)
        {
            gravityController.SetGravityEnabled(true);
            // No resetear el vector inmediatamente, dejar que ActiveGravity lo haga
            Debug.Log("[ClimbController] FinishClimb: Gravedad reactivada");
        }
    }
    
    /// <summary>
    /// Fuerza a detener el trepar (útil si el jugador es interrumpido)
    /// </summary>
    public void CancelClimb()
    {
        if (isClimbing)
        {
            Debug.Log("[ClimbController] CancelClimb: Trepar cancelado");
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
        if (result && Time.frameCount % 30 == 0) // Log ocasional cuando está disponible
        {
            Debug.Log($"[ClimbController] CanClimb: {result} (canClimb: {canClimb}, cooldown: {canClimbCooldown:F2}s, isClimbing: {isClimbing})");
        }
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

