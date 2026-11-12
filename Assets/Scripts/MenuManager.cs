using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;

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
        // Asegurar cursor visible e interactivo en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Iniciar música de fondo
        IniciarMusicaFondo();

        // Mostrar solo el menú principal al inicio
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

    // --- NAVEGACIÓN Y CARGA DE ESCENAS ---
    public void CargarJuego()
    {
        ReproducirSonidoBoton();
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
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // --- EFECTOS DE SONIDO ---
    public void ReproducirSonidoBoton()
    {
        if (fuenteAudioUI != null && clipBotonClic != null)
        {
            fuenteAudioUI.PlayOneShot(clipBotonClic);
        }
    }
}