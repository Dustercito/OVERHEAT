using UnityEngine;
using UnityEngine.UI;

public class GestorRadiacion : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SaludJugador saludJugador;
    [SerializeField] private Image imagenDestelloRadiacion; // Panel naranja en tu Canvas
    [SerializeField] private GameObject cartelRadiacion;     // Cartel de aviso en tu Canvas

    [Header("Audio de Radiación")]
    [SerializeField] private AudioSource fuenteAudioRadiacion;
    [SerializeField] private AudioClip clipRadiacion;

    [Header("Ajustes Visuales y de Daño")]
    [SerializeField] private float danioPorSegundo = 15.0f;
    [SerializeField] private float intervaloTick = 0.25f;
    [SerializeField][Range(0f, 1f)] private float opacidadMaximaDestello = 0.35f;
    [SerializeField] private float velocidadTransicionVisual = 6.0f;

    private int lucesTocandoJugador = 0;
    private float temporizadorDanio = 0f;
    private float opacidadActual = 0f;

    private void Awake()
    {
        if (saludJugador == null)
        {
            saludJugador = GetComponent<SaludJugador>();
        }

        if (fuenteAudioRadiacion == null)
        {
            fuenteAudioRadiacion = gameObject.AddComponent<AudioSource>();
        }
        fuenteAudioRadiacion.loop = true;
        fuenteAudioRadiacion.playOnAwake = false;
        fuenteAudioRadiacion.clip = clipRadiacion;

        if (imagenDestelloRadiacion != null)
        {
            Color c = imagenDestelloRadiacion.color;
            c.a = 0f;
            imagenDestelloRadiacion.color = c;
            imagenDestelloRadiacion.raycastTarget = false;
        }

        if (cartelRadiacion != null)
        {
            cartelRadiacion.SetActive(false);
        }
    }

    public void ReportarExposicion(bool estaEnLuz)
    {
        if (estaEnLuz)
        {
            lucesTocandoJugador++;
        }
    }

    private void LateUpdate()
    {
        bool bajoRadiacion = lucesTocandoJugador > 0;

        // 1. Aplicar daño periódico
        if (bajoRadiacion && saludJugador != null)
        {
            temporizadorDanio += Time.deltaTime;
            if (temporizadorDanio >= intervaloTick)
            {
                saludJugador.RecibirDanio(danioPorSegundo * intervaloTick);
                temporizadorDanio = 0f;
            }
        }
        else
        {
            temporizadorDanio = 0f;
        }

        // 2. Controlar cartel de alerta en pantalla
        if (cartelRadiacion != null && cartelRadiacion.activeSelf != bajoRadiacion)
        {
            cartelRadiacion.SetActive(bajoRadiacion);
        }

        // 3. Controlar sonido en bucle
        if (fuenteAudioRadiacion != null && clipRadiacion != null)
        {
            if (bajoRadiacion && !fuenteAudioRadiacion.isPlaying)
            {
                fuenteAudioRadiacion.Play();
            }
            else if (!bajoRadiacion && fuenteAudioRadiacion.isPlaying)
            {
                fuenteAudioRadiacion.Stop();
            }
        }

        // 4. Controlar destello naranja suave en pantalla
        if (imagenDestelloRadiacion != null)
        {
            float opacidadObjetivo = bajoRadiacion ? opacidadMaximaDestello : 0f;
            opacidadActual = Mathf.Lerp(opacidadActual, opacidadObjetivo, Time.deltaTime * velocidadTransicionVisual);

            Color c = imagenDestelloRadiacion.color;
            c.a = opacidadActual;
            imagenDestelloRadiacion.color = c;
        }

        lucesTocandoJugador = 0;
    }
}