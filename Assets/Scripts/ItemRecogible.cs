using System.Collections;
using UnityEngine;

public class ItemRecogible : MonoBehaviour
{
    public enum TipoItem { Cuchillo, Pistola, MunicionPistola, Salud }

    [Header("Configuracion del Recogible")]
    [SerializeField] private TipoItem tipoDeItem;
    [Tooltip("Cantidad de balas (MunicionPistola) o puntos de vida (Salud)")]
    [SerializeField] private int cantidad = 12;
    [SerializeField] private AudioClip sonidoRecogida;

    [Header("Animacion por Sprites (8 Frames)")]
    [SerializeField] private SpriteRenderer renderizadorSprite;
    [Tooltip("Arrastra aqui los 8 sprites recortados en orden")]
    [SerializeField] private Sprite[] spritesAnimacion;
    [SerializeField] private float duracionPorFrame = 0.12f;

    [Header("Efecto Visual (Flotacion Vertical)")]
    [SerializeField] private float velFlotacion = 2.0f;
    [SerializeField] private float altFlotacion = 0.08f;

    private Vector3 posInicial;

    private void Start()
    {
        posInicial = transform.position;

        if (renderizadorSprite == null)
        {
            renderizadorSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (spritesAnimacion != null && spritesAnimacion.Length > 0)
        {
            StartCoroutine(RutinaAnimacionSprites());
        }
    }

    private void Update()
    {
        // Solo flotar arriba/abajo (no rotamos el transform porque CarteleraSprite mira a la camara)
        float nuevoY = posInicial.y + Mathf.Sin(Time.time * velFlotacion) * altFlotacion;
        transform.position = new Vector3(transform.position.x, nuevoY, transform.position.z);
    }

    private IEnumerator RutinaAnimacionSprites()
    {
        int indice = 0;
        while (true)
        {
            if (renderizadorSprite != null && spritesAnimacion[indice] != null)
            {
                renderizadorSprite.sprite = spritesAnimacion[indice];
            }

            indice = (indice + 1) % spritesAnimacion.Length;
            yield return new WaitForSeconds(duracionPorFrame);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool recogidoExitosamente = false;

        if (tipoDeItem == TipoItem.Salud)
        {
            SaludJugador salud = other.GetComponent<SaludJugador>();
            if (salud == null) salud = other.GetComponentInParent<SaludJugador>();

            if (salud != null && !salud.EstaMuerto && salud.VidaActual < salud.VidaMaxima)
            {
                salud.Curar(cantidad);
                recogidoExitosamente = true;
            }
        }
        else
        {
            GestorArmas gestor = other.GetComponent<GestorArmas>();
            if (gestor == null) gestor = other.GetComponentInParent<GestorArmas>();

            if (gestor != null)
            {
                recogidoExitosamente = gestor.RecogerItem(tipoDeItem, cantidad);
            }
        }

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