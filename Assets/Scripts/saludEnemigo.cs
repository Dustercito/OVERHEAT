using System.Collections;
using UnityEngine;

public class SaludEnemigo : MonoBehaviour
{
    [Header("Configuracion de Vida")]
    [SerializeField] private float vidaMaxima = 100.0f;
    private float vidaActual;

    [Header("Referencias Visuales y Audio")]
    [SerializeField] private SpriteRenderer renderizaSprites;
    [SerializeField] private CarteleraSprite cartelera;
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipDanio;
    [SerializeField] private AudioClip clipMuerte;

    [Header("Feedback de Impacto")]
    [SerializeField] private Color colorParpadeoImpacto = Color.red;
    [SerializeField] private float duracionParpadeo = 0.15f;
    private Color colorOriginal;

    private Animator animador;
    private CerebroEnemigoBasico cerebroBasico;
    private CerebroJefeTutorial cerebroJefe;
    private enemigoBasicoEstados fsmBasica;
    private TutorialEnemyFSM fsmJefe;
    private bool estaMuerto = false;

    public bool EstaMuerto => estaMuerto;
    public float VidaActual => vidaActual;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        animador = GetComponentInChildren<Animator>();
        cartelera = GetComponentInChildren<CarteleraSprite>();

        // Obtener cerebros
        cerebroBasico = GetComponent<CerebroEnemigoBasico>();
        cerebroJefe = GetComponent<CerebroJefeTutorial>();

        // Obtener máquinas de estado
        fsmBasica = GetComponent<enemigoBasicoEstados>();
        fsmJefe = GetComponent<TutorialEnemyFSM>();

        if (renderizaSprites == null)
        {
            renderizaSprites = GetComponentInChildren<SpriteRenderer>();
        }

        if (renderizaSprites != null)
        {
            colorOriginal = renderizaSprites.color;
        }

        if (fuenteAudio == null)
        {
            Transform objAudio = transform.Find("audioChiquillo");
            if (objAudio != null)
            {
                fuenteAudio = objAudio.GetComponent<AudioSource>();
            }
            else
            {
                fuenteAudio = GetComponentInChildren<AudioSource>();
            }
        }
    }

    public void RecibirDanio(float cantidadDanio)
    {
        if (estaMuerto) return;

        vidaActual -= cantidadDanio;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);

        // Audio de daño
        if (fuenteAudio != null && clipDanio != null)
        {
            fuenteAudio.PlayOneShot(clipDanio);
        }

        // Feedback visual
        StartCoroutine(RutinaFeedbackParpadeo());

        if (animador != null)
        {
            animador.SetTrigger("Danio");
        }

        // --- ALERTA INMEDIATA AL RECIBIR DISPARO ---
        AlertaInmediataPorDisparo();

        if (vidaActual <= 0)
        {
            ProcesarMuerte();
        }
    }

    private void AlertaInmediataPorDisparo()
    {
        // Si el enemigo es de tipo Básico, forzar el cambio de estado a PERSECUCION
        if (fsmBasica != null)
        {
            // Usamos Reflection para cambiar el estado si CambiarEstado es privado
            System.Reflection.MethodInfo metodoCambiarEstado = fsmBasica.GetType().GetMethod("CambiarEstado",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (metodoCambiarEstado != null)
            {
                metodoCambiarEstado.Invoke(fsmBasica, new object[] { enemigoBasicoEstados.EstadoEnemigo.PERSECUCION });
            }
        }
        // Si es el Jefe Tutorial
        else if (fsmJefe != null)
        {
            System.Reflection.MethodInfo metodoCambiarEstado = fsmJefe.GetType().GetMethod("CambiarEstado",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (metodoCambiarEstado != null)
            {
                metodoCambiarEstado.Invoke(fsmJefe, new object[] { TutorialEnemyFSM.EstadoEnemigo.PERSECUCION });
            }
        }
    }

    private IEnumerator RutinaFeedbackParpadeo()
    {
        if (cartelera != null)
        {
            cartelera.AplicarTinteTemp(colorParpadeoImpacto);
            yield return new WaitForSeconds(duracionParpadeo);
            cartelera.RestaurarColorBrillo();
        }
        else if (renderizaSprites != null)
        {
            renderizaSprites.color = colorParpadeoImpacto;
            yield return new WaitForSeconds(duracionParpadeo);
            renderizaSprites.color = colorOriginal;
        }
    }

    private void ProcesarMuerte()
    {
        estaMuerto = true;

        if (fuenteAudio != null && clipMuerte != null)
        {
            fuenteAudio.PlayOneShot(clipMuerte);
        }

        if (cerebroBasico != null) cerebroBasico.enabled = false;
        if (cerebroJefe != null) cerebroJefe.enabled = false;
        if (fsmBasica != null) fsmBasica.enabled = false;
        if (fsmJefe != null) fsmJefe.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (animador != null)
        {
            animador.SetTrigger("Morir");
        }

        Destroy(gameObject, 0.5f);
    }
}