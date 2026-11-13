
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SaludJugador : MonoBehaviour
{
    [Header("Configuracion de Vida")]
    [SerializeField] private float vidaMaxima = 100.0f;
    private float vidaActual;

    [Header("Efectos de Poca Vida")]
    [SerializeField][Range(0.1f, 0.5f)] private float porcentajePocaVida = 0.35f;
    [SerializeField] private AudioSource fuenteAudioRespiracion;
    [SerializeField] private AudioClip clipRespiracion;
    [SerializeField] private Volume volumenPostProcesado;

    [Header("Efectos y Feedback")]
    [SerializeField] private AudioSource fuenteAudioEfectos;
    [SerializeField] private AudioClip clipDanio;
    [SerializeField] private AudioClip clipMuerte;

    [Header("Tiempo de Invulnerabilidad (I-Frames)")]
    [SerializeField] private float tiempoInvulnerable = 0.8f;
    private bool esInvulnerable = false;
    private bool estaMuerto = false;

    [Header("Cinematica de Muerte")]
    [SerializeField] private GameObject objetoVideoPlayer;
    [SerializeField] private float duracionCinematica = 2.5f;
    [SerializeField] private GameObject hudJugador;
    [SerializeField] private GameObject panelGameOver;

    [Header("Configuracion del Menu")]
    [SerializeField] private string nombreMenuPrincipal = "MainMenu";

    private VideoPlayer reproductorVideo;
    private PlayerController controlJugador;
    private ColorAdjustments colorAdjustments;

    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;
    public bool EstaMuerto => estaMuerto;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        controlJugador = GetComponent<PlayerController>();

        if (fuenteAudioEfectos == null)
        {
            fuenteAudioEfectos = GetComponent<AudioSource>();
        }

        if (fuenteAudioRespiracion == null)
        {
            fuenteAudioRespiracion = gameObject.AddComponent<AudioSource>();
        }

        if (fuenteAudioRespiracion != null)
        {
            fuenteAudioRespiracion.playOnAwake = false;
            fuenteAudioRespiracion.loop = true;

            if (clipRespiracion != null)
            {
                fuenteAudioRespiracion.clip = clipRespiracion;
            }
        }

        // Obtener Video Player
        if (objetoVideoPlayer != null)
        {
            reproductorVideo =
                objetoVideoPlayer.GetComponent<VideoPlayer>();
        }

        ObtenerColorAdjustments();
    }

    private void Start()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
    }

    private void ObtenerColorAdjustments()
    {
        if (volumenPostProcesado != null &&
            volumenPostProcesado.profile != null)
        {
            volumenPostProcesado.profile.TryGet(out colorAdjustments);
        }
    }

    private void Update()
    {
        ActualizarEfectosPocaVida();
    }

    public void RecibirDanio(float cantidadDanio)
    {
        if (estaMuerto || esInvulnerable) return;

        vidaActual -= cantidadDanio;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        if (fuenteAudioEfectos != null && clipDanio != null)
        {
            fuenteAudioEfectos.PlayOneShot(clipDanio);
        }

        StartCoroutine(RutinaInvulnerabilidad());

        if (vidaActual <= 0)
        {
            ProcesarMuerte();
        }
    }

    private void ActualizarEfectosPocaVida()
    {
        if (estaMuerto) return;

        float umbralPocaVida = vidaMaxima * porcentajePocaVida;

        if (vidaActual <= umbralPocaVida && vidaActual > 0)
        {
            if (fuenteAudioRespiracion != null &&
                !fuenteAudioRespiracion.isPlaying &&
                clipRespiracion != null)
            {
                fuenteAudioRespiracion.Play();
            }

            float factorCritico =
                1.0f - (vidaActual / umbralPocaVida);

            if (controlJugador != null)
            {
                controlJugador.AjustarIntensidadShakeVida(factorCritico);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.overrideState = true;
                colorAdjustments.saturation.value =
                    Mathf.Lerp(0f, -100f, factorCritico);

                colorAdjustments.postExposure.overrideState = true;
                colorAdjustments.postExposure.value =
                    Mathf.Lerp(0f, -0.8f, factorCritico);
            }
        }
        else
        {
            if (fuenteAudioRespiracion != null &&
                fuenteAudioRespiracion.isPlaying)
            {
                fuenteAudioRespiracion.Stop();
            }

            if (controlJugador != null)
            {
                controlJugador.AjustarIntensidadShakeVida(0f);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = 0f;
                colorAdjustments.postExposure.value = 0f;
            }
        }
    }

    private IEnumerator RutinaInvulnerabilidad()
    {
        esInvulnerable = true;

        yield return new WaitForSeconds(tiempoInvulnerable);

        esInvulnerable = false;
    }

    public void Curar(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
    }

    // ==========================================
    // SISTEMA DE MUERTE DEL JUGADOR
    // ==========================================

    private void ProcesarMuerte()
    {
        if (estaMuerto) return;

        estaMuerto = true;

        if (fuenteAudioRespiracion != null)
        {
            fuenteAudioRespiracion.Stop();
        }

        if (controlJugador != null)
        {
            controlJugador.AjustarIntensidadShakeVida(0f);
            controlJugador.enabled = false;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = -100f;
        }

        if (fuenteAudioEfectos != null && clipMuerte != null)
        {
            fuenteAudioEfectos.PlayOneShot(clipMuerte);
        }

        StartCoroutine(RutinaMuerte());
    }


    private IEnumerator RutinaMuerte()
    {
        // 1. Pausar el tiempo del juego
        Time.timeScale = 0f;

        // 2. Ocultar HUD
        if (hudJugador != null)
        {
            hudJugador.SetActive(false);
        }

        // 3. Activar video
        if (objetoVideoPlayer != null)
        {
            objetoVideoPlayer.SetActive(true);
        }

        // 4. Reproducir cinematica
        if (reproductorVideo != null)
        {
            reproductorVideo.frame = 0;
            reproductorVideo.Play();
        }

        // 5. Esperar a que termine la cinematica
        yield return new WaitForSecondsRealtime(duracionCinematica);

        // 6. Desactivar video
        if (objetoVideoPlayer != null)
        {
            objetoVideoPlayer.SetActive(false);
        }

        // 7. Mostrar panel Game Over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        // 8. Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    // ==========================================
    // BOTON: REINTENTAR
    // ==========================================

    public void Reintentar()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // ==========================================
    // BOTON: MENU PRINCIPAL
    // ==========================================

    public void IrMenuPrincipal()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(nombreMenuPrincipal);
    }
}
