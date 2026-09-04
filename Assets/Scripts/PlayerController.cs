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

    [Header("Efecto Camara Doom (Head Bobbing)")]
    [SerializeField] private bool usarEfectoBobbing = true;
    [SerializeField] private float frecuenciaBobbing = 12.0f; 
    [SerializeField] private float amplitudBobbingY = 0.05f;  
    [SerializeField] private float amplitudBobbingX = 0.03f;  
    [SerializeField] private float velocidadSuavizado = 8.0f; 

    [Header("Configuracion de Sonidos de Pasos")]
    [SerializeField] private AudioSource fuenteAudioPasos;
    [SerializeField] private AudioClip[] sonidosPasos;
    [SerializeField] [Range(0.1f, 1.0f)] private float volumenPasos = 0.5f;
    [SerializeField] private float intervaloPasos = 0.45f; 
    private float temporizadorPasos = 0.0f;

    [Header("Configuracion de Linterna")]
    [SerializeField] private Light luzLinterna;
    [SerializeField] private AudioClip sonidoInterruptorLinterna;
    private bool linternaEncendida = true;

    // --- VARIABLES INTERNAS ---
    private Vector2 entMov;
    private Vector2 entCam;
    private float rotVert = 0.0f;
    private Vector3 velVert;

    private Vector3 posInicialCamara;
    private float temporizadorBobbing = 0.0f;

    // --- INICIALIZACION ---
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (transfCamara != null)
        {
            posInicialCamara = transfCamara.localPosition;
        }

        if (fuenteAudioPasos == null)
        {
            fuenteAudioPasos = GetComponent<AudioSource>();
            if (fuenteAudioPasos == null)
            {
                fuenteAudioPasos = gameObject.AddComponent<AudioSource>();
            }
        }

        // Asegurar estado inicial de la linterna
        if (luzLinterna != null)
        {
            luzLinterna.enabled = linternaEncendida;
        }

        if (UnityEngine.InputSystem.Gyroscope.current != null)
        {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        }
    }

    // --- BUCLE PRINCIPAL DE LOGICA ---
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState == CursorLockMode.Locked)
        {
            EjecutarAtaque();
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame && gestorArmas != null) gestorArmas.EquiparArma(1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame && gestorArmas != null) gestorArmas.EquiparArma(2);
            
            // Alternar Linterna con la tecla F en PC
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                AlternarLinterna();
            }
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            EjecutarRecarga();
        }

        entMov = Vector2.zero;

        if (Gamepad.current != null)
        {
            entMov = Gamepad.current.leftStick.ReadValue();
            entCam = Gamepad.current.rightStick.ReadValue();
        }

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

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RealizarDash();
        }

        ProcesarMovimiento();
        ProcesarVista();
        ProcesarEfectoDoomCamara();
        ProcesarSonidoPasos();
    }

    // --- LOGICA DE MOVIMIENTO Y GRAVEDAD ---
    private void ProcesarMovimiento()
    {
        if (estaEnDash) return;

        if (contrPers.isGrounded && velVert.y < 0)
        {
            velVert.y = -2f;
        }

        Vector3 dirMov = transform.right * entMov.x + transform.forward * entMov.y;
        contrPers.Move(dirMov * velMov * Time.deltaTime);

        velVert.y += gravedad * Time.deltaTime;
        contrPers.Move(velVert * Time.deltaTime);
    }

    // --- LOGICA DE VISTA Y CAMARA (PC / ANDROID) ---
    private void ProcesarVista()
    {
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 deltaMouse = Mouse.current.delta.ReadValue();

            rotVert -= deltaMouse.y * sensMouse;
            rotVert = Mathf.Clamp(rotVert, -80f, 80f);
            transfCamara.localRotation = Quaternion.Euler(rotVert, transfCamara.localRotation.eulerAngles.y, 0f);

            transform.Rotate(Vector3.up * deltaMouse.x * sensMouse);
            return;
        }

        if (entCam.sqrMagnitude > 0.01f)
        {
            rotVert -= entCam.y * sensJoystick;
            rotVert = Mathf.Clamp(rotVert, -80f, 80f);
            transfCamara.localRotation = Quaternion.Euler(rotVert, transfCamara.localRotation.eulerAngles.y, 0f);

            transform.Rotate(Vector3.up * entCam.x * sensJoystick);
            return;
        }

        var girosc = UnityEngine.InputSystem.Gyroscope.current;
        if (girosc != null)
        {
            Vector3 deltaGirosc = girosc.angularVelocity.ReadValue();
            if (deltaGirosc.sqrMagnitude > 0.001f)
            {
                rotVert -= deltaGirosc.x * sensGirosc;
                rotVert = Mathf.Clamp(rotVert, -80f, 80f);
                transfCamara.localRotation = Quaternion.Euler(rotVert, transfCamara.localRotation.eulerAngles.y, 0f);

                transform.Rotate(Vector3.up * deltaGirosc.y * sensGirosc);
            }
        }
    }

    // --- LOGICA DEL BAMBOLEO DE CAMARA TIPO DOOM ---
    private void ProcesarEfectoDoomCamara()
    {
        if (!usarEfectoBobbing || transfCamara == null) return;

        if (contrPers.isGrounded && entMov.sqrMagnitude > 0.01f && !estaEnDash)
        {
            temporizadorBobbing += Time.deltaTime * frecuenciaBobbing;

            float desplazamientoY = Mathf.Sin(temporizadorBobbing) * amplitudBobbingY;
            float desplazamientoX = Mathf.Cos(temporizadorBobbing * 0.5f) * amplitudBobbingX;

            Vector3 posObjetivo = posInicialCamara + new Vector3(desplazamientoX, desplazamientoY, 0f);
            transfCamara.localPosition = Vector3.Lerp(transfCamara.localPosition, posObjetivo, Time.deltaTime * velocidadSuavizado);
        }
        else
        {
            temporizadorBobbing = 0.0f;
            transfCamara.localPosition = Vector3.Lerp(transfCamara.localPosition, posInicialCamara, Time.deltaTime * velocidadSuavizado);
        }
    }

    // --- REPRODUCCION DE PASOS DE AUDIO ---
    private void ProcesarSonidoPasos()
    {
        if (contrPers.isGrounded && entMov.sqrMagnitude > 0.01f && !estaEnDash)
        {
            temporizadorPasos += Time.deltaTime;

            if (temporizadorPasos >= intervaloPasos)
            {
                ReproducirPasoAleatorio();
                temporizadorPasos = 0.0f;
            }
        }
        else
        {
            temporizadorPasos = intervaloPasos;
        }
    }

    private void ReproducirPasoAleatorio()
    {
        if (sonidosPasos == null || sonidosPasos.Length == 0 || fuenteAudioPasos == null) return;

        int indiceAleatorio = Random.Range(0, sonidosPasos.Length);
        AudioClip clipSeleccionado = sonidosPasos[indiceAleatorio];

        if (clipSeleccionado != null)
        {
            fuenteAudioPasos.pitch = Random.Range(0.9f, 1.1f);
            fuenteAudioPasos.PlayOneShot(clipSeleccionado, volumenPasos);
        }
    }

    // --- MECANICA DE LINTERNA (ACTIVACION PÚBLICA / UI ANDROID / TECLA F) ---
    public void AlternarLinterna()
    {
        if (luzLinterna != null)
        {
            linternaEncendida = !linternaEncendida;
            luzLinterna.enabled = linternaEncendida;

            if (fuenteAudioPasos != null && sonidoInterruptorLinterna != null)
            {
                fuenteAudioPasos.PlayOneShot(sonidoInterruptorLinterna, 0.7f);
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

    // --- MECANICA DE RECARGA (ACTIVACION PÚBLICA / UI ANDROID) ---
    public void EjecutarRecarga()
    {
        if (gestorArmas != null)
        {
            gestorArmas.EjecutarRecarga();
        }
    }

    // --- MECANICA DE DASH (ACTIVACION PÚBLICA) ---
    public void RealizarDash()
    {
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

        Vector3 dirDash = transform.right * entMov.x + transform.forward * entMov.y;
        if (dirDash.sqrMagnitude < 0.01f)
        {
            dirDash = transform.forward;
        }
        dirDash.Normalize();

        float temp = 0f;
        while (temp < durDash)
        {
            contrPers.Move(dirDash * velDash * Time.deltaTime);
            temp += Time.deltaTime;
            yield return null;
        }

        estaEnDash = false;

        yield return new WaitForSeconds(recargaDash);
        puedeDash = true;
    }
}