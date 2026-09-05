using UnityEngine;
using UnityEngine.UI;

public class ControladorVidaSpriteUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SaludJugador saludJugador;
    [SerializeField] private Image imagenBarraVida;

    [Header("Secuencia de Sprites de Vida")]
    [Tooltip("Ordena los sprites desde el más lleno (índice 0) hasta el más vacío (último índice)")]
    [SerializeField] private Sprite[] spritesBarraVida;

    private void Start()
    {
        if (saludJugador == null)
        {
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                saludJugador = jugador.GetComponent<SaludJugador>();
            }
        }
    }

    private void Update()
    {
        if (saludJugador == null || imagenBarraVida == null || spritesBarraVida == null || spritesBarraVida.Length == 0)
            return;

        ActualizarSpriteVida();
    }

    private void ActualizarSpriteVida()
    {
        // 1. Calcular el porcentaje actual de salud (rango de 0.0 a 1.0)
        float porcentajeVida = Mathf.Clamp01(saludJugador.VidaActual / saludJugador.VidaMaxima);

        // 2. Mapear el porcentaje al rango de índices del arreglo de sprites
        int totalSprites = spritesBarraVida.Length;

        // Convertimos el porcentaje inverso a un índice (100% vida = índice 0, 0% vida = último índice)
        int indiceSprite = Mathf.FloorToInt((1.0f - porcentajeVida) * totalSprites);

        // Clampear el índice para evitar salir del rango del arreglo
        indiceSprite = Mathf.Clamp(indiceSprite, 0, totalSprites - 1);

        // 3. Asignar el sprite correspondiente a la imagen UI
        if (spritesBarraVida[indiceSprite] != null)
        {
            imagenBarraVida.sprite = spritesBarraVida[indiceSprite];
        }
    }
}