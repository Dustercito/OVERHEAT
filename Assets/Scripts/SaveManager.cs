using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    [SerializeField] private Transform transfJugador;
    [SerializeField] private CharacterController contrPers;
    [SerializeField] private SaludJugador saludJugador;
    [SerializeField] private GestorArmas gestorArmas;

    [Header("Interfaz de Guardado")]
    [SerializeField] private GameObject panelConfirmacion; // Ventana de "Ya existe partida"
    [SerializeField] private GameObject panelAvisoExito;   // Pop-up temporal de "Save Complete"
    [SerializeField] private float tiempoAvisoExito = 1.5f; // Segundos que durará en pantalla

    private void Start()
    {
        // Asegurarse de que los paneles estén ocultos al iniciar
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
        if (panelAvisoExito != null) panelAvisoExito.SetActive(false);

        // Cargar datos automáticamente si hay guardado previo
        if (PlayerPrefs.HasKey("VidaJugador"))
        {
            CargarDatosJugador();
        }
    }

    // --- FLUJO DE GUARDADO ---

    // 1. Este método lo pones en el botón principal "Save Game" del menú de pausa
    public void IntentarGuardar()
    {
        // Verificamos si ya existe una partida
        if (PlayerPrefs.HasKey("VidaJugador"))
        {
            // Ya hay partida, mostramos el aviso de confirmación
            if (panelConfirmacion != null)
            {
                panelConfirmacion.SetActive(true);
            }
        }
        else
        {
            // No hay partida, guardamos directo
            ConfirmarYGuardar();
        }
    }

    // 2. Este método lo pones en el botón "Cancelar/NO" de la ventana de confirmación
    public void CancelarGuardado()
    {
        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(false);
        }
    }

    // 3. Este método lo pones en el botón "SÍ" de la ventana de confirmación, y se llama automáticamente si no había partida.
    public void ConfirmarYGuardar()
    {
        // Ocultamos la ventana de pregunta si estaba abierta
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);

        // Ejecutar lógica de guardado
        EjecutarGuardadoFisico();

        // Mostrar el pop-up de éxito
        if (panelAvisoExito != null)
        {
            StartCoroutine(MostrarAvisoTemporal());
        }
    }

    private IEnumerator MostrarAvisoTemporal()
    {
        panelAvisoExito.SetActive(true);

        // Esperamos en tiempo REAL, porque el juego está pausado (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempoAvisoExito);

        panelAvisoExito.SetActive(false);
    }

    // --- LÓGICA INTERNA DE GUARDADO Y CARGA ---

    private void EjecutarGuardadoFisico()
    {
        PlayerPrefs.SetInt("EscenaGuardada", SceneManager.GetActiveScene().buildIndex);

        if (transfJugador != null)
        {
            PlayerPrefs.SetFloat("JugadorPosX", transfJugador.position.x);
            PlayerPrefs.SetFloat("JugadorPosY", transfJugador.position.y);
            PlayerPrefs.SetFloat("JugadorPosZ", transfJugador.position.z);
        }

        if (saludJugador != null)
        {
            PlayerPrefs.SetFloat("VidaJugador", saludJugador.VidaActual);
        }

        if (gestorArmas != null)
        {
            PlayerPrefs.SetInt("TieneCuchillo", gestorArmas.tieneCuchillo ? 1 : 0);
            PlayerPrefs.SetInt("TienePistola", gestorArmas.tienePistola ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    private void CargarDatosJugador()
    {
        if (transfJugador != null && PlayerPrefs.HasKey("JugadorPosX"))
        {
            float posX = PlayerPrefs.GetFloat("JugadorPosX");
            float posY = PlayerPrefs.GetFloat("JugadorPosY");
            float posZ = PlayerPrefs.GetFloat("JugadorPosZ");
            Vector3 posGuardada = new Vector3(posX, posY, posZ);

            if (contrPers != null) contrPers.enabled = false;
            transfJugador.position = posGuardada;
            if (contrPers != null) contrPers.enabled = true;
        }

        if (saludJugador != null)
        {
            float vidaGuardada = PlayerPrefs.GetFloat("VidaJugador", saludJugador.VidaMaxima);
            saludJugador.Curar(vidaGuardada);
        }

        if (gestorArmas != null)
        {
            gestorArmas.tieneCuchillo = PlayerPrefs.GetInt("TieneCuchillo", 0) == 1;
            gestorArmas.tienePistola = PlayerPrefs.GetInt("TienePistola", 0) == 1;

            if (gestorArmas.tienePistola) gestorArmas.EquiparArma(2);
            else if (gestorArmas.tieneCuchillo) gestorArmas.EquiparArma(1);
        }
    }
}