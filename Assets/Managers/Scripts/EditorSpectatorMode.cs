#if UNITY_EDITOR
using UnityEngine;

namespace JNBE
{
    /// <summary>
    /// Modo espectador solo para pruebas en el editor.
    /// Se activa con F12 para entrar/salir del modo.
    /// Solo funciona en el editor (no en builds finales).
    /// </summary>
    public class EditorSpectatorMode : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Velocidad de movimiento en modo espectador")]
        [SerializeField] private float flySpeed = 10f;
        
        [Tooltip("Velocidad de movimiento rápida (presiona Shift)")]
        [SerializeField] private float fastFlySpeed = 50f;
        
        [Tooltip("Velocidad de rotación del mouse")]
        [SerializeField] private float mouseSensitivity = 2f;

        private bool isSpectatorMode = false;
        private float rotationX = 0f;
        private float rotationY = 0f;

        private Camera spectatorCamera;
        private GameObject spectatorCameraObj;
        private Camera originalCamera;
        
        private bool wasGamePaused = false;

        private void Update()
        {
            // Solo en el editor
            if (!Application.isEditor)
                return;

            // Toggle con F12
            if (Input.GetKeyDown(KeyCode.F12))
            {
                ToggleSpectatorMode();
            }

            // Manejar input del espectador
            if (isSpectatorMode)
            {
                HandleSpectatorInput();
            }
        }

        private void ToggleSpectatorMode()
        {
            isSpectatorMode = !isSpectatorMode;

            if (isSpectatorMode)
            {
                EnterSpectatorMode();
            }
            else
            {
                ExitSpectatorMode();
            }
        }

        private void EnterSpectatorMode()
        {
            // Guardar el estado de pausa del juego
            wasGamePaused = false;

            // Crear cámara de espectador
            if (spectatorCameraObj == null)
            {
                spectatorCameraObj = new GameObject("SpectatorCamera");
                spectatorCamera = spectatorCameraObj.AddComponent<Camera>();
                spectatorCamera.enabled = true;
            }
            else
            {
                spectatorCameraObj.SetActive(true);
            }

            // Desactivar la cámara original
            originalCamera = Camera.main;
            if (originalCamera != null)
            {
                originalCamera.enabled = false;
            }

            // Configurar la posición inicial de la cámara espectador
            if (originalCamera != null)
            {
                spectatorCamera.transform.position = originalCamera.transform.position;
                spectatorCamera.transform.rotation = originalCamera.transform.rotation;
            }
            else
            {
                spectatorCamera.transform.position = Vector3.zero;
                spectatorCamera.transform.rotation = Quaternion.identity;
            }

            // Guardar la rotación actual
            rotationX = spectatorCamera.transform.eulerAngles.x;
            rotationY = spectatorCamera.transform.eulerAngles.y;

            // Bloquear y ocultar el cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ExitSpectatorMode()
        {
            // Restaurar la cámara original
            if (originalCamera != null)
            {
                originalCamera.enabled = true;
            }

            // Desactivar la cámara de espectador
            if (spectatorCameraObj != null)
            {
                spectatorCameraObj.SetActive(false);
            }

            // Desbloquear el cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleSpectatorInput()
        {
            // Rotación con el mouse
            rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            spectatorCamera.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

            // Movimiento
            Vector3 moveDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
                moveDirection += spectatorCamera.transform.forward;
            if (Input.GetKey(KeyCode.S))
                moveDirection -= spectatorCamera.transform.forward;
            if (Input.GetKey(KeyCode.A))
                moveDirection -= spectatorCamera.transform.right;
            if (Input.GetKey(KeyCode.D))
                moveDirection += spectatorCamera.transform.right;
            if (Input.GetKey(KeyCode.Q))
                moveDirection += Vector3.down;
            if (Input.GetKey(KeyCode.E))
                moveDirection += Vector3.up;

            // Aplicar velocidad (rápida o normal)
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? fastFlySpeed : flySpeed;
            spectatorCamera.transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;

            // Desbloquear cursor con Escape o F12
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Volver a bloquear con click izquierdo
            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnGUI()
        {
            if (isSpectatorMode)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 16;
                style.normal.textColor = Color.yellow;
                
                string info = "MODO ESPECTADOR ACTIVO\n" +
                             "WASD - Movimiento\n" +
                             "Q/E - Bajar/Subir\n" +
                             "Shift - Velocidad rápida\n" +
                             "F12 - Salir del modo espectador\n" +
                             "Escape - Liberar cursor";

                GUI.Label(new Rect(10, 10, 500, 150), info, style);
            }
        }
    }
}
#endif

