using UnityEngine;

/// <summary>
/// Sistema inteligente de detección de superficies basado en ángulos.
/// Elimina la dependencia de layers específicos y maneja casos complejos como bordes.
/// Responsable de detectar y clasificar las superficies con las que el jugador interactúa.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Legacy naming convention")]
public class SurfaceDetector : MonoBehaviour
{
    [Header("Collision Detection")]
    [SerializeField] private LayerMask collisionLayers = -1; // Todas las capas por defecto
    [SerializeField] private float floorAngleThreshold = 45f; // Ángulo máximo para considerar superficie como piso (en grados)
    [SerializeField] private float wallAngleThreshold = 15f; // Ángulo máximo para considerar superficie como pared (en grados)
    
    [Header("Raycast References")]
    [SerializeField] private GameObject footPoint; // puntos de apoyo y colision
    [SerializeField] private GameObject wallPointL, wallPointR;
    
    [Header("Physics Constants")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    
    // Current surface detection state
    private SurfaceType currentSurfaceType = SurfaceType.None;
    private Vector3 currentSurfaceNormal = Vector3.up;
    private float currentSurfaceAngle = 0f;
    
    // Legacy variables for compatibility
    private bool isInFloor;
    private bool isInWall;
    private bool isInWallLeft;
    private bool isInWallRight;
    
    // Public accessors for legacy variables
    public bool IsInFloor => isInFloor;
    public bool IsInWall => isInWall;
    public bool IsInWallLeft => isInWallLeft;
    public bool IsInWallRight => isInWallRight;
    
    /// <summary>
    /// Initialize surface detector with required references
    /// </summary>
    public void Setup(GameObject footPointRef, GameObject wallPointLRef, GameObject wallPointRRef,
                     LayerMask layers, float floorAngle, float wallAngle, float groundRadius, float wallDistance)
    {
        footPoint = footPointRef;
        wallPointL = wallPointLRef;
        wallPointR = wallPointRRef;
        collisionLayers = layers;
        floorAngleThreshold = floorAngle;
        wallAngleThreshold = wallAngle;
        groundCheckRadius = groundRadius;
        wallCheckDistance = wallDistance;
    }
    
    /// <summary>
    /// Detecta superficie debajo del player (piso, pendientes, techos)
    /// </summary>
    private void DetectGroundSurface()
    {
        if (footPoint == null) return;
        
        RaycastHit hit;
        Vector3 rayOrigin = footPoint.transform.position;
        Vector3 rayDirection = Vector3.down;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, groundCheckRadius + 0.1f, collisionLayers))
        {
            currentSurfaceNormal = hit.normal;
            currentSurfaceAngle = Vector3.Angle(Vector3.up, hit.normal);
            
            // Determinar tipo de superficie basado en el ángulo
            if (currentSurfaceAngle <= floorAngleThreshold)
            {
                currentSurfaceType = SurfaceType.Floor;
            }
            else if (currentSurfaceAngle <= 90f - wallAngleThreshold)
            {
                currentSurfaceType = SurfaceType.Slope;
            }
            else if (currentSurfaceAngle <= 90f + wallAngleThreshold)
            {
                currentSurfaceType = SurfaceType.Wall;
            }
            else
            {
                currentSurfaceType = SurfaceType.Ceiling;
            }
        }
        else
        {
            currentSurfaceType = SurfaceType.None;
            currentSurfaceNormal = Vector3.up;
            currentSurfaceAngle = 0f;
        }
    }
    
    /// <summary>
    /// Detecta superficies laterales para determinar si estamos en una pared
    /// </summary>
    private void DetectWallSurfaces()
    {
        if (wallPointL == null || wallPointR == null) return;
        
        RaycastHit hitL, hitR;
        bool leftWall = Physics.Raycast(wallPointL.transform.position, transform.TransformDirection(Vector3.left), 
                                      out hitL, wallCheckDistance, collisionLayers);
        bool rightWall = Physics.Raycast(wallPointR.transform.position, transform.TransformDirection(Vector3.right), 
                                       out hitR, wallCheckDistance, collisionLayers);
        
        // Determinar qué pared estamos tocando
        if (leftWall || rightWall)
        {
            // Si estamos tocando una pared lateral Y no hay superficie debajo, es una pared
            if (currentSurfaceType == SurfaceType.None || currentSurfaceType == SurfaceType.Ceiling)
            {
                currentSurfaceType = SurfaceType.Wall;
            }
            // Si hay superficie debajo, usar sistema de prioridades
            else if (currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope)
            {
                // Caso especial: estamos en un borde (tanto piso como pared)
                // Priorizar el piso si el ángulo es menor al threshold
                if (currentSurfaceAngle <= floorAngleThreshold)
                {
                    // Mantener como Floor, pero marcar que también hay pared
                    isInWallLeft = leftWall;
                    isInWallRight = rightWall;
                    return;
                }
                else
                {
                    // Si el ángulo es mayor, tratarlo como pared
                    currentSurfaceType = SurfaceType.Wall;
                }
            }
        }
        
        isInWallLeft = leftWall;
        isInWallRight = rightWall;
    }
    
    /// <summary>
    /// Actualiza las variables legacy para mantener compatibilidad con el código existente
    /// </summary>
    private void UpdateLegacyVariables()
    {
        // Actualizar isInFloor basado en el nuevo sistema
        isInFloor = (currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope);
        
        // Actualizar isInWall basado en el nuevo sistema
        isInWall = (currentSurfaceType == SurfaceType.Wall || isInWallLeft || isInWallRight);
    }
    
    /// <summary>
    /// Sistema inteligente de detección de superficies basado en ángulos.
    /// Elimina la dependencia de layers específicos y maneja casos complejos como bordes.
    /// </summary>
    public void DetectSurface()
    {
        // Detectar superficie debajo del player (para piso/pendiente)
        DetectGroundSurface();
        
        // Detectar superficies laterales (para paredes)
        DetectWallSurfaces();
        
        // Actualizar variables legacy para compatibilidad
        UpdateLegacyVariables();
    }
    
    // Métodos públicos para obtener información sobre la superficie actual
    public SurfaceType GetCurrentSurfaceType() => currentSurfaceType;
    public Vector3 GetCurrentSurfaceNormal() => currentSurfaceNormal;
    public float GetCurrentSurfaceAngle() => currentSurfaceAngle;
    public bool IsOnWalkableSurface() => currentSurfaceType == SurfaceType.Floor || currentSurfaceType == SurfaceType.Slope;
    public bool IsOnWallSurface() => currentSurfaceType == SurfaceType.Wall;
}

