    /// <summary>
/// Tipos de superficie detectados por el sistema de colisiones inteligente
/// </summary>
public enum SurfaceType
{
    None,       // No hay superficie detectada
    Floor,      // Superficie horizontal (piso)
    Wall,       // Superficie vertical (pared)
    Ceiling,    // Superficie superior
    Slope       // Superficie inclinada (entre piso y pared)
}

