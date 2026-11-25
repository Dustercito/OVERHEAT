using UnityEngine;

public class GeneradorPatrullaRandom : MonoBehaviour
{
    [Header("Configuracion del Area")]
    [SerializeField] private int cantidadPuntos = 4;
    [SerializeField] private float radioPatrulla = 8.0f;
    [SerializeField] private LayerMask capasObstaculos;
    [SerializeField] private float margenPared = 0.6f; // Distancia minima respecto a muros

    [Header("Visualizacion Editor")]
    [SerializeField] private bool mostrarGizmos = true;

    private void Awake()
    {
        GenerarPuntosEnArea();
    }

    public void GenerarPuntosEnArea()
    {
        // 1. Crear un contenedor para no desordenar la jerarquia
        GameObject contenedorPuntos = new GameObject("PuntosPatrulla_Generados");
        contenedorPuntos.transform.position = transform.position;

        Transform[] nuevosPuntos = new Transform[cantidadPuntos];
        Vector3 origenOjos = transform.position + Vector3.up * 0.5f;

        for (int i = 0; i < cantidadPuntos; i++)
        {
            Vector3 posicionValida = transform.position;

            // Intentar hasta 15 veces encontrar un punto que no atraviese paredes ni quede pegado a un muro
            for (int intento = 0; intento < 15; intento++)
            {
                Vector2 puntoRandom2D = Random.insideUnitCircle * radioPatrulla;
                Vector3 candidato = transform.position + new Vector3(puntoRandom2D.x, 0f, puntoRandom2D.y);

                // Ajustar altura al suelo mediante un Raycast hacia abajo
                if (Physics.Raycast(candidato + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, ~capasObstaculos))
                {
                    candidato.y = hit.point.y + 0.1f;
                }

                Vector3 destinoOjos = candidato + Vector3.up * 0.5f;

                // Verificar que no haya pared entre el enemigo y el punto, y que no quede incrustado en un muro
                bool bloqueadoPorPared = Physics.Linecast(origenOjos, destinoOjos, capasObstaculos);
                bool muyCercaDePared = Physics.CheckSphere(destinoOjos, margenPared, capasObstaculos);

                if (!bloqueadoPorPared && !muyCercaDePared)
                {
                    posicionValida = candidato;
                    break;
                }
            }

            // Crear el Transform para el Waypoint
            GameObject puntoObj = new GameObject($"Punto_{i + 1}");
            puntoObj.transform.position = posicionValida;
            puntoObj.transform.parent = contenedorPuntos.transform;

            nuevosPuntos[i] = puntoObj.transform;
        }

        // 2. Asignar la nueva lista de puntos automaticamente al Cerebro de la IA
        CerebroJefeTutorial cerebroJefe = GetComponent<CerebroJefeTutorial>();
        if (cerebroJefe != null)
        {
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