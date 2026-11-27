using UnityEngine;
using UnityEngine.UI;

public class GestorArmas : MonoBehaviour
{
    [Header("Referencias a las Armas")]
    public ArmaCuchillo armaCuchillo;
    public ArmaPistola armaPistola;

    [Header("UI Dinamica Boton Ataque")]
    [SerializeField] private Image imagenBotonAtaque;
    [SerializeField] private Sprite iconoCuchillo;
    [SerializeField] private Sprite iconoPistola;

    [Header("Estado del Inventario")]
    public bool tieneCuchillo = true;
    public bool tienePistola = false;

    // 0 = Ninguna, 1 = Cuchillo, 2 = Pistola
    private int armaActiva = 0;

    private void Start()
    {
        // Si no se ha equipado nada desde SaveManager, equipar el cuchillo por defecto
        if (armaActiva == 0 && tieneCuchillo)
        {
            EquiparArma(1);
        }
        else
        {
            ActualizarArmaActiva();
        }
    }

    public bool RecogerItem(ItemRecogible.TipoItem tipo, int cantidad)
    {
        switch (tipo)
        {
            case ItemRecogible.TipoItem.Cuchillo:
                if (tieneCuchillo) return false;
                tieneCuchillo = true;
                EquiparArma(1);
                return true;

            case ItemRecogible.TipoItem.Pistola:
                if (tienePistola) return false;
                tienePistola = true;
                EquiparArma(2);
                return true;

            case ItemRecogible.TipoItem.MunicionPistola:
                if (armaPistola != null)
                {
                    armaPistola.AgregarMunicionReserva(cantidad);
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

        ActualizarArmaActiva();
    }

    public void AlternarArma()
    {
        if (armaActiva == 1 && tienePistola)
        {
            EquiparArma(2);
        }
        else if (armaActiva == 2 && tieneCuchillo)
        {
            EquiparArma(1);
        }
        else if (armaActiva == 0)
        {
            if (tienePistola) EquiparArma(2);
            else if (tieneCuchillo) EquiparArma(1);
        }
    }

    private void ActualizarArmaActiva()
    {
        if (armaCuchillo != null) armaCuchillo.gameObject.SetActive(armaActiva == 1);
        if (armaPistola != null) armaPistola.gameObject.SetActive(armaActiva == 2);

        ActualizarIconoUI();
    }

    private void ActualizarIconoUI()
    {
        if (imagenBotonAtaque == null) return;

        if (armaActiva == 1 && iconoCuchillo != null)
        {
            imagenBotonAtaque.enabled = true;
            imagenBotonAtaque.sprite = iconoCuchillo;
        }
        else if (armaActiva == 2 && iconoPistola != null)
        {
            imagenBotonAtaque.enabled = true;
            imagenBotonAtaque.sprite = iconoPistola;
        }
        else
        {
            imagenBotonAtaque.enabled = false;
        }
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