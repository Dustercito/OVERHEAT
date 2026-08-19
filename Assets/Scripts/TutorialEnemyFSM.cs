using System.Collections;
using UnityEngine;

public class TutorialEnemyFSM : MonoBehaviour
{
    // Estados de la Maquina de Estados Finita (FSM)
    public enum EstadoEnemigo { ESPERA, PERSECUCION, ATAQUE, RECUPERACION }

    [Header("Referencias")]
    [SerializeField] private Transform posicionJugador;
    [SerializeField] private MeshRenderer rendererEnemigo;

    [Header("Parametros de IA")]
    [SerializeField] private float distanciaDeteccion = 10.0f;
    [SerializeField] private float distanciaAtaque = 3.5f;
    [SerializeField] private float velocidadPersecucion = 2.5f;

    [Header("Tiempos Tutorial")]
    [SerializeField] private float duracionAlerta = 0.8f;       // Tiempo de alerta (rojo)
    [SerializeField][Range(0.05f, 1.0f)] private float duracionEmbestida = 0.3f; // Tiempo de embestida (menor = mas rapido)
    [SerializeField] private float duracionAturdimiento = 2.0f;     // Tiempo de descanso/aturdimiento (azul)

    private EstadoEnemigo estadoActual = EstadoEnemigo.ESPERA;
    private Vector3 posicionObjetivoAtacar;

    private void Update()
    {
        // Maquina de estados principal
        switch (estadoActual)
        {
            case EstadoEnemigo.ESPERA:
                ProcesarEspera();
                break;

            case EstadoEnemigo.PERSECUCION:
                ProcesarPersecucion();
                break;

            case EstadoEnemigo.ATAQUE:
                // Se procesa mediante corrutina
                break;

            case EstadoEnemigo.RECUPERACION:
                // Se procesa mediante corrutina
                break;
        }
    }

    private void ProcesarEspera()
    {
        if (posicionJugador == null) return;

        // Detectar si el jugador esta dentro del rango de alerta
        float distAlJugador = Vector3.Distance(transform.position, posicionJugador.position);
        if (distAlJugador <= distanciaDeteccion)
        {
            estadoActual = EstadoEnemigo.PERSECUCION;
        }
    }

    private void ProcesarPersecucion()
    {
        if (posicionJugador == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, posicionJugador.position);

        // Orientar enemigo hacia el jugador en el eje horizontal (Y)
        Vector3 direccionEnemigo = (posicionJugador.position - transform.position).normalized;
        direccionEnemigo.y = 0;
        transform.rotation = Quaternion.LookRotation(direccionEnemigo);

        // Avanzar hacia el jugador
        transform.position += transform.forward * velocidadPersecucion * Time.deltaTime;

        // Cambiar a estado de ataque al entrar en rango
        if (distanciaAlJugador <= distanciaAtaque)
        {
            StartCoroutine(RutinaEmbestida());
        }
    }

    private IEnumerator RutinaEmbestida()
    {
        estadoActual = EstadoEnemigo.ATAQUE;

        // Feedback visual: Rojo al empezar ataque
        if (rendererEnemigo != null) rendererEnemigo.material.color = Color.red;

        // Guardar la posicion del jugador antes de embestir
        posicionObjetivoAtacar = posicionJugador.position;
        yield return new WaitForSeconds(duracionAlerta);

        // Embestida y duracion
        float temp = 0f;
        Vector3 posInicio = transform.position;

        while (temp < duracionEmbestida)
        {
            transform.position = Vector3.Lerp(posInicio, posicionObjetivoAtacar, temp / duracionEmbestida);
            temp += Time.deltaTime;
            yield return null;
        }

        // Estado de recuperacion: Azul al quedar aturdido
        estadoActual = EstadoEnemigo.RECUPERACION;
        if (rendererEnemigo != null) rendererEnemigo.material.color = Color.blue;

        yield return new WaitForSeconds(duracionAturdimiento);

        // Restaurar estado original y perseguir nuevamente
        if (rendererEnemigo != null) rendererEnemigo.material.color = Color.grey;
        estadoActual = EstadoEnemigo.PERSECUCION;
    }

    // Dibujar rangos en el editor (Gizmos), esta increible :D
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}