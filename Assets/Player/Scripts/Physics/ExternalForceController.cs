using UnityEngine;
using DG.Tweening;

/// <summary>
/// Sistema de fuerzas externas aplicadas al jugador. Maneja el recoil de la escopeta
/// y el lanzamiento del jugador desde rampas. Usa DOTween para animaciones suaves.
/// Integra con GravityController para resetear el vector de gravedad después de aplicar fuerzas.
/// </summary>
public class ExternalForceController : MonoBehaviour
{
    [Header("Force Settings")]
    [SerializeField] private float wallSlideGravity = -2f;
    
    private CharacterController characterController;
    private GravityController gravityController;
    private InertiaController inertiaController;
    private GameObject camera;
    
    private bool isBeingThrown = false;
    private Tween recoilTween;
    private Tween throwTween;
    
    /// <summary>
    /// Initialize external force controller with required dependencies
    /// </summary>
    public void Setup(CharacterController controller, GravityController gravityCtrl,
                     InertiaController inertiaCtrl, GameObject cameraRef, float wallSlideGrav)
    {
        characterController = controller;
        gravityController = gravityCtrl;
        inertiaController = inertiaCtrl;
        camera = cameraRef;
        wallSlideGravity = wallSlideGrav;
    }
    
    /// <summary>
    /// Handles shotgun recoil using DOTween for smooth backward movement.
    /// </summary>
    public void ExecuteRecoil(float recoilTime, float recoilSpeed)
    {
        if (characterController == null || camera == null) return;
        
        // Kill any existing recoil tween
        recoilTween?.Kill();
        
        // Calculate recoil direction (opposite to camera forward)
        Vector3 recoilDirection = -camera.transform.forward;
        Vector3 recoilTarget = transform.position + recoilDirection * recoilSpeed * recoilTime;
        
        // Create smooth recoil animation
        recoilTween = transform.DOMove(recoilTarget, recoilTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = recoilTarget;
                Vector3 movement = (nextPos - currentPos).normalized * recoilSpeed * Time.deltaTime;
                characterController.Move(movement);
            })
            .OnComplete(() => {
                // Reset gravity vector after recoil
                if (gravityController != null)
                {
                    gravityController.ResetGravityVector();
                }
            });
    }
    
    /// <summary>
    /// Executes player throw using DOTween for smooth and performant animation.
    /// Used for ramps and pogo bounces.
    /// </summary>
    public void ExecuteThrow(Vector3 throwDirection, float rampPower, float rampTime)
    {
        if (characterController == null) return;
        
        // Kill any existing throw tween
        throwTween?.Kill();
        
        isBeingThrown = true;
        
        // Calculate throw target position
        Vector3 throwTarget = transform.position + throwDirection * rampPower * rampTime;
        
        // Create smooth throw animation
        throwTween = transform.DOMove(throwTarget, rampTime)
            .SetEase(Ease.OutQuad)
            .OnUpdate(() => {
                // Apply movement to character controller for collision detection
                Vector3 currentPos = transform.position;
                Vector3 nextPos = throwTarget;
                Vector3 movement = (nextPos - currentPos).normalized * rampPower * Time.deltaTime;
                characterController.Move(movement);
            })
            .OnComplete(() => {
                // Reset gravity and throw state
                if (gravityController != null)
                {
                    gravityController.ResetGravityVector();
                }
                isBeingThrown = false;
            });
    }
    
    /// <summary>
    /// Handles collision with ramp and applies throw force
    /// </summary>
    public void HandleRampCollision(Vector3 throwDirection, float rampPower, float rampTime)
    {
        if (isBeingThrown == false)
        {
            ExecuteThrow(throwDirection, rampPower, rampTime);
            
            // Apply inertia boost from ramp
            if (inertiaController != null)
            {
                inertiaController.SetInertia(1.5f);
                inertiaController.SetInertiaFOV(inertiaController.GetInertiaFOV() + 2.5f);
            }
        }
    }
    
    /// <summary>
    /// Checks if player is currently being thrown
    /// </summary>
    public bool IsBeingThrown() => isBeingThrown;
    
    /// <summary>
    /// Cancels any active throw
    /// </summary>
    public void CancelThrow()
    {
        throwTween?.Kill();
        isBeingThrown = false;
    }
    
    /// <summary>
    /// Cleanup on destroy
    /// </summary>
    private void OnDestroy()
    {
        recoilTween?.Kill();
        throwTween?.Kill();
    }
}

