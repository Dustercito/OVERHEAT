using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CerebroJefeTutorial))]
public class TutorialEnemyFSM : MonoBehaviour
{
    public enum EstadoEnemigo { PATRULLA, PERSECUCION, ATAQUE, RECUPERACION }

    [Header("Estado Actual")]
    [SerializeField] private EstadoEnemigo estadoActual = EstadoEnemigo.PATRULLA;

    [Header("Animación y Audio")]
    [SerializeField] private Animator Animador;
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip clipAlerta;
    [SerializeField] private AudioClip clipAtaque;

    [Header("Visuales")]
    [SerializeField] private SpriteRenderer renderizaSprites;
    [SerializeField] private Transform transformJugador;

    private CerebroJefeTutorial cerebro;

    private void Awake()
    {
        cerebro = GetComponent<CerebroJefeTutorial>();
    }

    private void Update()
    {
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

        // Ahora solo persigue si el jugador entra en su cono de vision y no hay obstaculos
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
        // 1. Si el jugador se alejó demasiado, vuelve a Patrulla
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

        cerebro.CambiarColorVisual(Color.red);

        if (Animador != null) Animador.SetTrigger("Anticipar");
        yield return new WaitForSeconds(cerebro.DuracionAlerta);

        if (Animador != null) Animador.SetTrigger("Atacar");
        if (fuenteAudio != null && clipAtaque != null) fuenteAudio.PlayOneShot(clipAtaque);

        yield return new WaitForSeconds(cerebro.DuracionAlerta);

        yield return StartCoroutine(cerebro.RutinaEmbestidaFisica());

        CambiarEstado(EstadoEnemigo.RECUPERACION);
        cerebro.CambiarColorVisual(Color.blue);

        if (Animador != null) Animador.SetTrigger("Recuperar");

        yield return new WaitForSeconds(cerebro.DuracionAturdimiento);

        cerebro.CambiarColorVisual(Color.grey);

        CambiarEstado(EstadoEnemigo.PERSECUCION);
    }

    private void CambiarEstado(EstadoEnemigo nuevoEstado)
    {
        estadoActual = nuevoEstado;

        if (Animador != null)
        {
            Animador.SetBool("Corriendo", estadoActual == EstadoEnemigo.PERSECUCION);
        }
    }

    private void VoltearHaciaJugador()
    {
        if (transformJugador == null || renderizaSprites == null) return;

        // Si el jugador está a la derecha del monstruo
        if (transformJugador.position.x > transform.position.x)
        {
            renderizaSprites.flipX = false; // Cambia esto a 'true' si el dibujo original mira a la izquierda
        }
        // Si el jugador está a la izquierda
        else if (transformJugador.position.x < transform.position.x)
        {
            renderizaSprites.flipX = true; // Cambia esto a 'false' si tu dibujo original mira a la izquierda
        }
    }
}