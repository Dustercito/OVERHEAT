using System.Collections;
using UnityEngine;

public class ArmaPistola : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACION ---
    [Header("Configuracion de Disparo")]
    [SerializeField] private float danio = 40.0f;
    [SerializeField] private float alcanceDisparo = 25.0f;
    [SerializeField] private float recargaDisparo = 0.4f;
    [SerializeField] private LayerMask capasEnemigo;

    [Header("Referencias Visuales (Sprite 2D)")]
    [SerializeField] private SpriteRenderer renderizadorSprite;
    [SerializeField] private Sprite spriteReposo;
    [SerializeField] private Sprite spriteDisparo;

    [Header("Audio")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip sonidoDisparo;

    [Header("Camara")]
    [SerializeField] private Transform camaraJugador;

    private bool puedeDisparar = true;

    private void OnEnable()
    {
        if (renderizadorSprite != null && spriteReposo != null)
        {
            renderizadorSprite.sprite = spriteReposo;
        }
    }

    public void Disparar()
    {
        if (puedeDisparar)
        {
            StartCoroutine(RutinaDisparo());
        }
    }

    private IEnumerator RutinaDisparo()
    {
        puedeDisparar = false;

        // 1. Mostrar sprite de disparo
        if (renderizadorSprite != null && spriteDisparo != null)
        {
            renderizadorSprite.sprite = spriteDisparo;
        }

        // 2. Audio
        if (fuenteAudio != null && sonidoDisparo != null)
        {
            fuenteAudio.PlayOneShot(sonidoDisparo);
        }

        // 3. Impacto Raycast
        if (camaraJugador != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(camaraJugador.position, camaraJugador.forward, out hit, alcanceDisparo, capasEnemigo))
            {
                CerebroJefeTutorial cerebroEnemigo = hit.collider.GetComponent<CerebroJefeTutorial>();
                if (cerebroEnemigo != null)
                {
                    Debug.Log("Disparo de pistola asestado a: " + hit.collider.name);
                }
            }
        }

        yield return new WaitForSeconds(0.15f);

        // 4. Volver a sprite reposo
        if (renderizadorSprite != null && spriteReposo != null)
        {
            renderizadorSprite.sprite = spriteReposo;
        }

        float tiempoRestante = recargaDisparo - 0.15f;
        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

        puedeDisparar = true;
    }
}