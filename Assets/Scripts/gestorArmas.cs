using UnityEngine;

public class GestorArmas : MonoBehaviour
{
    [Header("Referencias a las Armas")]
    public ArmaCuchillo armaCuchillo;
    public ArmaPistola armaPistola;

    [Header("Estado del Inventario")]
    public bool tieneCuchillo = false;
    public bool tienePistola = false;
    public bool tieneEscopeta = false;

    // 0 = Manos vacías, 1 = Cuchillo, 2 = Pistola, 3 = Escopeta
    private int armaActiva = 0;

    private void Start()
    {
        // Al iniciar el Nivel 1, armaActiva será 0 y ocultará todas las armas
        ActualizarArmaActiva();
    }

    // --- NUEVO METODO UNIFICADO DE RECOGIDA ---
    public bool RecogerItem(ItemRecogible.TipoItem tipo, int cantidadMunicion)
    {
        switch (tipo)
        {
            case ItemRecogible.TipoItem.Cuchillo:
                if (tieneCuchillo) return false; // Ya lo tiene, no lo recoge
                tieneCuchillo = true;
                EquiparArma(1);
                Debug.Log("¡Cuchillo recogido!");
                return true;

            case ItemRecogible.TipoItem.Pistola:
                if (tienePistola) return false;
                tienePistola = true;
                EquiparArma(2);
                Debug.Log("¡Pistola recogida!");
                return true;

            case ItemRecogible.TipoItem.MunicionPistola:
                if (armaPistola != null)
                {
                    armaPistola.AgregarMunicionReserva(cantidadMunicion);
                    Debug.Log("Munición recogida: +" + cantidadMunicion);
                    return true;
                }
                return false;
        }
        return false;
    }

    public void EquiparArma(int indiceArma)
    {
        if (indiceArma == 1 && tieneCuchillo) armaActiva = 1;
        else if (indiceArma == 2 && tienePistola) armaActiva = 2;
        // else if (indiceArma == 3 && tieneEscopeta) armaActiva = 3;

        ActualizarArmaActiva();
    }

    private void ActualizarArmaActiva()
    {
        if (armaCuchillo != null) armaCuchillo.gameObject.SetActive(armaActiva == 1);
        if (armaPistola != null) armaPistola.gameObject.SetActive(armaActiva == 2);
    }

    public void EjecutarAtaque()
    {
        if (armaActiva == 1 && armaCuchillo != null) armaCuchillo.Atacar();
        else if (armaActiva == 2 && armaPistola != null) armaPistola.Disparar();
    }

    public void EjecutarRecarga()
    {
        if (armaActiva == 2 && armaPistola != null) armaPistola.Recargar();
    }
}