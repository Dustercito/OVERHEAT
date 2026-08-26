using System.Collections;
using UnityEngine;

public class ArmaCuchillo : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACION ---
    [Header("Configuracion de Ataque")]
    [SerializeField] private float danio = 25.0f;
    [SerializeField] private float alcanceAtaque = 2.0f;
    [SerializeField] private float recargaAtaque = 0.6f;
    [SerializeField] private LayerMask capasEnemigo;

    [Header("Referencias Visuales (Sprite 2D)")]
    [SerializeField] private SpriteRenderer renderizadorSprite;
    [SerializeField] private Sprite spriteReposo;
    [SerializeField] private Sprite spriteAtaque;

    [Header("Referencias de Audio")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip sonidoAtaque;

    [Header("Camara")]
    [SerializeField] private Transform camaraJugador;

    // --- VARIABLES INTERNAS ---
    private bool puedeAtacar = true;

    private void Start()
    {
        // Asegurar que el sprite inicial sea el de reposo
        if (renderizadorSprite != null && spriteReposo != null)
        {
            renderizadorSprite.sprite = spriteReposo;
        }
    }

    // --- ACCION DE ATAQUE PÚBLICA ---
    public void Atacar()
    {
        if (puedeAtacar)
        {
            StartCoroutine(RutinaAtaque());
        }
    }

    // --- CORRUTINA DE IMPACTO, ANIMACION SPRITE Y COOLDOWN ---
    private IEnumerator RutinaAtaque()
    {
        puedeAtacar = false;

        // 1. Cambiar visualmente al Sprite de ataque
        if (renderizadorSprite != null && spriteAtaque != null)
        {
            renderizadorSprite.sprite = spriteAtaque;
        }

        // 2. Reproducir sonido si esta asignado
        if (fuenteAudio != null && sonidoAtaque != null)
        {
            fuenteAudio.PlayOneShot(sonidoAtaque);
        }

        // 3. Lanzar Raycast de impacto desde el centro de la camara (vista FPS)
        if (camaraJugador != null)
        {
            RaycastHit hit;
            Vector3 origen = camaraJugador.position;
            Vector3 direccion = camaraJugador.forward;

            if (Physics.Raycast(origen, direccion, out hit, alcanceAtaque, capasEnemigo))
            {
                // Detectar el componente cerebro de la IA enemiga
                CerebroJefeTutorial cerebroEnemigo = hit.collider.GetComponent<CerebroJefeTutorial>();
                if (cerebroEnemigo != null)
                {
                    Debug.Log("Impacto con cuchillo a: " + hit.collider.name);
                }
            }
        }

        // 4. Mostrar el frame de ataque durante un momento breve
        yield return new WaitForSeconds(0.2f);

        // 5. Regresar al Sprite de reposo
        if (renderizadorSprite != null && spriteReposo != null)
        {
            renderizadorSprite.sprite = spriteReposo;
        }

        // 6. Esperar el resto del tiempo de recarga
        float tiempoRestante = recargaAtaque - 0.2f;
        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

        puedeAtacar = true;
    }

    // --- DIBUJAR ALCANCE EN SCENE (GIZMOS) ---
    private void OnDrawGizmosSelected()
    {
        if (camaraJugador != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(camaraJugador.position, camaraJugador.forward * alcanceAtaque);
        }
    }
}