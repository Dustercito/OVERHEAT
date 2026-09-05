using System.Collections;
using UnityEngine;

public class ArmaCuchillo : MonoBehaviour
{
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

    private bool puedeAtacar = true;

    private void Start()
    {
        if (renderizadorSprite != null && spriteReposo != null)
        {
            renderizadorSprite.sprite = spriteReposo;
        }
    }

    public void Atacar()
    {
        if (puedeAtacar)
        {
            StartCoroutine(RutinaAtaque());
        }
    }

    private IEnumerator RutinaAtaque()
    {
        puedeAtacar = false;

        if (renderizadorSprite != null && spriteAtaque != null)
        {
            renderizadorSprite.sprite = spriteAtaque;
        }

        if (fuenteAudio != null && sonidoAtaque != null)
        {
            fuenteAudio.PlayOneShot(sonidoAtaque);
        }

        if (camaraJugador != null)
        {
            RaycastHit hit;
            Vector3 origen = camaraJugador.position;
            Vector3 direccion = camaraJugador.forward;

            if (Physics.Raycast(origen, direccion, out hit, alcanceAtaque, capasEnemigo))
            {
                SaludEnemigo salud = hit.collider.GetComponent<SaludEnemigo>();
                if (salud == null) salud = hit.collider.GetComponentInParent<SaludEnemigo>();

                if (salud != null)
                {
                    salud.RecibirDanio(danio);
                }
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (renderizadorSprite != null && spriteReposo != null)
        {
            renderizadorSprite.sprite = spriteReposo;
        }

        float tiempoRestante = recargaAtaque - 0.2f;
        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

        puedeAtacar = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (camaraJugador != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(camaraJugador.position, camaraJugador.forward * alcanceAtaque);
        }
    }
}