using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CerebroEnemigoBasico))]
public class enemigoBasicoEstados : MonoBehaviour
{
    public enum EstadoEnemigo { PATRULLA, PERSECUCION, ATAQUE, RECUPERACION }

    [Header("Estado Actual")]
    [SerializeField] private EstadoEnemigo estadoActual = EstadoEnemigo.PATRULLA;

    [Header("Animacion y Audio")]
    [SerializeField] private Animator Animador;
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipAlerta;
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private CarteleraSprite cartelera;

    [Header("Visuales")]
    [SerializeField] private SpriteRenderer renderizaSprites;
    [SerializeField] private Transform transformJugador;

    private CerebroEnemigoBasico cerebro;
    private SaludEnemigo salud;
    private Vector3 posicionAnterior;

    private void Awake()
    {
        cerebro = GetComponent<CerebroEnemigoBasico>();
        cartelera = GetComponentInChildren<CarteleraSprite>();
        salud = GetComponent<SaludEnemigo>();
    }

    private void Update()
    {
        if (salud != null && salud.EstaMuerto) return;

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

        if (cartelera != null)
        {
            switch (estadoActual)
            {
                case EstadoEnemigo.PATRULLA:
                    cartelera.CambiarBrillo(1.0f);
                    break;
                case EstadoEnemigo.PERSECUCION:
                    cartelera.CambiarBrillo(1.4f);
                    break;
                case EstadoEnemigo.ATAQUE:
                    cartelera.CambiarBrillo(2.0f);
                    break;
                case EstadoEnemigo.RECUPERACION:
                    cartelera.CambiarBrillo(0.4f);
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

        Vector3 direccionMovimiento = (transform.position - posicionAnterior).normalized;

        if (Mathf.Abs(direccionMovimiento.x) > Mathf.Abs(direccionMovimiento.z))
        {
            Animador.SetFloat("VelocidadX", Mathf.Abs(direccionMovimiento.x));
            Animador.SetFloat("VelocidadY", 0f);
        }
        else
        {
            Animador.SetFloat("VelocidadX", 0f);
            Animador.SetFloat("VelocidadY", direccionMovimiento.z);
        }

        posicionAnterior = transform.position;
    }
}