using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // ← Agregado para reiniciar escena

/// <summary>
/// Sistema de Screamer (RF18)
/// Muestra imagen de susto, reproduce sonido y reinicia la escena
/// </summary>
public class ScreamerManager : MonoBehaviour
{
    [Header("=== REFERENCIAS ===")]
    [Tooltip("Imagen del screamer (debe estar en un Canvas)")]
    public Image imagenScreamer;

    [Tooltip("AudioSource para el sonido del screamer")]
    public AudioSource audioScreamer;

    [Header("=== CONFIGURACIÓN ===")]
    [Tooltip("Duración que se muestra el screamer (segundos)")]
    public float duracionScreamer = 2.5f;

    [Tooltip("Volumen del sonido del screamer (0-1)")]
    [Range(0f, 1f)]
    public float volumenScreamer = 1.0f;

    [Tooltip("Reiniciar escena después del screamer")]
    public bool reiniciarEscena = true;

    [Tooltip("Tiempo adicional antes de reiniciar (después del screamer)")]
    public float tiempoAntesDeReiniciar = 0.5f;

    [Header("=== ESTADO ===")]
    [Tooltip("Indica si el screamer está activo actualmente")]
    public bool screamerActivo = false;

    private static ScreamerManager instance;

    void Awake()
    {
        // Singleton: Solo puede haber una instancia
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Asegurar que el screamer esté oculto al inicio
        if (imagenScreamer != null)
        {
            imagenScreamer.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Método estático para activar el screamer desde cualquier script
    /// </summary>
    public static void MostrarScreamer()
    {
        if (instance != null)
        {
            instance.ActivarScreamer();
        }
        else
        {
            Debug.LogError("❌ No hay ScreamerManager en la escena");
        }
    }

    /// <summary>
    /// Activa el screamer: muestra imagen, reproduce sonido y reinicia escena
    /// </summary>
    void ActivarScreamer()
    {
        if (screamerActivo) return; // Evitar múltiples activaciones

        StartCoroutine(MostrarScreamerCoroutine());
    }

    IEnumerator MostrarScreamerCoroutine()
    {
        screamerActivo = true;

        Debug.Log("💀 ¡SCREAMER ACTIVADO!");

        // Mostrar imagen
        if (imagenScreamer != null)
        {
            imagenScreamer.gameObject.SetActive(true);

            // Fade in rápido
            Color color = imagenScreamer.color;
            color.a = 0f;
            imagenScreamer.color = color;

            float fadeInDuration = 0.2f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                imagenScreamer.color = color;
                yield return null;
            }

            color.a = 1f;
            imagenScreamer.color = color;
        }

        // Reproducir sonido
        if (audioScreamer != null && audioScreamer.clip != null)
        {
            audioScreamer.volume = volumenScreamer;
            audioScreamer.Play();
        }

        // Esperar duración del screamer
        yield return new WaitForSeconds(duracionScreamer);

        Debug.Log("✅ Screamer finalizado");

        // Reiniciar escena si está activado
        if (reiniciarEscena)
        {
            Debug.Log("🔄 Reiniciando escena...");

            // Esperar un poco más antes de reiniciar (opcional)
            if (tiempoAntesDeReiniciar > 0)
            {
                yield return new WaitForSeconds(tiempoAntesDeReiniciar);
            }

            // Obtener el nombre de la escena actual
            string escenaActual = SceneManager.GetActiveScene().name;

            // Reiniciar la escena
            SceneManager.LoadScene(escenaActual);
        }
        else
        {
            // Si no reinicia, solo ocultar
            if (imagenScreamer != null)
            {
                imagenScreamer.gameObject.SetActive(false);
            }

            screamerActivo = false;
        }
    }

    /// <summary>
    /// Forzar ocultar el screamer (útil para debugging)
    /// </summary>
    public void OcultarScreamer()
    {
        StopAllCoroutines();

        if (imagenScreamer != null)
        {
            imagenScreamer.gameObject.SetActive(false);
        }

        screamerActivo = false;
    }

    /// <summary>
    /// Reiniciar escena manualmente (puede llamarse desde otros scripts)
    /// </summary>
    public static void ReiniciarEscena()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(escenaActual);
    }
}