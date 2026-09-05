using UnityEngine;

public class GeneradorPatrullaRandom : MonoBehaviour
{
    [Header("Configuración del Área")]
    [SerializeField] private int cantidadPuntos = 4;
    [SerializeField] private float radioPatrulla = 8.0f;
    [SerializeField] private LayerMask capasObstaculos;

    [Header("Visualización Editor")]
    [SerializeField] private bool mostrarGizmos = true;

    private void Awake()
    {
        GenerarPuntosEnArea();
    }

    public void GenerarPuntosEnArea()
    {
        // 1. Crear un contenedor para no desordenar la jerarquía
        GameObject contenedorPuntos = new GameObject("PuntosPatrulla_Generados");
        contenedorPuntos.transform.position = transform.position;

        Transform[] nuevosPuntos = new Transform[cantidadPuntos];

        for (int i = 0; i < cantidadPuntos; i++)
        {
            // Generar un punto aleatorio dentro del radio circular (plano XZ)
            Vector2 puntoRandom2D = Random.insideUnitCircle * radioPatrulla;
            Vector3 posicionPunto = transform.position + new Vector3(puntoRandom2D.x, 0f, puntoRandom2D.y);

            // Ajustar altura al suelo mediante un Raycast hacia abajo
            if (Physics.Raycast(posicionPunto + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, ~capasObstaculos))
            {
                posicionPunto.y = hit.point.y + 0.1f;
            }

            // Crear el Transform para el Waypoint
            GameObject puntoObj = new GameObject($"Punto_{i + 1}");
            puntoObj.transform.position = posicionPunto;
            puntoObj.transform.parent = contenedorPuntos.transform;

            nuevosPuntos[i] = puntoObj.transform;
        }

        // 2. Asignar la nueva lista de puntos automáticamente al Cerebro de la IA
        CerebroJefeTutorial cerebroJefe = GetComponent<CerebroJefeTutorial>();
        if (cerebroJefe != null)
        {
            // Inyectamos la lista al campo puntosPatrulla por Reflection si es privado
            AsignarPuntosAlCerebro(cerebroJefe, nuevosPuntos);
            return;
        }

        CerebroEnemigoBasico cerebroBasico = GetComponent<CerebroEnemigoBasico>();
        if (cerebroBasico != null)
        {
            AsignarPuntosAlCerebro(cerebroBasico, nuevosPuntos);
        }
    }

    private void AsignarPuntosAlCerebro(object cerebro, Transform[] puntos)
    {
        System.Reflection.FieldInfo campoPuntos = cerebro.GetType().GetField("puntosPatrulla",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        if (campoPuntos != null)
        {
            campoPuntos.SetValue(cerebro, puntos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos) return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radioPatrulla);
    }
}