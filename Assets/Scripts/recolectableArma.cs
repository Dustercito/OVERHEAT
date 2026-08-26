using UnityEngine;

public class RecolectableArma : MonoBehaviour
{
    public enum TipoArma { Cuchillo, Pistola, Escopeta }

    [Header("Configuracion de Recogida")]
    [SerializeField] private TipoArma armaAEntregar;
    [SerializeField] private AudioClip sonidoRecogida;

    [Header("Efecto Visual (Flotacion 2D en 3D)")]
    [SerializeField] private float velRotacion = 50.0f;
    [SerializeField] private float velFlotacion = 2.0f;
    [SerializeField] private float altFlotacion = 0.1f;

    private Vector3 posInicial;

    private void Start()
    {
        posInicial = transform.position;
    }

    private void Update()
    {
        // Giro horizontal constante del Sprite/Objeto
        transform.Rotate(Vector3.up * velRotacion * Time.deltaTime, Space.World);

        // Movimiento suave arriba y abajo (Efecto flotante clasico)
        float nuevoY = posInicial.y + Mathf.Sin(Time.time * velFlotacion) * altFlotacion;
        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si el Jugador entra al Trigger
        GestorArmas gestor = other.GetComponent<GestorArmas>();
        if (gestor != null)
        {
            // Entregar el arma correspondiente
            gestor.DesbloquearArma(armaAEntregar);

            // Sonido de recogida (opcional)
            if (sonidoRecogida != null)
            {
                AudioSource.PlayClipAtPoint(sonidoRecogida, transform.position);
            }

            // Eliminar el recogible del escenario
            Destroy(gameObject);
        }
    }
}