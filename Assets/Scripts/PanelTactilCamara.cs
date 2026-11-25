using UnityEngine;
using UnityEngine.EventSystems;

public class PanelTactilCamara : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float sensibilidadTactil = 0.25f;

    private int dedoActivoId = -1;

    private void Start()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Registrar solo el dedo que toco esta zona derecha
        if (dedoActivoId == -1)
        {
            dedoActivoId = eventData.pointerId;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != dedoActivoId || playerController == null) return;

        // Enviar el desplazamiento del dedo al PlayerController
        playerController.RotarCamaraDesdePantalla(eventData.delta * sensibilidadTactil);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == dedoActivoId)
        {
            dedoActivoId = -1;
        }
    }
}