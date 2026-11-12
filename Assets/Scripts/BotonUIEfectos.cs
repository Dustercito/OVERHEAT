using UnityEngine;
using UnityEngine.EventSystems;

public class BotonUIEfectos : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Indicadores de Selección (Flechas)")]
    [Tooltip("Objeto contenedor o grupo de las flechas asociadas a este botón")]
    [SerializeField] private GameObject flechasSeleccion;

    [Header("Audio Al Pasar el Mouse")]
    [SerializeField] private AudioSource fuenteAudioUI;
    [SerializeField] private AudioClip clipHoverMouse;

    private void OnDisable()
    {
        // Ocultar las flechas si el botón o su panel se desactiva
        OcultarFlechas();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MostrarFlechas();
        ReproducirSonidoHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OcultarFlechas();
    }

    private void MostrarFlechas()
    {
        if (flechasSeleccion != null)
        {
            flechasSeleccion.SetActive(true);
        }
    }

    private void OcultarFlechas()
    {
        if (flechasSeleccion != null)
        {
            flechasSeleccion.SetActive(false);
        }
    }

    private void ReproducirSonidoHover()
    {
        if (fuenteAudioUI != null && clipHoverMouse != null)
        {
            fuenteAudioUI.PlayOneShot(clipHoverMouse);
        }
    }
}