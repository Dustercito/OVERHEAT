using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACION ---
    [Header("Configuracion de Movimiento")]
    [SerializeField] private CharacterController contrPers;
    [SerializeField] private float velMov = 4.0f;
    [SerializeField] private float gravedad = -9.81f;

    [Header("Configuracion de Dash (Esquiva)")]
    [SerializeField] private float velDash = 14.0f;
    [SerializeField] private float durDash = 0.2f;
    [SerializeField] private float recargaDash = 1.5f;
    private bool estaEnDash = false;
    private bool puedeDash = true;

    [Header("Armas y Combate")]
    [SerializeField] private GestorArmas gestorArmas;

    [Header("Sensibilidad y Camara")]
    [SerializeField] private Transform transfCamara;
    [SerializeField] private float sensJoystick = 1.0f;
    [SerializeField] private float sensGirosc = 1.2f;
    [SerializeField] private float sensMouse = 0.2f;

    // --- VARIABLES INTERNAS ---
    private Vector2 entMov;
    private Vector2 entCam;
    private float rotVert = 0.0f;
    private Vector3 velVert;

    // --- INICIALIZACION ---
    private void Start()
    {
        // Ocultar y bloquear cursor para PC
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Habilitar giroscopio en moviles si esta disponible
        if (UnityEngine.InputSystem.Gyroscope.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        }
    }

    // --- BUCLE PRINCIPAL DE LOGICA ---
    private void Update()
    {
        // Liberar cursor en PC con la tecla ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Volver a bloquear cursor al dar clic en la pantalla
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // --- ENTRADA DE ATAQUE / DISPARO (Clic Izquierdo en PC) ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.Locked)
        {
            EjecutarAtaque();
        }

        // --- CAMBIO DE ARMA EN PC (Teclas 1 y 2) ---
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && gestorArmas != null) gestorArmas.EquiparArma(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame && gestorArmas != null) gestorArmas.EquiparArma(2);
        }

        // Resetear vector de entrada
        entMov = Vector2.zero;

        // Leer Joysticks (Gamepad / Pantalla tactil Android)
        if (Gamepad.current != null)
        {
            entMov = Gamepad.current.leftStick.ReadValue();
            entCam = Gamepad.current.rightStick.ReadValue();
        }

        // Leer Teclado WASD en PC (si no hay Joystick activo)
        if (entMov.sqrMagnitude < 0.01f && Keyboard.current != null)
        {
            float x = 0f;
            float y = 0f;

            if (Keyboard.current.wKey.isPressed) y += 1f;
            if (Keyboard.current.sKey.isPressed) y -= 1f;
            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;

            entMov = new Vector2(x, y).normalized;
        }

        // Activar Dash con la tecla Espacio en PC
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RealizarDash();
        }

        // Ejecutar calculo de fisicas y rotacion de camara
        ProcesarMovimiento();
        ProcesarVista();
    }

    // --- LOGICA DE MOVIMIENTO Y GRAVEDAD ---
    private void ProcesarMovimiento()
    {
        // Cancelar movimiento normal durante el Dash
        if (estaEnDash) return;

        // Estabilizar personaje en el suelo
        if (contrPers.isGrounded && velVert.y < 0)
        {
            velVert.y = -2f;
        }

        // Movimiento horizontal relativo a la rotacion del jugador
        Vector3 dirMov = transform.right * entMov.x + transform.forward * entMov.y;
        contrPers.Move(dirMov * velMov * Time.deltaTime);

        // Aplicar fuerza de gravedad vertical
        velVert.y += gravedad * Time.deltaTime;
        contrPers.Move(velVert * Time.deltaTime);
    }

    // --- LOGICA DE VISTA Y CAMARA (PC / ANDROID) ---
    private void ProcesarVista()
    {
        // Modo 1: Control de camara con Mouse (PC)
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 deltaMouse = Mouse.current.delta.ReadValue();

            // Rotacion vertical (Camara) con limite de inclinacion
            rotVert -= deltaMouse.y * sensMouse;
            rotVert = Mathf.Clamp(rotVert, -80f, 80f);
            transfCamara.localRotation = Quaternion.Euler(rotVert, 0f, 0f);

            // Rotacion horizontal (Cuerpo del personaje)
            transform.Rotate(Vector3.up * deltaMouse.x * sensMouse);
            return;
        }

        // Modo 2: Control con el Joystick Derecho (UI Android / Gamepad)
        if (entCam.sqrMagnitude > 0.01f)
        {
            rotVert -= entCam.y * sensJoystick;
            rotVert = Mathf.Clamp(rotVert, -80f, 80f);
            transfCamara.localRotation = Quaternion.Euler(rotVert, 0f, 0f);

            transform.Rotate(Vector3.up * entCam.x * sensJoystick);
            return;
        }

        // Modo 3: Control por Giroscopio (Sensor en APK Nativo)
        var girosc = UnityEngine.InputSystem.Gyroscope.current;
        if (girosc != null)
        {
            Vector3 deltaGirosc = girosc.angularVelocity.ReadValue();
            if (deltaGirosc.sqrMagnitude > 0.001f)
            {
                rotVert -= deltaGirosc.x * sensGirosc;
                rotVert = Mathf.Clamp(rotVert, -80f, 80f);
                transfCamara.localRotation = Quaternion.Euler(rotVert, 0f, 0f);

                transform.Rotate(Vector3.up * deltaGirosc.y * sensGirosc);
            }
        }
    }

    // --- MECANICA DE ATAQUE (ACTIVACION PÚBLICA / UI ANDROID) ---
    public void EjecutarAtaque()
    {
        if (gestorArmas != null)
        {
            gestorArmas.EjecutarAtaque();
        }
    }

    // --- MECANICA DE DASH (ACTIVACION PÚBLICA) ---
    public void RealizarDash()
    {
        // Ejecutar corrutina si no esta en cooldown
        if (puedeDash && !estaEnDash)
        {
            StartCoroutine(RutinaDash());
        }
    }

    // --- CORRUTINA DE IMPULSO Y RECARGA ---
    private IEnumerator RutinaDash()
    {
        puedeDash = false;
        estaEnDash = true;

        // Calcular direccion del impulso (Direccion actual o hacia adelante)
        Vector3 dirDash = transform.right * entMov.x + transform.forward * entMov.y;
        if (dirDash.sqrMagnitude < 0.01f)
        {
            dirDash = transform.forward;
        }
        dirDash.Normalize();

        // Aplicar velocidad del Dash durante la duracion configurada
        float temp = 0f;
        while (temp < durDash)
        {
            contrPers.Move(dirDash * velDash * Time.deltaTime);
            temp += Time.deltaTime;
            yield return null;
        }

        estaEnDash = false;

        // Tiempo de espera para recargar la habilidad
        yield return new WaitForSeconds(recargaDash);
        puedeDash = true;
    }
}