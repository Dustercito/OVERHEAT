using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [Header("Referencias del Jugador")]
    [SerializeField] private Transform transfJugador;
    [SerializeField] private CharacterController contrPers;
    [SerializeField] private SaludJugador saludJugador;
    [SerializeField] private GestorArmas gestorArmas;

    // --- GUARDAR PARTIDA ---
    public void GuardarJuego()
    {
        // 1. Guardar Escena Actual
        PlayerPrefs.SetInt("EscenaGuardada", SceneManager.GetActiveScene().buildIndex);

        // 2. Guardar Posición del Jugador
        if (transfJugador != null)
        {
            PlayerPrefs.SetFloat("JugadorPosX", transfJugador.position.x);
            PlayerPrefs.SetFloat("JugadorPosY", transfJugador.position.y);
            PlayerPrefs.SetFloat("JugadorPosZ", transfJugador.position.z);
        }

        // 3. Guardar Salud
        if (saludJugador != null)
        {
            PlayerPrefs.SetFloat("VidaJugador", saludJugador.VidaActual);
        }

        // 4. Guardar Armas (0 = false, 1 = true)
        if (gestorArmas != null)
        {
            PlayerPrefs.SetInt("TieneCuchillo", gestorArmas.tieneCuchillo ? 1 : 0);
            PlayerPrefs.SetInt("TienePistola", gestorArmas.tienePistola ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log("¡Partida Guardada Exitosamente con Posición!");
    }

    // --- CARGAR DATOS AL INICIAR LA ESCENA ---
    private void Start()
    {
        // Solo cargamos los datos si existe un guardado previo para esta escena
        if (PlayerPrefs.HasKey("VidaJugador"))
        {
            CargarDatosJugador();
        }
    }

    private void CargarDatosJugador()
    {
        // 1. Restaurar Posición
        if (transfJugador != null && PlayerPrefs.HasKey("JugadorPosX"))
        {
            float posX = PlayerPrefs.GetFloat("JugadorPosX");
            float posY = PlayerPrefs.GetFloat("JugadorPosY");
            float posZ = PlayerPrefs.GetFloat("JugadorPosZ");
            Vector3 posGuardada = new Vector3(posX, posY, posZ);

            // Para mover al jugador correctamente, apagamos el CharacterController un instante
            if (contrPers != null) contrPers.enabled = false;

            transfJugador.position = posGuardada;

            if (contrPers != null) contrPers.enabled = true;
        }

        // 2. Restaurar Salud
        if (saludJugador != null)
        {
            float vidaGuardada = PlayerPrefs.GetFloat("VidaJugador", saludJugador.VidaMaxima);
            saludJugador.Curar(vidaGuardada);
        }

        // 3. Restaurar Armas
        if (gestorArmas != null)
        {
            gestorArmas.tieneCuchillo = PlayerPrefs.GetInt("TieneCuchillo", 0) == 1;
            gestorArmas.tienePistola = PlayerPrefs.GetInt("TienePistola", 0) == 1;

            // Re-equipar la última arma
            if (gestorArmas.tienePistola) gestorArmas.EquiparArma(2);
            else if (gestorArmas.tieneCuchillo) gestorArmas.EquiparArma(1);
        }
    }
}