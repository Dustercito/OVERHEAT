using UnityEngine;
using UnityEngine.UI;

public class ControladorVidaSpriteUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SaludJugador saludJugador;
    [SerializeField] private Image imagenRellenoVida; // Arrastra aqui Relleno_Vida

    [Header("Suavizado Visual")]
    [SerializeField] private float velocidadSuavizado = 10f;

    private float porcentajeVisualActual = 1f;

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

        if (saludJugador != null && imagenRellenoVida != null)
        {
            porcentajeVisualActual = Mathf.Clamp01(saludJugador.VidaActual / saludJugador.VidaMaxima);
            imagenRellenoVida.fillAmount = porcentajeVisualActual;
        }
    }

    private void Update()
    {
        if (saludJugador == null || imagenRellenoVida == null) return;

        float porcentajeObjetivo = Mathf.Clamp01(saludJugador.VidaActual / saludJugador.VidaMaxima);
        porcentajeVisualActual = Mathf.Lerp(porcentajeVisualActual, porcentajeObjetivo, Time.deltaTime * velocidadSuavizado);

        imagenRellenoVida.fillAmount = porcentajeVisualActual;
    }
}