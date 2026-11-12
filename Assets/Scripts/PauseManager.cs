using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject panelPausaPrincipal; // El menú con los botones (continue, options...)
    [SerializeField] private GameObject panelOpciones;       // El panel de ajustes

    [Header("Controles de Opciones")]
    [SerializeField] private Slider sliderVolumen;
    [SerializeField] private Slider sliderSensibilidad;

    [Header("Configuración de Carga")]
    [SerializeField] private string nombreEscenaMenu = "MainMenuScene";

    [Header("Audio de Efectos de Interfaz (SFX)")]
    [SerializeField] private AudioSource fuenteAudioUI;
    [SerializeField] private AudioClip clipBotonClic;

    public static bool juegoPausado = false;

    private void Start()
    {
        juegoPausado = false;
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        CargarAjustesGuardados();
    }

    private void CargarAjustesGuardados()
    {
        if (sliderVolumen != null)
        {
            sliderVolumen.value = PlayerPrefs.GetFloat("VolumenGlobal", 1f);
            ActualizarVolumen(sliderVolumen.value);
        }

        if (sliderSensibilidad != null)
        {
            sliderSensibilidad.value = PlayerPrefs.GetFloat("Sensibilidad", 1f);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (juegoPausado)
            {
                // Si estás dentro de opciones y presionas ESC, regresa al menú de pausa principal
                if (panelOpciones != null && panelOpciones.activeSelf)
                {
                    MostrarMenuPausa();
                }
                else
                {
                    ReanudarJuego();
                }
            }
            else
            {
                PausarJuego();
            }
        }
    }

    // --- SISTEMA DE OPCIONES ---
    public void ActualizarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("VolumenGlobal", valor);
    }

    public void ActualizarSensibilidad(float valor)
    {
        PlayerPrefs.SetFloat("Sensibilidad", valor);
    }

    // --- NAVEGACIÓN ---
    public void PausarJuego()
    {
        juegoPausado = true;
        MostrarMenuPausa();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReanudarJuego()
    {
        ReproducirSonidoBoton();
        juegoPausado = false;

        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerPrefs.Save();
    }

    public void MostrarMenuPausa()
    {
        ReproducirSonidoBoton();
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void MostrarOpciones()
    {
        ReproducirSonidoBoton();
        if (panelPausaPrincipal != null) panelPausaPrincipal.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        ReproducirSonidoBoton();
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    private void ReproducirSonidoBoton()
    {
        if (fuenteAudioUI != null && clipBotonClic != null)
        {
            // ignorar si el juego no está pausado para que no suene al presionar ESC por primera vez
            if (juegoPausado)
            {
                fuenteAudioUI.PlayOneShot(clipBotonClic);
            }
        }
    }
}