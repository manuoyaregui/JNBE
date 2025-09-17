## Plan de Acción: Mejoras de Arquitectura y Prácticas de Desarrollo

### Fase 1: Fundación y Estructura (Alta Prioridad - Impacto Inmediato)

#### 1.1 Reorganización de Estructura de Carpetas y Assembly Definitions
**Objetivo**: Establecer una base sólida para el desarrollo modular  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Crear nueva estructura de carpetas:
    ```
    Assets/
    ├── _Core/           # Sistemas fundamentales
    ├── _Presentation/   # UI y visuales
    ├── _Infrastructure/ # Servicios y persistencia
    ├── _Game/           # Lógica de juego específica
    └── _Shared/         # Recursos compartidos
    ```
  - Crear Assembly Definitions (.asmdef) por módulo: `Core`, `Presentation`, `Infrastructure`, `Game`, `Managers`.
  - Migrar archivos existentes a la nueva estructura y reparar referencias.

#### 1.2 Event System con ScriptableObjects
**Objetivo**: Desacoplar sistemas mediante eventos  
**Tiempo estimado**: 1-2 días

- **Tareas**:
  - Crear `Scripts/Core/Events/`: `GameEvent`, `GameEventListener`, `VoidEvent`, `IntEvent`, `FloatEvent`, `StringEvent`.
  - Crear eventos del juego: `PlayerHealthChangedEvent`, `EnemyKilledEvent`, `WeaponFiredEvent`, `LevelCompletedEvent`.
  - Reemplazar referencias directas en Managers por emisiones/escuchas de eventos.

#### 1.3 Refactorización de Managers
**Objetivo**: Dividir responsabilidades y crear interfaces  
**Tiempo estimado**: 3-4 días

- **Tareas**:
  - Crear interfaces en `Scripts/Core/Interfaces/`: `IAudioService`, `ISaveService`, `ILevelService`, `IEnemySpawnService`.
  - Dividir Managers: `AudioManager` → `AudioService` + `AudioController`; `GameManager` → `LevelService` + `GameStateController`; `EnemyManager` → `EnemySpawnService` + `EnemyController`.
  - Implementar Service Locator simple o DI ligera.

---

### Fase 2: Gestión de Assets y Contenido (Alta Prioridad - Impacto Medio)

#### 2.1 Addressables
**Objetivo**: Optimizar carga de contenido y memoria  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Configurar Addressables Groups: `Enemies`, `Weapons`, `UI`, `Scenes`.
  - Crear `Scripts/Infrastructure/Addressables/`: `AddressableLoader`, `AddressableManager`.
  - Migrar prefabs pesados a Addressables y liberar memoria al salir de escenas.

#### 2.2 ScriptableObject Catalogs
**Objetivo**: Centralizar configuración y referencias  
**Tiempo estimado**: 2 días

- **Tareas**:
  - Crear `Scripts/Core/Data/`: `WeaponData`, `EnemyData`, `LevelData`, `GameConfig`.
  - Crear `Scripts/Core/Catalogs/`: `WeaponCatalog`, `EnemyCatalog`, `MaterialCatalog`.
  - Migrar configuraciones hardcodeadas a SOs.

#### 2.3 Prefabs y Variants
**Objetivo**: Reutilización y consistencia  
**Tiempo estimado**: 1-2 días

- **Tareas**:
  - Crear prefabs base: `PF_Enemy_Base`, `PF_Weapon_Base`, `PF_Floor_Base`.
  - Crear variantes: `PF_Enemy_Fast`, `PF_Weapon_Rifle`, `PF_Floor_Moving`.
  - Revisar y corregir overrides en instancias.

---

### Fase 3: Sistema de Escenas y Flujo de Juego (Media Prioridad - Impacto Alto)

#### 3.1 Scene Bootstrap
**Objetivo**: Inicialización robusta  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Crear `Scenes/Bootstrap.unity`.
  - Crear `Scripts/Core/Bootstrap/`: `BootstrapManager`, `ServiceInitializer`.
  - Implementar carga asíncrona de escenas principales y transiciones.

#### 3.2 Carga Asíncrona
**Objetivo**: Transiciones suaves  
**Tiempo estimado**: 1-2 días

- **Tareas**:
  - Crear `Scripts/Infrastructure/SceneManagement/`: `SceneLoader`, `LoadingScreen`, `SceneTransition`.
  - Mostrar progreso de carga y pre-cargar assets críticos.

---

### Fase 4: Optimización de Rendimiento (Media Prioridad - Impacto Medio)

#### 4.1 Object Pooling
**Objetivo**: Reducir allocs y picos de GC  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Crear `Scripts/Core/Pooling/`: `ObjectPool`, `PooledObject`, `PoolManager`.
  - Pools para balas, VFX, enemigos y partículas de impacto.

#### 4.2 URP
**Objetivo**: Rendimiento gráfico  
**Tiempo estimado**: 1-2 días

- **Tareas**:
  - Perfiles: `URP-Desktop.asset`, `URP-Mobile.asset`, `URP-LowEnd.asset`.
  - Renderer Features para post-process; optimizar materiales para SRP Batcher.

---

### Fase 5: Sistema de Input y UI (Media Prioridad - Impacto Medio)

#### 5.1 Nuevo Input System
**Objetivo**: Entrada moderna y rebindable  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Crear `Scripts/Core/Input/`: `InputManager`, `InputActions`, `InputHandler`.
  - Mapas: `Gameplay`, `UI`, `Cinematics`; implementar rebinding y persistencia.

#### 5.2 Refactor de UI
**Objetivo**: Arquitectura limpia (MVP/MVVM simple)  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Crear `Scripts/Presentation/UI/`: `UIBase`, `UIManager`, `UIViewController`.
  - Navegación por teclado/mando y estados de UI centralizados.

---

### Fase 6: Testing y Validación (Baja Prioridad - Impacto Alto a Largo Plazo)

#### 6.1 Tests Básicos
**Objetivo**: Base de pruebas  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - Crear `Tests/`: `EditModeTests/`, `PlayModeTests/`, `ValidationTests/`.
  - Tests para daño/salud, spawn, puntuación y validación de prefabs.

#### 6.2 Validación de Assets
**Objetivo**: Prevenir errores  
**Tiempo estimado**: 1-2 días

- **Tareas**:
  - `Scripts/Editor/Validation/`: `AssetValidator`, `PrefabValidator`, `SceneValidator`.
  - Validar referencias rotas, materiales sin shader y overrides excesivos.

---

### Fase 7: Automatización y CI/CD (Baja Prioridad - Impacto Medio)

#### 7.1 CI/CD
**Objetivo**: Builds y validaciones automáticas  
**Tiempo estimado**: 2-3 días

- **Tareas**:
  - `.github/workflows/`: `build.yml`, `test.yml`, `validate.yml`.
  - Configurar Unity Builder multiplataforma y notificaciones.

#### 7.2 Herramientas de Editor
**Objetivo**: Flujo de trabajo mejorado  
**Tiempo estimado**: 1-2 días

- **Tareas**:
  - `Scripts/Editor/Tools/`: `BuildTools`, `AssetTools`, `ValidationTools`.
  - Menús de editor personalizados y scripts de automatización.

---

## Criterios de Éxito por Fase

- **Fase 1**: Compila con .asmdef; eventos en 3+ sistemas; interfaces en Managers.
- **Fase 2**: Addressables cargan dinámicamente; SOs sustituyen configuraciones; prefabs con variantes.
- **Fase 3**: Bootstrap inicializa sistemas; transiciones suaves; barra de progreso en cargas.
- **Fase 4**: 50%+ menos allocs; perfiles URP; -30% draw calls.
- **Fase 5**: Input en todas las pantallas; UI navegable; rebinding activo.
- **Fase 6**: Cobertura de lógica crítica; validación automática; 0 referencias rotas.
- **Fase 7**: Builds automáticos; tests en CI; herramientas de editor disponibles.

---

## Notas para el Agente

- **Priorizar Fases 1-3** para impacto inmediato.
- **Validar cada fase** antes de continuar.
- **Mantener compatibilidad** durante la transición.
- **Documentar cambios** en cada commit.
- **Crear backups** antes de refactors grandes.
- **Probar en varias plataformas** tras cada fase.


