using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Necesario para ColorAdjustments en URP
using UnityEngine.SceneManagement;

public class SaludJugador : MonoBehaviour
{
    [Header("Configuracion de Vida")]
    [SerializeField] private float vidaMaxima = 100.0f;
    private float vidaActual;

    [Header("Efectos de Poca Vida")]
    [SerializeField][Range(0.1f, 0.5f)] private float porcentajePocaVida = 0.35f;
    [SerializeField] private AudioSource fuenteAudioRespiracion;
    [SerializeField] private AudioClip clipRespiracion;
    [SerializeField] private Volume volumenPostProcesado; // Global Volume

    [Header("Efectos y Feedback")]
    [SerializeField] private AudioSource fuenteAudioEfectos;
    [SerializeField] private AudioClip clipDanio;
    [SerializeField] private AudioClip clipMuerte;

    [Header("Tiempo de Invulnerabilidad (I-Frames)")]
    [SerializeField] private float tiempoInvulnerable = 0.8f;
    private bool esInvulnerable = false;
    private bool estaMuerto = false;

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

        ObtenerColorAdjustments();
    }

    private void ObtenerColorAdjustments()
    {
        if (volumenPostProcesado != null && volumenPostProcesado.profile != null)
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
            // 1. Audio de respiración
            if (fuenteAudioRespiracion != null && !fuenteAudioRespiracion.isPlaying && clipRespiracion != null)
            {
                fuenteAudioRespiracion.Play();
            }

            // Factor crítico (0.0 al entrar al umbral -> 1.0 cuando la vida llega a 0)
            float factorCritico = 1.0f - (vidaActual / umbralPocaVida);

            // 2. Temblor de cámara
            if (controlJugador != null)
            {
                controlJugador.AjustarIntensidadShakeVida(factorCritico);
            }

            // 3. Forzar Blanco y Negro absoluto (-100) + Oscurecido (-0.8)
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.overrideState = true;
                colorAdjustments.saturation.value = Mathf.Lerp(0f, -100f, factorCritico);

                colorAdjustments.postExposure.overrideState = true;
                colorAdjustments.postExposure.value = Mathf.Lerp(0f, -0.8f, factorCritico);
            }
        }
        else
        {
            // Restablecer estado normal
            if (fuenteAudioRespiracion != null && fuenteAudioRespiracion.isPlaying)
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

    private void ProcesarMuerte()
    {
        estaMuerto = true;

        if (fuenteAudioRespiracion != null && fuenteAudioRespiracion.isPlaying)
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

        Invoke(nameof(ReiniciarNivel), 1.5f);
    }

    private void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}