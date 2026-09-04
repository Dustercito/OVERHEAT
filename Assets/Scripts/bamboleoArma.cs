using UnityEngine;

public class BamboleoArma : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CharacterController contrPers;

    [Header("Parametros de Balanceo Independiente")]
    [SerializeField] private float frecuencia = 10.0f;  // Velocidad del vaiven de la mano
    [SerializeField] private float amplitudX = 0.03f;   // Balanceo horizontal
    [SerializeField] private float amplitudY = 0.02f;   // Balanceo vertical
    [SerializeField] private float suavizado = 8.0f;    // Fluidez de la inercia

    [Header("Inercia por Rotacion (Sway)")]
    [SerializeField] private float inclinacionGiro = 0.05f; // Reaccion suave al rotar la vista
    [SerializeField] private float maxInclinacion = 0.1f;

    private Vector3 posInicialLocal;
    private float temporizador = 0.0f;

    private void Start()
    {
        // Guardar la posicion en reposo respecto al objeto padre (Main Camera)
        posInicialLocal = transform.localPosition;

        // Si no se asigna manualmente, buscar el CharacterController en la raiz
        if (contrPers == null)
        {
            contrPers = GetComponentInParent<CharacterController>();
        }
    }

    private void Update()
    {
        if (contrPers == null) return;

        // 1. Obtener la velocidad del movimiento horizontal
        Vector3 velHorizontal = new Vector3(contrPers.velocity.x, 0f, contrPers.velocity.z);
        float velActual = velHorizontal.magnitude;

        Vector3 posObjetivo = posInicialLocal;

        // 2. Calcular bamboleo si el jugador camina sobre el suelo
        if (contrPers.isGrounded && velActual > 0.1f)
        {
            temporizador += Time.deltaTime * frecuencia;

            // Figura sinusoidal en '8' horizontal
            float desfaseX = Mathf.Cos(temporizador * 0.5f) * amplitudX;
            float desfaseY = Mathf.Sin(temporizador) * amplitudY;

            posObjetivo += new Vector3(desfaseX, desfaseY, 0f);
        }
        else
        {
            temporizador = 0.0f;
        }

        // 3. Aplicar Lerp hacia la posicion deseada (Se suma/complementa al movimiento de la camara)
        transform.localPosition = Vector3.Lerp(transform.localPosition, posObjetivo, Time.deltaTime * suavizado);
    }
}