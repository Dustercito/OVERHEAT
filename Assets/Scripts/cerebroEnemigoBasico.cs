using System.Collections;
using UnityEngine;

public class CerebroEnemigoBasico : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform posicionJugador;
    [SerializeField] private MeshRenderer rendererEnemigo;
    [SerializeField] private SaludEnemigo saludEnemigo;

    [Header("Parametros de Deteccion y FOV")]
    [SerializeField] private float distanciaDeteccion = 10.0f;
    [SerializeField] private float distanciaProximidad = 2.0f;
    [SerializeField] private float distanciaPerdida = 15.0f;
    [SerializeField][Range(0f, 360f)] private float anguloVision = 90.0f;
    [SerializeField] private LayerMask capasObstaculos;

    [Header("Tiempo de Reaccion (Delay de Atencion)")]
    [SerializeField] private float tiempoReaccion = 0.5f;
    private float temporizadorReaccion = 0.0f;

    [Header("Parametros de Combate e IA")]
    [SerializeField] private float distanciaAtaque = 3.5f;
    [SerializeField] private float velocidadPersecucion = 2.5f;
    [SerializeField] private float danioAtaque = 20.0f; // <-- PARAMETRO EDITABLE DE DAÑO AL JUGADOR

    [Header("Patrullaje")]
    [SerializeField] private Transform[] puntosPatrulla;
    [SerializeField] private float velocidadPatrulla = 1.5f;
    [SerializeField] private float distanciaCambioPunto = 0.5f;
    [SerializeField] private float velocidadRotacion = 5.0f;
    private int indicePuntoActual = 0;

    [Header("Comportamiento Variable de Patrulla")]
    [SerializeField] private float tiempoEsperaMin = 1.0f;
    [SerializeField] private float tiempoEsperaMax = 3.0f;
    [SerializeField][Range(0, 100)] private int probabilidadRegresar = 25;
    [SerializeField][Range(0, 100)] private int probabilidadAleatorio = 30;
    private bool estaEsperandoPunto = false;

    [Header("Tiempos Tutorial")]
    [SerializeField] private float duracionAlerta = 0.8f;
    [SerializeField][Range(0.05f, 1.0f)] private float duracionEmbestida = 0.3f;
    [SerializeField] private float duracionAturdimiento = 2.0f;

    // Propiedades públicas
    public float DistanciaDeteccion => distanciaDeteccion;
    public float DistanciaAtaque => distanciaAtaque;
    public float DuracionAlerta => duracionAlerta;
    public float DuracionAturdimiento => duracionAturdimiento;
    public float DanioAtaque => danioAtaque;

    private void Awake()
    {
        if (saludEnemigo == null)
        {
            saludEnemigo = GetComponent<SaludEnemigo>();
        }
    }

    public bool EstaMuerto()
    {
        return saludEnemigo != null && saludEnemigo.EstaMuerto;
    }

    public bool PuedeVerAlJugador()
    {
        if (posicionJugador == null || EstaMuerto())
        {
            temporizadorReaccion = 0f;
            return false;
        }

        bool jugadorEnVistaOProximidad = false;
        Vector3 direccionHaciaJugador = (posicionJugador.position - transform.position);
        float distancia = direccionHaciaJugador.magnitude;

        if (distancia <= distanciaProximidad)
        {
            jugadorEnVistaOProximidad = true;
        }
        else if (distancia <= distanciaDeteccion)
        {
            direccionHaciaJugador.y = 0;
            float angulo = Vector3.Angle(transform.forward, direccionHaciaJugador);

            if (angulo <= anguloVision / 2f)
            {
                Vector3 origenRayo = transform.position + Vector3.up * 0.5f;
                Vector3 destinoRayo = posicionJugador.position + Vector3.up * 0.5f;
                Vector3 dirRayo = (destinoRayo - origenRayo).normalized;

                if (!Physics.Raycast(origenRayo, dirRayo, distancia, capasObstaculos))
                {
                    jugadorEnVistaOProximidad = true;
                }
            }
        }

        if (jugadorEnVistaOProximidad)
        {
            temporizadorReaccion += Time.deltaTime;

            if (temporizadorReaccion >= tiempoReaccion)
            {
                return true;
            }
        }
        else
        {
            temporizadorReaccion = Mathf.Max(0f, temporizadorReaccion - Time.deltaTime * 2f);
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
        if (puntosPatrulla == null || puntosPatrulla.Length == 0 || estaEsperandoPunto || EstaMuerto()) return;

        Transform puntoObjetivo = puntosPatrulla[indicePuntoActual];
        if (puntoObjetivo == null) return;

        Vector3 direccionPunto = (puntoObjetivo.position - transform.position);
        direccionPunto.y = 0;

        if (direccionPunto != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionPunto.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
        }

        transform.position += transform.forward * velocidadPatrulla * Time.deltaTime;

        Vector3 posEnemigoPlana = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 posPuntoPlana = new Vector3(puntoObjetivo.position.x, 0, puntoObjetivo.position.z);

        if (Vector3.Distance(posEnemigoPlana, posPuntoPlana) <= distanciaCambioPunto)
        {
            StartCoroutine(RutinaDecidirSiguientePunto());
        }
    }

    private IEnumerator RutinaDecidirSiguientePunto()
    {
        estaEsperandoPunto = true;

        float tiempoEspera = Random.Range(tiempoEsperaMin, tiempoEsperaMax);
        yield return new WaitForSeconds(tiempoEspera);

        int azar = Random.Range(0, 100);

        if (azar < probabilidadRegresar)
        {
            indicePuntoActual = (indicePuntoActual - 1 + puntosPatrulla.Length) % puntosPatrulla.Length;
        }
        else if (azar < (probabilidadRegresar + probabilidadAleatorio))
        {
            indicePuntoActual = Random.Range(0, puntosPatrulla.Length);
        }
        else
        {
            indicePuntoActual = (indicePuntoActual + 1) % puntosPatrulla.Length;
        }

        estaEsperandoPunto = false;
    }

    public void MoverHaciaJugador()
    {
        if (posicionJugador == null || EstaMuerto()) return;

        Vector3 direccionEnemigo = (posicionJugador.position - transform.position).normalized;
        direccionEnemigo.y = 0;
        if (direccionEnemigo != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionEnemigo);
        }

        transform.position += transform.forward * velocidadPersecucion * Time.deltaTime;
    }

    public bool EstaDemasiadoLejos()
    {
        if (posicionJugador == null) return true;

        float distancia = Vector3.Distance(transform.position, posicionJugador.position);
        return distancia > distanciaPerdida;
    }

    public IEnumerator RutinaEmbestidaFisica()
    {
        if (posicionJugador == null || EstaMuerto()) yield break;

        Vector3 posicionObjetivoAtacar = posicionJugador.position;
        posicionObjetivoAtacar.y = transform.position.y;

        float temp = 0f;
        Vector3 posInicio = transform.position;

        while (temp < duracionEmbestida)
        {
            float t = temp / duracionEmbestida;
            float curvaSuave = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.position = Vector3.Lerp(posInicio, posicionObjetivoAtacar, curvaSuave);
            temp += Time.deltaTime;
            yield return null;
        }

        // Deteccion de impacto de daño al finalizar el movimiento de la embestida
        float distanciaAlJugador = ObtenerDistanciaAlJugador();
        if (distanciaAlJugador <= 1.8f)
        {
            SaludJugador saludJugador = posicionJugador.GetComponent<SaludJugador>();
            if (saludJugador != null)
            {
                saludJugador.RecibirDanio(danioAtaque);
            }
        }
    }

    public void CambiarColorVisual(Color nuevoColor)
    {
        if (rendererEnemigo != null)
        {
            rendererEnemigo.material.color = nuevoColor;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaProximidad);

        Gizmos.color = Color.yellow;
        Vector3 limiteIzquierdo = DirDesdeAngulo(-anguloVision / 2f, false);
        Vector3 limiteDerecho = DirDesdeAngulo(anguloVision / 2f, false);

        Gizmos.DrawLine(transform.position, transform.position + limiteIzquierdo * distanciaDeteccion);
        Gizmos.DrawLine(transform.position, transform.position + limiteDerecho * distanciaDeteccion);

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