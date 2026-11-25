using UnityEngine;

[RequireComponent(typeof(Light))]
public class LuzRadiactiva : MonoBehaviour
{
    [Header("Configuración del Haz")]
    [SerializeField] private LayerMask capasParedes;
    [SerializeField][Range(0.5f, 1.2f)] private float multiplicadorRadio = 0.9f;

    private Light focoLuz;
    private Transform jugadorTransform;
    private GestorRadiacion gestorRadiacion;

    private void Awake()
    {
        focoLuz = GetComponent<Light>();
    }

    private void Start()
    {
        gestorRadiacion = FindFirstObjectByType<GestorRadiacion>();
        if (gestorRadiacion != null)
        {
            jugadorTransform = gestorRadiacion.transform;
        }
    }

    private void Update()
    {
        if (!focoLuz.enabled || jugadorTransform == null || gestorRadiacion == null) return;

        if (JugadorEstaEnLaLuz())
        {
            gestorRadiacion.ReportarExposicion(true);
        }
    }

    private bool JugadorEstaEnLaLuz()
    {
        Vector3 puntoJugador = jugadorTransform.position + Vector3.up * 0.9f;
        Vector3 vectorHaciaJugador = puntoJugador - transform.position;
        float distancia = vectorHaciaJugador.magnitude;

        if (distancia > focoLuz.range) return false;

        float angulo = Vector3.Angle(transform.forward, vectorHaciaJugador.normalized);
        float limiteAngulo = (focoLuz.spotAngle * 0.5f) * multiplicadorRadio;
        if (angulo > limiteAngulo) return false;

        if (Physics.Raycast(transform.position, vectorHaciaJugador.normalized, distancia, capasParedes))
        {
            return false;
        }

        return true;
    }
}