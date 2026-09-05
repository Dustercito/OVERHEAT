using UnityEngine;
using UnityEngine.Rendering;

public class CarteleraSprite : MonoBehaviour
{
    private Transform transformCamara;
    private SpriteRenderer renderizadorSprite;

    // Guarda la intensidad actual dictada por el estado de la FSM
    private float intensidadBrilloActual = 1.0f;

    private void Start()
    {
        // 1. Obtener la referencia de la camara principal
        if (Camera.main != null)
        {
            transformCamara = Camera.main.transform;
        }

        // 2. Activar la construccion a dos caras (TwoSided) y recepcion de sombras
        renderizadorSprite = GetComponent<SpriteRenderer>();
        if (renderizadorSprite != null)
        {
            // TwoSided permite que proyecte sombra dinámicamente sin importar el ángulo de la luz
            renderizadorSprite.shadowCastingMode = ShadowCastingMode.TwoSided;
            renderizadorSprite.receiveShadows = true;
        }
    }

    private void LateUpdate()
    {
        if (transformCamara == null) return;

        // 3. Orientar el sprite hacia la camara bloqueando el eje Y
        Vector3 direccionCamara = transformCamara.forward;
        direccionCamara.y = 0;

        if (direccionCamara != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionCamara);
        }
    }

    // --- METODO FSM: Modifica el tono/brillo base segun el estado actual del enemigo ---
    public void CambiarBrillo(float intensidad)
    {
        intensidadBrilloActual = intensidad;

        if (renderizadorSprite != null)
        {
            renderizadorSprite.color = new Color(intensidad, intensidad, intensidad, 1f);
        }
    }

    // --- METODO SALUD: Aplica temporalmente un tinte de color (ejemplo: Rojo de daño) ---
    public void AplicarTinteTemp(Color colorTinte)
    {
        if (renderizadorSprite != null)
        {
            renderizadorSprite.color = colorTinte;
        }
    }

    // --- METODO SALUD: Restaura el color al brillo que le corresponde segun la FSM ---
    public void RestaurarColorBrillo()
    {
        if (renderizadorSprite != null)
        {
            renderizadorSprite.color = new Color(intensidadBrilloActual, intensidadBrilloActual, intensidadBrilloActual, 1f);
        }
    }
}