using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TriggerCambioNivel : MonoBehaviour
{
    [Header("Destino de Escena")]
    [Tooltip("Nombre exacto de la escena a cargar (Level_2, Level_3 o MainMenuScene)")]
    [SerializeField] private string nombreSiguienteEscena = "Level_2";

    [Tooltip("Marcar solo en la puerta final del Nivel 3 para reiniciar progreso al terminar")]
    [SerializeField] private bool esFinalDelJuego = false;

    [Header("Secuencia de Videos (Puerta -> Cinemática)")]
    [Tooltip("Video 1: Animación de la puerta abriéndose")]
    [SerializeField] private VideoClip videoAbrirPuerta;

    [Tooltip("Video 2: Cinemática de transición al siguiente nivel (o final)")]
    [SerializeField] private VideoClip videoCinematicaNivel;

    [Header("Referencias de UI y Reproductor")]
    [Tooltip("Panel contenedor con el Fondo Negro y el RawImage del video")]
    [SerializeField] private GameObject contenedorCinematica;
    [SerializeField] private VideoPlayer reproductorVideo;

    [Tooltip("Canvas del HUD (vida, armas, controles táctiles) para ocultarlo")]
    [SerializeField] private GameObject hudJugador;

    private bool yaActivado = false;
    private Queue<VideoClip> colaVideos = new Queue<VideoClip>();

    private void Start()
    {
        if (contenedorCinematica != null)
        {
            contenedorCinematica.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!yaActivado && (other.GetComponent<PlayerController>() != null || other.CompareTag("Player")))
        {
            yaActivado = true;
            IniciarSecuenciaVideos();
        }
    }

    private void IniciarSecuenciaVideos()
    {
        // 1. Ocultar el HUD inmediatamente
        if (hudJugador != null)
        {
            hudJugador.SetActive(false);
        }

        // 2. Guardar progreso o limpiar partida si es el cierre del Nivel 3
        GestionarGuardadoTransicion();

        // 3. Congelar enemigos y físicas para que no ataquen durante los videos
        Time.timeScale = 0f;

        // 4. Encolar los videos en orden: primero puerta, luego cinemática
        colaVideos.Clear();
        if (videoAbrirPuerta != null) colaVideos.Enqueue(videoAbrirPuerta);
        if (videoCinematicaNivel != null) colaVideos.Enqueue(videoCinematicaNivel);

        if (contenedorCinematica != null && reproductorVideo != null && colaVideos.Count > 0)
        {
            contenedorCinematica.SetActive(true);
            reproductorVideo.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
            reproductorVideo.isLooping = false;

            reproductorVideo.loopPointReached += AlTerminarVideoActual;
            ReproducirSiguienteEnCola();
        }
        else
        {
            CargarSiguienteEscena();
        }
    }

    private void ReproducirSiguienteEnCola()
    {
        if (colaVideos.Count > 0)
        {
            VideoClip siguienteClip = colaVideos.Dequeue();
            reproductorVideo.Stop();
            reproductorVideo.clip = siguienteClip;
            reproductorVideo.frame = 0;
            reproductorVideo.Play();
        }
        else
        {
            // Se terminaron ambos videos
            reproductorVideo.loopPointReached -= AlTerminarVideoActual;
            CargarSiguienteEscena();
        }
    }

    private void AlTerminarVideoActual(VideoPlayer vp)
    {
        ReproducirSiguienteEnCola();
    }

    private void CargarSiguienteEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreSiguienteEscena);
    }

    private void GestionarGuardadoTransicion()
    {
        if (esFinalDelJuego)
        {
            PlayerPrefs.DeleteKey("EscenaGuardada");
            PlayerPrefs.DeleteKey("UsarPosicionGuardada");
            PlayerPrefs.DeleteKey("VidaJugador");
            PlayerPrefs.DeleteKey("TieneCuchillo");
            PlayerPrefs.DeleteKey("TienePistola");
            PlayerPrefs.DeleteKey("MunicionActualPistola");
            PlayerPrefs.DeleteKey("MunicionReservaPistola");
            PlayerPrefs.DeleteKey("JugadorPosX");
            PlayerPrefs.DeleteKey("JugadorPosY");
            PlayerPrefs.DeleteKey("JugadorPosZ");
            PlayerPrefs.Save();
        }
        else
        {
            int indiceSiguiente = SceneManager.GetActiveScene().buildIndex + 1;
            SaveManager saveManager = FindFirstObjectByType<SaveManager>();
            if (saveManager != null)
            {
                // false = NO usar posición guardada en el nuevo nivel, usar su PuntoInicioNivel
                saveManager.EjecutarGuardadoFisico(guardarPosicionExacta: false, indiceEscenaDestino: indiceSiguiente);
            }
            else
            {
                PlayerPrefs.SetInt("EscenaGuardada", indiceSiguiente);
                PlayerPrefs.SetInt("UsarPosicionGuardada", 0);
                PlayerPrefs.Save();
            }
        }
    }
}