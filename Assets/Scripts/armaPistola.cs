using System.Collections;
using UnityEngine;

public class ArmaPistola : MonoBehaviour
{
    // --- VARIABLES DE CONFIGURACION ---
    [Header("Configuracion de Disparo")]
    [SerializeField] private float danio = 40.0f;
    [SerializeField] private float alcanceDisparo = 25.0f;
    [SerializeField] private float recargaDisparo = 0.5f; // Cooldown total entre disparos
    [SerializeField] private LayerMask capasEnemigo;

    [Header("Configuracion de Municion y Recarga")]
    [SerializeField] private int tamanoCargador = 12;
    [SerializeField] private int municionActual = 12;
    [SerializeField] private int municionReserva = 36;
    [SerializeField] private float tiempoRecarga = 1.2f;

    [Header("Sprite Estado Sin Balas")]
    [SerializeField] private Sprite spriteSinBalas;       // Cerrojo abierto / Sin municion

    [Header("Animacion de Disparo (5 Sprites)")]
    [SerializeField] private Sprite spriteDisparo1;     // Frame 1: Reposo / Inicio
    [SerializeField] private Sprite spriteDisparo2;     // Frame 2: Retroceso
    [SerializeField] private Sprite spriteDisparo3;     // Frame 3: Destello en cañon
    [SerializeField] private Sprite spriteDisparo4;     // Frame 4: Expulsion de casquillo
    [SerializeField] private Sprite spriteDisparo5;     // Frame 5: Recuperacion

    [Header("Duracion por Frame de Disparo (Segundos)")]
    [SerializeField] private float duracionFrameDisparo1 = 0.03f;
    [SerializeField] private float duracionFrameDisparo2 = 0.03f;
    [SerializeField] private float duracionFrameDisparo3 = 0.03f;
    [SerializeField] private float duracionFrameDisparo4 = 0.03f;
    [SerializeField] private float duracionFrameDisparo5 = 0.03f;

    [Header("Animacion de Recarga (5 Sprites)")]
    [SerializeField] private Sprite spriteRecarga1;     // Frame 1: Inclinando arma
    [SerializeField] private Sprite spriteRecarga2;     // Frame 2: Extrayendo cargador
    [SerializeField] private Sprite spriteRecarga3;     // Frame 3: Cama vacia / Toma de repuesto
    [SerializeField] private Sprite spriteRecarga4;     // Frame 4: Insertando cargador
    [SerializeField] private Sprite spriteRecarga5;     // Frame 5: Cerrojo rearmado

    [Header("Duracion por Frame de Recarga (Segundos)")]
    [SerializeField] private float duracionFrameRecarga1 = 0.2f;
    [SerializeField] private float duracionFrameRecarga2 = 0.2f;
    [SerializeField] private float duracionFrameRecarga3 = 0.3f;
    [SerializeField] private float duracionFrameRecarga4 = 0.3f;
    [SerializeField] private float duracionFrameRecarga5 = 0.2f;

    [Header("Referencias Componentes")]
    [SerializeField] private SpriteRenderer renderizadorSprite;
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoSinBalas;
    [SerializeField] private AudioClip sonidoRecarga;
    [SerializeField] private Transform camaraJugador;

    // --- VARIABLES INTERNAS ---
    private bool puedeDisparar = true;
    private bool estaRecargando = false;

    // --- PROPIEDADES PUBLICAS PARA UI ---
    public int MunicionActual => municionActual;
    public int MunicionReserva => municionReserva;

    private void OnEnable()
    {
        estaRecargando = false;
        puedeDisparar = true;
        ActualizarSpriteReposo();
    }

    // --- ACCION DE DISPARAR ---
    public void Disparar()
    {
        if (estaRecargando || !puedeDisparar) return;

        if (municionActual > 0)
        {
            StartCoroutine(RutinaAnimacionDisparo());
        }
        else
        {
            // Intentar recargar si hay reserva o sonar clic seco
            if (municionReserva > 0)
            {
                Recargar();
            }
            else
            {
                EstablecerSprite(spriteSinBalas);
                if (fuenteAudio != null && sonidoSinBalas != null)
                {
                    fuenteAudio.PlayOneShot(sonidoSinBalas);
                }
            }
        }
    }

    // --- CORRUTINA DE DISPARO (SECUENCIA DE 5 SPRITES CON TIEMPOS EDITABLES) ---
    private IEnumerator RutinaAnimacionDisparo()
    {
        puedeDisparar = false;
        municionActual--;

        // Audio
        if (fuenteAudio != null && sonidoDisparo != null)
        {
            fuenteAudio.PlayOneShot(sonidoDisparo);
        }

        // Raycast de disparo al centro de la pantalla
        if (camaraJugador != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(camaraJugador.position, camaraJugador.forward, out hit, alcanceDisparo, capasEnemigo))
            {
                // Buscar el script SaludEnemigo en el objeto impactado o en sus padres
                SaludEnemigo salud = hit.collider.GetComponent<SaludEnemigo>();
                if (salud == null) salud = hit.collider.GetComponentInParent<SaludEnemigo>();

                if (salud != null)
                {
                    salud.RecibirDanio(danio); // Transfiere los puntos de daño al enemigo
                }
            }
        }

        // Secuencia visual de 5 Sprites para el disparo
        EstablecerSprite(spriteDisparo1);
        yield return new WaitForSeconds(duracionFrameDisparo1);

        EstablecerSprite(spriteDisparo2);
        yield return new WaitForSeconds(duracionFrameDisparo2);

        EstablecerSprite(spriteDisparo3);
        yield return new WaitForSeconds(duracionFrameDisparo3);

        EstablecerSprite(spriteDisparo4);
        yield return new WaitForSeconds(duracionFrameDisparo4);

        EstablecerSprite(spriteDisparo5);
        yield return new WaitForSeconds(duracionFrameDisparo5);

        // Volver al sprite de reposo correspondiente (normal o sin balas)
        ActualizarSpriteReposo();

        // Calcular el tiempo restante del cooldown
        float tiempoAnimacionDisparo = duracionFrameDisparo1 + duracionFrameDisparo2 + duracionFrameDisparo3 + duracionFrameDisparo4 + duracionFrameDisparo5;
        float tiempoRestante = recargaDisparo - tiempoAnimacionDisparo;

        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

        puedeDisparar = true;
    }

    // --- ACCION DE RECARGAR ---
    public void Recargar()
    {
        if (estaRecargando || municionActual == tamanoCargador || municionReserva <= 0) return;

        StartCoroutine(RutinaAnimacionRecarga());
    }

    // --- CORRUTINA DE RECARGA (SECUENCIA DE 5 SPRITES CON TIEMPOS EDITABLES) ---
    private IEnumerator RutinaAnimacionRecarga()
    {
        estaRecargando = true;
        puedeDisparar = false;

        if (fuenteAudio != null && sonidoRecarga != null)
        {
            fuenteAudio.PlayOneShot(sonidoRecarga);
        }

        // Secuencia visual de 5 Sprites para la recarga
        EstablecerSprite(spriteRecarga1);
        yield return new WaitForSeconds(duracionFrameRecarga1);

        EstablecerSprite(spriteRecarga2);
        yield return new WaitForSeconds(duracionFrameRecarga2);

        EstablecerSprite(spriteRecarga3);
        yield return new WaitForSeconds(duracionFrameRecarga3);

        EstablecerSprite(spriteRecarga4);
        yield return new WaitForSeconds(duracionFrameRecarga4);

        EstablecerSprite(spriteRecarga5);
        yield return new WaitForSeconds(duracionFrameRecarga5);

        // Calculo de traspaso de municion
        int necesidades = tamanoCargador - municionActual;
        int recargaEfectiva = Mathf.Min(necesidades, municionReserva);

        municionActual += recargaEfectiva;
        municionReserva -= recargaEfectiva;

        // Finalizar recarga y cambiar sprite
        ActualizarSpriteReposo();
        estaRecargando = false;
        puedeDisparar = true;
    }

    // --- METODO DE RECOGIDA DE BALAS EN EL MAPA ---
    public void AgregarMunicionReserva(int cantidad)
    {
        municionReserva += cantidad;
    }

    private void ActualizarSpriteReposo()
    {
        if (municionActual <= 0)
        {
            EstablecerSprite(spriteSinBalas);
        }
        else
        {
            EstablecerSprite(spriteDisparo1); // Usa el Frame 1 como posicion de reposo habitual
        }
    }

    private void EstablecerSprite(Sprite nuevoSprite)
    {
        if (renderizadorSprite != null && nuevoSprite != null)
        {
            renderizadorSprite.sprite = nuevoSprite;
        }
    }
}