using UnityEngine;
using DG.Tweening;

/// <summary>
/// Sistema de dash del jugador. Permite al jugador hacer dash en la dirección
/// de la cámara. Incluye gestión de cooldown para prevenir spam de dash.
/// Usa DOTween para animaciones suaves y eficientes.
/// </summary>
public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashTime = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    
    private CharacterController characterController;
    private Camera mainCamera;
    private PlayerController playerController;
    
    private float dashCD;
    private Tween dashTween;
    
    /// <summary>
    /// Initialize dash controller with required dependencies
    /// </summary>
    public void Setup(CharacterController controller, Camera camera, PlayerController playerCtrl,
                     float speed, float time, float cooldown)
    {
        characterController = controller;
        mainCamera = camera;
        playerController = playerCtrl;
        dashSpeed = speed;
        dashTime = time;
        dashCooldown = cooldown;
    }
    
    /// <summary>
    /// Handles dash input and execution. Allows the player to dash in camera direction.
    /// Includes cooldown management to prevent spam dashing. Now uses DOTween for better performance.
    /// </summary>
    public void HandleDash()
    {
        if (characterController == null || mainCamera == null) return;
        
        if (Time.time > dashCD && Input.GetButtonDown("Fire3"))
        {
            if (playerController != null)
            {
                playerController.PlayerIsDashing();
            }
            ExecuteDash();
            dashCD = Time.time + dashCooldown;
        }
    }
    
    /// <summary>
    /// Executes dash movement using DOTween for smooth and performant animation.
    /// Dash always goes in the direction the player is looking (camera direction).
    /// </summary>
    public void ExecuteDash()
    {
        if (characterController == null || mainCamera == null) return;
        
        // Kill any existing dash tween
        dashTween?.Kill();
        
        // Get camera forward direction (where player is looking)
        Vector3 dashDirection = mainCamera.transform.forward;
        
        // Calculate dash target position
        Vector3 dashTarget = transform.position + dashDirection * dashSpeed * dashTime;
        
        // Create smooth dash animation
        dashTween = transform.DOMove(dashTarget, dashTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = dashTarget;
                Vector3 movement = (nextPos - currentPos).normalized * dashSpeed * Time.deltaTime;
                characterController.Move(movement);
            })
            .OnComplete(() => {
                // Resetear la animación de dash cuando termine
                if (playerController != null)
                {
                    playerController.StopDashAnimation();
                }
            });
    }
    
    /// <summary>
    /// Cancels any active dash
    /// </summary>
    public void CancelDash()
    {
        dashTween?.Kill();
        if (playerController != null)
        {
            playerController.StopDashAnimation();
        }
    }
    
    /// <summary>
    /// Checks if dash is currently active
    /// </summary>
    public bool IsDashing()
    {
        return dashTween != null && dashTween.IsActive();
    }
    
    /// <summary>
    /// Resets dash cooldown (useful for pogo reset)
    /// </summary>
    public void ResetCooldown()
    {
        dashCD = 0f;
    }
    
    /// <summary>
    /// Gets the dash tween (useful for other systems like pogo)
    /// </summary>
    public Tween GetDashTween() => dashTween;
    
    /// <summary>
    /// Cleanup on destroy
    /// </summary>
    private void OnDestroy()
    {
        dashTween?.Kill();
    }
}

