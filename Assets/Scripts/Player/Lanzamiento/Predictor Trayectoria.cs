using UnityEngine;

/// <summary>
/// Calcula y dibuja la trayectoria predictiva del lanzamiento
/// Usa física real para simular dónde caerá el objeto
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PredictorTrayectoria : MonoBehaviour
{
    [Header("Configuración de la Línea")]
    [Tooltip("Número de puntos en la trayectoria (más = más preciso)")]
    [Range(10, 100)]
    public int numeroPuntos = 50;

    [Tooltip("Tiempo total de la simulación en segundos")]
    [Range(1f, 10f)]
    public float tiempoSimulacion = 5f;

    [Tooltip("Color de la línea de trayectoria")]
    public Color colorLinea = new Color(1f, 1f, 1f, 0.3f);

    [Tooltip("Ancho de la línea")]
    public float anchoLinea = 0.05f;

    private LineRenderer lineRenderer;
    private bool mostrarTrayectoria = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ConfigurarLineRenderer();
    }

    void ConfigurarLineRenderer()
    {
        // Configurar el LineRenderer
        lineRenderer.positionCount = numeroPuntos;
        lineRenderer.startWidth = anchoLinea;
        lineRenderer.endWidth = anchoLinea;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = colorLinea;
        lineRenderer.endColor = colorLinea;

        // Desactivar por defecto
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Muestra la trayectoria predictiva
    /// </summary>
    /// <param name="puntoInicio">Punto desde donde se lanza</param>
    /// <param name="velocidadInicial">Velocidad inicial del lanzamiento</param>
    public void MostrarTrayectoria(Vector3 puntoInicio, Vector3 velocidadInicial)
    {
        mostrarTrayectoria = true;
        lineRenderer.enabled = true;

        // Calcular y dibujar la trayectoria
        CalcularTrayectoria(puntoInicio, velocidadInicial);
    }

    /// <summary>
    /// Oculta la trayectoria
    /// </summary>
    public void OcultarTrayectoria()
    {
        mostrarTrayectoria = false;
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Calcula los puntos de la trayectoria usando física real
    /// </summary>
    void CalcularTrayectoria(Vector3 puntoInicio, Vector3 velocidadInicial)
    {
        Vector3[] puntos = new Vector3[numeroPuntos];
        float incrementoTiempo = tiempoSimulacion / numeroPuntos;

        for (int i = 0; i < numeroPuntos; i++)
        {
            float tiempo = i * incrementoTiempo;

            // Fórmula de trayectoria parabólica
            // Posición = PosInicial + Velocidad*Tiempo + 0.5*Gravedad*Tiempo^2
            puntos[i] = puntoInicio
                      + velocidadInicial * tiempo
                      + 0.5f * Physics.gravity * tiempo * tiempo;

            // Verificar si choca con algo
            if (i > 0)
            {
                RaycastHit hit;
                Vector3 direccion = puntos[i] - puntos[i - 1];
                float distancia = direccion.magnitude;

                if (Physics.Raycast(puntos[i - 1], direccion.normalized, out hit, distancia))
                {
                    // Si choca, acortar la línea hasta el punto de impacto
                    puntos[i] = hit.point;
                    lineRenderer.positionCount = i + 1;
                    break;
                }
            }
        }

        lineRenderer.SetPositions(puntos);
    }

    /// <summary>
    /// Actualiza la trayectoria en tiempo real
    /// </summary>
    public void ActualizarTrayectoria(Vector3 puntoInicio, Vector3 velocidadInicial)
    {
        if (mostrarTrayectoria)
        {
            // Resetear el número de puntos por si fue acortado
            lineRenderer.positionCount = numeroPuntos;
            CalcularTrayectoria(puntoInicio, velocidadInicial);
        }
    }
}