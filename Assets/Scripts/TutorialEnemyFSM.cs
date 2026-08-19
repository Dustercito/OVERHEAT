using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CerebroJefeTutorial))]
public class TutorialEnemyFSM : MonoBehaviour
{
    public enum EstadoEnemigo { PATRULLA, PERSECUCION, ATAQUE, RECUPERACION }

    [Header("Estado Actual")]
    [SerializeField] private EstadoEnemigo estadoActual = EstadoEnemigo.PATRULLA;

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
            CambiarEstado(EstadoEnemigo.PERSECUCION);
        }
    }

    private void ProcesarPersecucion()
    {
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

        yield return new WaitForSeconds(cerebro.DuracionAlerta);

        yield return StartCoroutine(cerebro.RutinaEmbestidaFisica());

        CambiarEstado(EstadoEnemigo.RECUPERACION);
        cerebro.CambiarColorVisual(Color.blue);

        yield return new WaitForSeconds(cerebro.DuracionAturdimiento);

        cerebro.CambiarColorVisual(Color.grey);

        CambiarEstado(EstadoEnemigo.PERSECUCION);
    }

    private void CambiarEstado(EstadoEnemigo nuevoEstado)
    {
        estadoActual = nuevoEstado;
    }
}