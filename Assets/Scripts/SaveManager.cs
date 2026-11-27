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

    [Header("Punto de Inicio Obligatorio del Nivel")]
    [Tooltip("Objeto vacío donde aparecerá el jugador al entrar nuevo a este nivel")]
    [SerializeField] private Transform puntoInicioNivel;

    [Header("Interfaz de Guardado")]
    [SerializeField] private GameObject panelConfirmacion;
    [SerializeField] private GameObject panelAvisoExito;
    [SerializeField] private float tiempoAvisoExito = 1.5f;

    private void Start()
    {
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
        if (panelAvisoExito != null) panelAvisoExito.SetActive(false);

        int escenaActual = SceneManager.GetActiveScene().buildIndex;
        int escenaGuardada = PlayerPrefs.GetInt("EscenaGuardada", -1);
        bool usarPosicionGuardada = PlayerPrefs.GetInt("UsarPosicionGuardada", 0) == 1;

        // 1. Decidir dónde colocar al jugador (Spawn del nivel vs Posición guardada en esta misma escena)
        if (usarPosicionGuardada && escenaGuardada == escenaActual && PlayerPrefs.HasKey("JugadorPosX"))
        {
            Vector3 posGuardada = new Vector3(
                PlayerPrefs.GetFloat("JugadorPosX"),
                PlayerPrefs.GetFloat("JugadorPosY"),
                PlayerPrefs.GetFloat("JugadorPosZ")
            );
            TeletransportarJugador(posGuardada, transfJugador != null ? transfJugador.rotation : Quaternion.identity);
        }
        else if (puntoInicioNivel != null)
        {
            TeletransportarJugador(puntoInicioNivel.position, puntoInicioNivel.rotation);
        }

        // 2. Cargar vida, armas y munición si existen datos de progreso
        if (PlayerPrefs.HasKey("VidaJugador"))
        {
            CargarEstadoJugador();
        }
    }

    private void TeletransportarJugador(Vector3 destino, Quaternion rotacion)
    {
        if (transfJugador == null) return;

        if (contrPers != null) contrPers.enabled = false;
        transfJugador.position = destino;
        transfJugador.rotation = rotacion;
        if (contrPers != null) contrPers.enabled = true;
    }

    // --- FLUJO DE GUARDADO MANUAL (MENÚ DE PAUSA) ---
    public void IntentarGuardar()
    {
        if (PlayerPrefs.HasKey("VidaJugador"))
        {
            if (panelConfirmacion != null) panelConfirmacion.SetActive(true);
        }
        else
        {
            ConfirmarYGuardar();
        }
    }

    public void CancelarGuardado()
    {
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
    }

    public void ConfirmarYGuardar()
    {
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);

        EjecutarGuardadoFisico(guardarPosicionExacta: true);

        if (panelAvisoExito != null)
        {
            StartCoroutine(MostrarAvisoTemporal());
        }
    }

    private IEnumerator MostrarAvisoTemporal()
    {
        panelAvisoExito.SetActive(true);
        yield return new WaitForSecondsRealtime(tiempoAvisoExito);
        panelAvisoExito.SetActive(false);
    }

    // --- GUARDADO INTERNO (Usado tanto por Pausa como por TriggerCambioNivel) ---
    public void EjecutarGuardadoFisico(bool guardarPosicionExacta, int? indiceEscenaDestino = null)
    {
        int escenaAGuardar = indiceEscenaDestino ?? SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("EscenaGuardada", escenaAGuardar);

        if (guardarPosicionExacta && transfJugador != null)
        {
            PlayerPrefs.SetInt("UsarPosicionGuardada", 1);
            PlayerPrefs.SetFloat("JugadorPosX", transfJugador.position.x);
            PlayerPrefs.SetFloat("JugadorPosY", transfJugador.position.y);
            PlayerPrefs.SetFloat("JugadorPosZ", transfJugador.position.z);
        }
        else
        {
            // Al cambiar de nivel, limpiamos la posición para que use el PuntoInicioNivel
            PlayerPrefs.SetInt("UsarPosicionGuardada", 0);
            PlayerPrefs.DeleteKey("JugadorPosX");
            PlayerPrefs.DeleteKey("JugadorPosY");
            PlayerPrefs.DeleteKey("JugadorPosZ");
        }

        if (saludJugador != null)
        {
            PlayerPrefs.SetFloat("VidaJugador", saludJugador.VidaActual);
        }

        if (gestorArmas != null)
        {
            PlayerPrefs.SetInt("TieneCuchillo", gestorArmas.tieneCuchillo ? 1 : 0);
            PlayerPrefs.SetInt("TienePistola", gestorArmas.tienePistola ? 1 : 0);

            if (gestorArmas.armaPistola != null)
            {
                PlayerPrefs.SetInt("MunicionActualPistola", gestorArmas.armaPistola.MunicionActual);
                PlayerPrefs.SetInt("MunicionReservaPistola", gestorArmas.armaPistola.MunicionReserva);
            }
        }

        PlayerPrefs.Save();
    }

    private void CargarEstadoJugador()
    {
        // Restaurar vida exacta (no sumar con Curar)
        if (saludJugador != null)
        {
            float vidaGuardada = PlayerPrefs.GetFloat("VidaJugador", saludJugador.VidaMaxima);
            saludJugador.EstablecerVida(vidaGuardada);
        }

        // Restaurar armas y munición
        if (gestorArmas != null)
        {
            gestorArmas.tieneCuchillo = PlayerPrefs.GetInt("TieneCuchillo", 0) == 1;
            gestorArmas.tienePistola = PlayerPrefs.GetInt("TienePistola", 0) == 1;

            if (gestorArmas.armaPistola != null && PlayerPrefs.HasKey("MunicionActualPistola"))
            {
                int cargador = PlayerPrefs.GetInt("MunicionActualPistola");
                int reserva = PlayerPrefs.GetInt("MunicionReservaPistola");
                gestorArmas.armaPistola.EstablecerMunicion(cargador, reserva);
            }

            if (gestorArmas.tienePistola) gestorArmas.EquiparArma(2);
            else if (gestorArmas.tieneCuchillo) gestorArmas.EquiparArma(1);
        }
    }
}