using UnityEngine;
using TMPro;

public class ControladorMunicionUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ArmaPistola armaPistola;
    [SerializeField] private TextMeshProUGUI textoMunicion;

    [Header("Colores de Aviso")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSinBalas = Color.red;

    private void Start()
    {
        if (armaPistola == null)
        {
            // Busca la pistola aunque inicie desactivada en la jerarquia
            armaPistola = FindFirstObjectByType<ArmaPistola>(FindObjectsInactive.Include);
        }

        if (textoMunicion == null)
        {
            textoMunicion = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (armaPistola == null || textoMunicion == null) return;

        // Mostrar el contador solo cuando la pistola este equipada y activa
        bool pistolaEquipada = armaPistola.gameObject.activeInHierarchy;

        if (textoMunicion.gameObject.activeSelf != pistolaEquipada)
        {
            textoMunicion.gameObject.SetActive(pistolaEquipada);
        }

        if (!pistolaEquipada) return;

        // Actualizar formato "Cargador / Reserva" (ej. 12 / 36)
        int cargador = armaPistola.MunicionActual;
        int reserva = armaPistola.MunicionReserva;

        textoMunicion.text = cargador + " / " + reserva;
        textoMunicion.color = (cargador == 0 && reserva == 0) ? colorSinBalas : colorNormal;
    }
}