using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    public enum TipoItem { Cuchillo, Pistola, Escopeta, MunicionPistola }

    [Header("Configuracion del Recogible")]
    [SerializeField] private TipoItem tipoDeItem;
    [Tooltip("Solo aplica si el tipo es MunicionPistola")]
    [SerializeField] private int cantidadMunicion = 12;
    [SerializeField] private AudioClip sonidoRecogida;

    [Header("Efecto Visual (Flotacion)")]
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
        // Giro y flotación para que llame la atención en el mapa
        transform.Rotate(Vector3.up * velRotacion * Time.deltaTime, Space.World);
        float nuevoY = posInicial.y + Mathf.Sin(Time.time * velFlotacion) * altFlotacion;
        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detectar si lo tocó el jugador
        GestorArmas gestor = other.GetComponent<GestorArmas>();
        if (gestor != null)
        {
            bool recogidoExitosamente = gestor.RecogerItem(tipoDeItem, cantidadMunicion);

            if (recogidoExitosamente)
            {
                if (sonidoRecogida != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRecogida, transform.position);
                }
                Destroy(gameObject);
            }
        }
    }
}