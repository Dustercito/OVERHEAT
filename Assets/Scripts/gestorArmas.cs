using UnityEngine;

public class GestorArmas : MonoBehaviour
{
    [Header("Referencias a las Armas (Hijos del Player/Camara)")]
    [SerializeField] private ArmaCuchillo armaCuchillo;
    [SerializeField] private ArmaPistola armaPistola;

    [Header("Estado del Inventario (Armas poseidas)")]
    public bool tieneCuchillo = false;
    public bool tienePistola = false;

    // Indicador de arma activa: 0 = Ninguna, 1 = Cuchillo, 2 = Pistola
    private int armaActiva = 0;

    private void Start()
    {
        // Al comenzar, desactivar todas las armas si no se inicia con ninguna
        ActualizarArmaActiva();
    }

    // --- METODO PARA DESBLOQUEAR AL RECOGER DEL MAPA ---
    public void DesbloquearArma(RecolectableArma.TipoArma tipo)
    {
        switch (tipo)
        {
            case RecolectableArma.TipoArma.Cuchillo:
                tieneCuchillo = true;
                EquiparArma(1);
                Debug.Log("¡Recogiste el Cuchillo!");
                break;

            case RecolectableArma.TipoArma.Pistola:
                tienePistola = true;
                EquiparArma(2);
                Debug.Log("¡Recogiste la Pistola!");
                break;
        }
    }

    // --- CAMBIAR Y MOSTRAR ARMA ACTIVA ---
    public void EquiparArma(int indiceArma)
    {
        if (indiceArma == 1 && tieneCuchillo) armaActiva = 1;
        else if (indiceArma == 2 && tienePistola) armaActiva = 2;

        ActualizarArmaActiva();
    }

    private void ActualizarArmaActiva()
    {
        if (armaCuchillo != null)
            armaCuchillo.gameObject.SetActive(armaActiva == 1);

        if (armaPistola != null)
            armaPistola.gameObject.SetActive(armaActiva == 2);
    }

    // --- ATAQUE / DISPARO SEGUN ARMA EQUIPADA ---
    public void EjecutarAtaque()
    {
        if (armaActiva == 1 && armaCuchillo != null)
        {
            armaCuchillo.Atacar();
        }
        else if (armaActiva == 2 && armaPistola != null)
        {
            armaPistola.Disparar();
        }
    }

    // --- CANALIZACION DE RECARGA PARA LA PISTOLA ---
    public void EjecutarRecarga()
    {
        if (armaActiva == 2 && armaPistola != null)
        {
            armaPistola.Recargar();
        }
    }
}