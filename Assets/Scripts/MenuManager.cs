using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;

    [Header("Cinemática de Contexto (Solo Nuevo Juego)")]
    [SerializeField] private GameObject contenedorVideoIntro;
    [SerializeField] private VideoPlayer reproductorVideoIntro;
    [SerializeField] private float duracionVideoIntro = 10.0f; // Ajusta a los segundos exactos de tu video

    [Header("Configuración de Carga")]
    [Tooltip("Nombre exacto de la escena del Nivel 1")]
    [SerializeField] private string nombreEscenaJuego = "Level_1";

    [Header("Audio de Fondo (Música)")]
    [SerializeField] private AudioSource fuenteAudioMusicaFondo;
    [SerializeField] private AudioClip clipMusicaFondo;

    [Header("Audio de Efectos de Interfaz (SFX)")]
    [SerializeField] private AudioSource fuenteAudioUI;
    [SerializeField] private AudioClip clipBotonClic;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (contenedorVideoIntro != null)
        {
            contenedorVideoIntro.SetActive(false);
        }

        IniciarMusicaFondo();
        MostrarMenuPrincipal();
    }

    private void IniciarMusicaFondo()
    {
        if (fuenteAudioMusicaFondo != null && clipMusicaFondo != null)
        {
            fuenteAudioMusicaFondo.clip = clipMusicaFondo;
            fuenteAudioMusicaFondo.loop = true;
            fuenteAudioMusicaFondo.playOnAwake = false;
            fuenteAudioMusicaFondo.Play();
        }
    }

    // --- BOTÓN: NUEVO JUEGO (Con Video de Contexto) ---
    public void NuevoJuego()
    {
        ReproducirSonidoBoton();

        // 1. Limpiar progreso anterior manteniendo Volumen y Sensibilidad
        PlayerPrefs.DeleteKey("VidaJugador");
        PlayerPrefs.DeleteKey("EscenaGuardada");
        PlayerPrefs.DeleteKey("TieneCuchillo");
        PlayerPrefs.DeleteKey("TienePistola");
        PlayerPrefs.DeleteKey("JugadorPosX");
        PlayerPrefs.DeleteKey("JugadorPosY");
        PlayerPrefs.DeleteKey("JugadorPosZ");
        PlayerPrefs.Save();

        // 2. Iniciar rutina del video antes de cargar el Nivel 1
        StartCoroutine(ReproducirIntroYCargar());
    }

    private IEnumerator ReproducirIntroYCargar()
    {
        // Apagar música del menú para que no se encime con el audio del video
        if (fuenteAudioMusicaFondo != null)
        {
            fuenteAudioMusicaFondo.Stop();
        }

        // Ocultar paneles del menú y mostrar pantalla negra + video
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        if (contenedorVideoIntro != null && reproductorVideoIntro != null)
        {
            contenedorVideoIntro.SetActive(true);
            reproductorVideoIntro.frame = 0;
            reproductorVideoIntro.Play();

            // Esperar la duración exacta del video
            yield return new WaitForSeconds(duracionVideoIntro);
        }

        // Cargar el Nivel 1 desde cero
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // --- BOTÓN: CONTINUAR JUEGO (Directo a la partida, sin video) ---
    public void ContinuarJuego()
    {
        ReproducirSonidoBoton();

        int escenaGuardada = PlayerPrefs.GetInt("EscenaGuardada", 1);
        SceneManager.LoadScene(escenaGuardada);
    }

    // --- NAVEGACIÓN DE PANELES ---
    public void MostrarMenuPrincipal()
    {
        ReproducirSonidoBoton();
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void MostrarOpciones()
    {
        ReproducirSonidoBoton();
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void SalirDelJuego()
    {
        ReproducirSonidoBoton();
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void ReproducirSonidoBoton()
    {
        if (fuenteAudioUI != null && clipBotonClic != null)
        {
            fuenteAudioUI.PlayOneShot(clipBotonClic);
        }
    }
}