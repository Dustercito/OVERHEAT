using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Necesario para controlar el VideoPlayer por código

public class TriggerCambioNivel : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("El nombre exacto de la escena del Nivel 2")]
    [SerializeField] private string nombreSiguienteNivel = "Level_2";

    [Tooltip("El tiempo en segundos que dura tu animación de la puerta")]
    [SerializeField] private float duracionCinematica = 2.0f;

    [Header("Referencias Visuales")]
    [Tooltip("El objeto que contiene el componente Video Player")]
    [SerializeField] private GameObject objetoVideoPlayer;

    [Tooltip("El Canvas del HUD para ocultarlo")]
    [SerializeField] private GameObject hudJugador;

    private bool yaActivado = false;
    private VideoPlayer reproductorVideo;

    private void Start()
    {
        // Obtener la referencia al reproductor de video
        if (objetoVideoPlayer != null)
        {
            reproductorVideo = objetoVideoPlayer.GetComponent<VideoPlayer>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si el que tocó la zona invisible fue el jugador
        if (!yaActivado && other.GetComponent<PlayerController>() != null)
        {
            yaActivado = true;
            StartCoroutine(RutinaCinematicaYCambio());
        }
    }

    private IEnumerator RutinaCinematicaYCambio()
    {
        // 1. Ocultar el HUD
        if (hudJugador != null)
        {
            hudJugador.SetActive(false);
        }

        // 2. Activar el objeto que contiene el video
        if (objetoVideoPlayer != null)
        {
            objetoVideoPlayer.SetActive(true);
        }

        // 3. Forzar el inicio de la reproducción desde el inicio (frame 0)
        if (reproductorVideo != null)
        {
            reproductorVideo.frame = 0;
            reproductorVideo.Play();
        }

        // 4. Esperar a que la animación termine. (Opcionalmente podrías usar un while basado en reproductorVideo.isPlaying)
        yield return new WaitForSeconds(duracionCinematica);

        // 5. Cargar el Nivel 2
        SceneManager.LoadScene(nombreSiguienteNivel);
    }
}