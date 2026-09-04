using UnityEngine;
using UnityEngine.Rendering; // Requerido para ShadowCastingMode

public class CarteleraSprite : MonoBehaviour
{
    private Transform transformCamara;
    private SpriteRenderer renderizadorSprite;

    private void Start()
    {
        // 1. Obtener la referencia de la camara principal
        if (Camera.main != null)
        {
            transformCamara = Camera.main.transform;
        }

        // 2. Activar la construccion y recepcion de sombras en el SpriteRenderer
        renderizadorSprite = GetComponent<SpriteRenderer>();
        if (renderizadorSprite != null)
        {
            renderizadorSprite.shadowCastingMode = ShadowCastingMode.On;
            renderizadorSprite.receiveShadows = true;
        }
    }

    private void LateUpdate()
    {
        if (transformCamara == null) return;

        // 3. Obtener la direccion frontal de la camara y bloquear la inclinacion en Y
        Vector3 direccionCamara = transformCamara.forward;
        direccionCamara.y = 0;

        // 4. Orientar el sprite hacia la camara
        if (direccionCamara != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccionCamara);
        }
    }

    // --- MODIFICACIÓN: Control de intensidad de brillo en escala de blancos ---
    public void CambiarBrillo(float intensidad)
    {
        if (renderizadorSprite != null)
        {
            // Mantiene el color original multiplicando el blanco base por la intensidad
            renderizadorSprite.color = new Color(intensidad, intensidad, intensidad, 1f);
        }
    }
}