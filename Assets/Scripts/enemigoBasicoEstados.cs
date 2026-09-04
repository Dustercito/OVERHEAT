using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CerebroEnemigoBasico))]
public class enemigoBasicoEstados : MonoBehaviour
{
    public enum EstadoEnemigo { PATRULLA, PERSECUCION, ATAQUE, RECUPERACION }

    [Header("Estado Actual")]
    [SerializeField] private EstadoEnemigo estadoActual = EstadoEnemigo.PATRULLA;

    [Header("Animación y Audio")]
    [SerializeField] private Animator Animador;
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipAlerta;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private CarteleraSprite cartelera;

    [Header("Visuales")]
    [SerializeField] private SpriteRenderer renderizaSprites;
    [SerializeField] private Transform transformJugador;

    private CerebroEnemigoBasico cerebro;
    private Vector3 posicionAnterior;

    private void Awake()
    {
        cerebro = GetComponent<CerebroEnemigoBasico>();
        cartelera = GetComponentInChildren<CarteleraSprite>();
    }
    

    private void Update()
    {
        // 1. AHORA SÍ: Esta línea enviará las velocidades al Animator en todo momento
        EnviarVelocidadAlAnimador();

        switch (estadoActual)
        {
            case EstadoEnemigo.PATRULLA:
                ProcesarPatrulla();
                break;

            case EstadoEnemigo.PERSECUCION:
                VoltearHaciaJugador();
                ProcesarPersecucion();
                break;

            case EstadoEnemigo.ATAQUE:
                break;

            case EstadoEnemigo.RECUPERACION:
                break;
        }
    }

    private void ProcesarPatrulla()
    {
        cerebro.MoverEnPatrulla();

        if (cerebro.PuedeVerAlJugador())
        {
            VoltearHaciaJugador();

            if (fuenteAudio != null && clipAlerta != null)
            {
                fuenteAudio.PlayOneShot(clipAlerta);
            }
            CambiarEstado(EstadoEnemigo.PERSECUCION);
        }
    }

    private void ProcesarPersecucion()
    {
        if (cerebro.EstaDemasiadoLejos())
        {
            CambiarEstado(EstadoEnemigo.PATRULLA);
            return;
        }

        float dist = cerebro.ObtenerDistanciaAlJugador();
        cerebro.MoverHaciaJugador();

        if (dist <= cerebro.DistanciaAtaque)
        {
            StartCoroutine(RutinaCicloAtaque());
        }
    }

    private IEnumerator RutinaCicloAtaque()
    {
        CambiarEstado(EstadoEnemigo.ATAQUE);

        if (Animador != null) Animador.SetTrigger("Anticipar");
        yield return new WaitForSeconds(cerebro.DuracionAlerta);

        if (Animador != null) Animador.SetTrigger("Atacar");
        if (fuenteAudio != null && clipAtaque != null) fuenteAudio.PlayOneShot(clipAtaque);

        yield return new WaitForSeconds(cerebro.DuracionAlerta);

        yield return StartCoroutine(cerebro.RutinaEmbestidaFisica());

        CambiarEstado(EstadoEnemigo.RECUPERACION);

        if (Animador != null) Animador.SetTrigger("Recuperar");

        yield return new WaitForSeconds(cerebro.DuracionAturdimiento);

        CambiarEstado(EstadoEnemigo.PERSECUCION);
    }

  private void CambiarEstado(EstadoEnemigo nuevoEstado)
    {
        estadoActual = nuevoEstado;

        // Intensidad de brillo por estado
        if (cartelera != null)
        {
            switch (estadoActual)
            {
                case EstadoEnemigo.PATRULLA:
                    cartelera.CambiarBrillo(1.0f); // Brillo normal (100%)
                    break;
                case EstadoEnemigo.PERSECUCION:
                    cartelera.CambiarBrillo(1.4f); // Más brillante (alerta/activo)
                    break;
                case EstadoEnemigo.ATAQUE:
                    cartelera.CambiarBrillo(2.0f); // Destello máximo de ataque
                    break;
                case EstadoEnemigo.RECUPERACION:
                    cartelera.CambiarBrillo(0.4f); // Opaco/oscurecido (aturdido)
                    break;
            }
        }

        if (Animador != null)
        {
            bool estaMoviendose = (estadoActual == EstadoEnemigo.PERSECUCION || estadoActual == EstadoEnemigo.PATRULLA);
            Animador.SetBool("Corriendo", estaMoviendose);
        }
    }

    private void VoltearHaciaJugador()
    {
        if (transformJugador == null || renderizaSprites == null) return;

        if (transformJugador.position.x > transform.position.x)
        {
            renderizaSprites.flipX = false; 
        }
        else if (transformJugador.position.x < transform.position.x)
        {
            renderizaSprites.flipX = true; 
        }
    }

private void EnviarVelocidadAlAnimador()
{
    if (Animador == null) return;

    // 1. Calculamos hacia dónde se movió REALMENTE en este fotograma
    Vector3 direccionMovimiento = (transform.position - posicionAnterior).normalized;

    // 2. Comparamos qué movimiento es más fuerte para no confundir al Animator
    if (Mathf.Abs(direccionMovimiento.x) > Mathf.Abs(direccionMovimiento.z))
    {
        // Se mueve más de lado
        Animador.SetFloat("VelocidadX", Mathf.Abs(direccionMovimiento.x));
        Animador.SetFloat("VelocidadY", 0f);
    }
    else
    {
        // Se mueve más hacia el fondo o el frente
        Animador.SetFloat("VelocidadX", 0f);
        Animador.SetFloat("VelocidadY", direccionMovimiento.z); // Cambia la 'z' por 'y' si es 2D puro
    }

    // 3. Guardamos la posición actual para el siguiente fotograma
    posicionAnterior = transform.position;
}
}