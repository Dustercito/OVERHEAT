using System.Collections;
using UnityEngine;

public class CerebroJefeTutorial : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform posicionJugador;
    [SerializeField] private MeshRenderer rendererEnemigo;

    [Header("Parametros de Deteccion y FOV")]
    [SerializeField] private float distanciaDeteccion = 10.0f;
    [SerializeField] private float distanciaProximidad = 2.0f; // Radio donde escucha/siente al jugador
    [SerializeField][Range(0f, 360f)] private float anguloVision = 90.0f;
    [SerializeField] private LayerMask capasObstaculos;

    [Header("Parametros de Combate e IA")]
    [SerializeField] private float distanciaAtaque = 3.5f;
    [SerializeField] private float velocidadPersecucion = 2.5f;

    [Header("Patrullaje")]
    [SerializeField] private Transform[] puntosPatrulla;
    [SerializeField] private float velocidadPatrulla = 1.5f;
    [SerializeField] private float distanciaCambioPunto = 0.5f;
    private int indicePuntoActual = 0;

    [Header("Tiempos Tutorial")]
    [SerializeField] private float duracionAlerta = 0.8f;
    [SerializeField][Range(0.05f, 1.0f)] private float duracionEmbestida = 0.3f;
    [SerializeField] private float duracionAturdimiento = 2.0f;

    // Propiedades publicas
    public float DistanciaDeteccion => distanciaDeteccion;
    public float DistanciaAtaque => distanciaAtaque;
    public float DuracionAlerta => duracionAlerta;
    public float DuracionAturdimiento => duracionAturdimiento;

    // --- DETECCION MEJORADA: CONO DE VISION + RADIO DE PROXIMIDAD ---
    public bool PuedeVerAlJugador()
    {
        if (posicionJugador == null) return false;

        Vector3 direccionHaciaJugador = (posicionJugador.position - transform.position);
        float distancia = direccionHaciaJugador.magnitude;

        // 1. CONDICION DE PROXIMIDAD (Escucha al jugador si se acerca demasiado por la espalda)
        if (distancia <= distanciaProximidad)
        {
            return true;
        }

        // Si esta fuera del rango de deteccion lejano, no hace nada
        if (distancia > distanciaDeteccion) return false;

        // 2. CONDICION DE VISION (Cono de vision + Raycast)
        direccionHaciaJugador.y = 0;
        float angulo = Vector3.Angle(transform.forward, direccionHaciaJugador);

        if (angulo <= anguloVision / 2f)
        {
            Vector3 origenRayo = transform.position + Vector3.up * 0.5f;
            Vector3 destinoRayo = posicionJugador.position + Vector3.up * 0.5f;
            Vector3 dirRayo = (destinoRayo - origenRayo).normalized;

            if (!Physics.Raycast(origenRayo, dirRayo, distancia, capasObstaculos))
            {
                return true; // Jugador visto de frente
            }
        }

        return false;
    }

    public float ObtenerDistanciaAlJugador()
    {
        if (posicionJugador == null) return float.MaxValue;
        return Vector3.Distance(transform.position, posicionJugador.position);
    }

    public void MoverEnPatrulla()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;

        Transform puntoObjetivo = puntosPatrulla[indicePuntoActual];
        if (puntoObjetivo == null) return;

        Vector3 direccionPunto = (puntoObjetivo.position - transform.position);
        direccionPunto.y = 0;

        if (direccionPunto != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionPunto.normalized);
        }

        transform.position += transform.forward * velocidadPatrulla * Time.deltaTime;

        Vector3 posEnemigoPlana = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 posPuntoPlana = new Vector3(puntoObjetivo.position.x, 0, puntoObjetivo.position.z);

        if (Vector3.Distance(posEnemigoPlana, posPuntoPlana) <= distanciaCambioPunto)
        {
            indicePuntoActual = (indicePuntoActual + 1) % puntosPatrulla.Length;
        }
    }

    public void MoverHaciaJugador()
    {
        if (posicionJugador == null) return;

        Vector3 direccionEnemigo = (posicionJugador.position - transform.position).normalized;
        direccionEnemigo.y = 0;
        if (direccionEnemigo != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionEnemigo);
        }

        transform.position += transform.forward * velocidadPersecucion * Time.deltaTime;
    }

    public IEnumerator RutinaEmbestidaFisica()
    {
        if (posicionJugador == null) yield break;

        Vector3 posicionObjetivoAtacar = posicionJugador.position;
        float temp = 0f;
        Vector3 posInicio = transform.position;

        while (temp < duracionEmbestida)
        {
            transform.position = Vector3.Lerp(posInicio, posicionObjetivoAtacar, temp / duracionEmbestida);
            temp += Time.deltaTime;
            yield return null;
        }
    }

    public void CambiarColorVisual(Color nuevoColor)
    {
        if (rendererEnemigo != null)
        {
            rendererEnemigo.material.color = nuevoColor;
        }
    }

    // Dibujar en el editor (Gizmos)
    private void OnDrawGizmos()
    {
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        // Radio de proximidad (Escucha)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaProximidad);

        // Lineas del cono de vision FOV
        Gizmos.color = Color.yellow;
        Vector3 limiteIzquierdo = DirDesdeAngulo(-anguloVision / 2f, false);
        Vector3 limiteDerecho = DirDesdeAngulo(anguloVision / 2f, false);

        Gizmos.DrawLine(transform.position, transform.position + limiteIzquierdo * distanciaDeteccion);
        Gizmos.DrawLine(transform.position, transform.position + limiteDerecho * distanciaDeteccion);

        // Puntos de patrulla
        if (puntosPatrulla == null || puntosPatrulla.Length == 0) return;

        for (int i = 0; i < puntosPatrulla.Length; i++)
        {
            if (puntosPatrulla[i] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(puntosPatrulla[i].position, 0.4f);

                int siguienteIndice = (i + 1) % puntosPatrulla.Length;
                if (puntosPatrulla[siguienteIndice] != null)
                {
                    Gizmos.DrawLine(puntosPatrulla[i].position, puntosPatrulla[siguienteIndice].position);
                }
            }
        }
    }

    private Vector3 DirDesdeAngulo(float anguloEnGrados, bool anguloEsGlobal)
    {
        if (!anguloEsGlobal)
        {
            anguloEnGrados += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(anguloEnGrados * Mathf.Deg2Rad), 0, Mathf.Cos(anguloEnGrados * Mathf.Deg2Rad));
    }
}