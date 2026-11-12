using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necesario para controlar los Sliders

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;

    [Header("Controles de Opciones")]
    [SerializeField] private Slider sliderVolumen;
    [SerializeField] private Slider sliderSensibilidad;

    [Header("Configuración de Carga")]
    [Tooltip("Nombre exacto de la escena de juego")]
    [SerializeField] private string nombreEscenaJuego = "SampleScene";

    [Header("Audio de Fondo (Música)")]
    [SerializeField] private AudioSource fuenteAudioMusicaFondo;
    [SerializeField] private AudioClip clipMusicaFondo;

    [Header("Audio de Efectos de Interfaz (SFX)")]
    [SerializeField] private AudioSource fuenteAudioUI;
    [SerializeField] private AudioClip clipBotonClic;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        IniciarMusicaFondo();
        CargarAjustesGuardados(); // Cargar volumen y sensibilidad al abrir el juego
        MostrarMenuPrincipal();
    }

    // --- SISTEMA DE OPCIONES ---
    private void CargarAjustesGuardados()
    {
        // Carga el volumen guardado (1 por defecto si es la primera vez)
        if (sliderVolumen != null)
        {
            sliderVolumen.value = PlayerPrefs.GetFloat("VolumenGlobal", 1f);
            ActualizarVolumen(sliderVolumen.value);
        }

        // Carga la sensibilidad guardada (1 por defecto)
        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = PlayerPrefs.GetFloat("Sensibilidad", 1f);
        }
    }

    public void ActualizarVolumen(float valor)
    {
        // AudioListener.volume controla el volumen maestro de todo Unity
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("VolumenGlobal", valor);
    }

    public void ActualizarSensibilidad(float valor)
    {
        // Solo lo guardamos, el PlayerController lo leerá al cargar la partida
        PlayerPrefs.SetFloat("Sensibilidad", valor);
    }

    // --- MÚSICA Y NAVEGACIÓN ---
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

    public void CargarJuego()
    {
        ReproducirSonidoBoton();
        PlayerPrefs.Save(); // Asegura guardar las opciones antes de jugar
        SceneManager.LoadScene(nombreEscenaJuego);
    }

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
        PlayerPrefs.Save();
        Application.Quit();
    }

    public void ReproducirSonidoBoton()
    {
        if (fuenteAudioUI != null && clipBotonClic != null)
        {
            fuenteAudioUI.PlayOneShot(clipBotonClic);
        }
    }

    public void ContinuarJuego()
    {
        ReproducirSonidoBoton();
        // Lee el índice de la escena guardada (si no hay, carga la escena 1 por defecto)
        int escenaAGuardar = PlayerPrefs.GetInt("EscenaGuardada", 1);
        SceneManager.LoadScene(escenaAGuardar);
    }
}