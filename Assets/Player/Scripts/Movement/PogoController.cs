using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Sistema de pogo del jugador. Detecta elementos "poggable" cuando el jugador
/// está haciendo dash y ejecuta un rebote en ángulo fijo. Incluye sistema de
/// momentum para pogos consecutivos. Integra con DashController, JumpController
/// y ExternalForceController.
/// </summary>
public class PogoController : MonoBehaviour
{
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
    
    private Camera mainCamera;
    private CharacterController characterController;
    private DashController dashController;
    private JumpController jumpController;
    private ExternalForceController externalForceController;
    private SurfaceDetector surfaceDetector;
    private PlayerController playerController;
    private float dashSpeed;
    private float dashTime;
    
    private bool isPogoActive = false;
    private float lastPogoTime = 0f;
    private int consecutivePogos = 0;
    
    /// <summary>
    /// Converts degrees to dot product threshold for FOV detection.
    /// </summary>
    private float PogoDetectionAngleDotProduct => Mathf.Cos(pogoDetectionAngleDegrees * Mathf.Deg2Rad);
    
    /// <summary>
    /// Initialize pogo controller with required dependencies
    /// </summary>
    public void Setup(Camera camera, CharacterController controller, DashController dashCtrl,
                     JumpController jumpCtrl, ExternalForceController forceCtrl, SurfaceDetector detector,
                     PlayerController playerCtrl, float speed, float time)
    {
        mainCamera = camera;
        characterController = controller;
        dashController = dashCtrl;
        jumpController = jumpCtrl;
        externalForceController = forceCtrl;
        surfaceDetector = detector;
        playerController = playerCtrl;
        dashSpeed = speed;
        dashTime = time;
    }
    
    /// <summary>
    /// Checks for pogo opportunities when dashing towards poggable elements.
    /// Uses OverlapSphere to detect elements with "PoggableElement" tag within camera field of view.
    /// Now uses a wide field of view for easier pogo activation.
    /// </summary>
    public void CheckForPogo()
    {
        if (mainCamera == null || characterController == null || dashController == null) return;
        
        // Only check for pogo if we're actively dashing (dashTween is active)
        if (isPogoActive || !dashController.IsDashing()) return;
        
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
        if (isPogoActive || externalForceController == null || characterController == null) return;
        
        isPogoActive = true;
        
        // Kill any existing dash before applying pogo
        dashController.CancelDash();
        
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
        
        // Use existing ExecuteThrowPlayer method from ExternalForceController
        externalForceController.ExecuteThrow(bounceDirection, pogoForce, pogoDuration);
        
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
    private IEnumerator ResetPogoActive(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPogoActive = false;
    }
    
    /// <summary>
    /// Resets dash cooldown and double jump when pogo is successful.
    /// </summary>
    private void ResetAbilitiesOnPogo()
    {
        if (dashController != null)
        {
            dashController.ResetCooldown();
        }
        
        if (jumpController != null)
        {
            jumpController.ResetDoubleJump();
        }
        
        Debug.Log("Pogo successful! Dash and double jump reset.");
    }
    
    /// <summary>
    /// Resets consecutive pogos (called when touching ground)
    /// </summary>
    public void ResetConsecutivePogos()
    {
        consecutivePogos = 0;
    }
    
    /// <summary>
    /// Gets the current consecutive pogos count
    /// </summary>
    public int GetConsecutivePogos() => consecutivePogos;
    
    /// <summary>
    /// Draws gizmos to visualize pogo detection ranges and mechanics.
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
        
        // Draw field of view cone
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
        
        // 4. Consecutive Pogo Momentum Visualization
        if (consecutivePogos > 0)
        {
            // Draw momentum indicator
            float momentumSize = 0.5f + (consecutivePogos * 0.2f);
            Gizmos.color = new Color(1f, 1f, 0f, 0.8f); // Yellow for momentum
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, momentumSize);
        }
    }
}

