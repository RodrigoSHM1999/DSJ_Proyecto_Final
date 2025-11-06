using UnityEngine;

/// <summary>
/// Sistema de lanzamiento con carga de fuerza progresiva
/// Maneja el click del mouse, carga de fuerza y lanzamiento
/// </summary>
[RequireComponent(typeof(PredictorTrayectoria))]
public class SistemaLanzamiento : MonoBehaviour
{
    [Header("Configuración de Fuerza")]
    [Tooltip("Fuerza mínima de lanzamiento")]
    [Range(1f, 20f)]
    public float fuerzaMinima = 5f;

    [Tooltip("Fuerza máxima de lanzamiento (límite realista)")]
    [Range(10f, 50f)]
    public float fuerzaMaxima = 20f;

    [Tooltip("Velocidad de carga de fuerza por segundo")]
    [Range(1f, 30f)]
    public float velocidadCarga = 10f;

    [Header("Referencias")]
    [Tooltip("Cámara del jugador")]
    public Camera camaraJugador;

    [Header("Configuración de Lanzamiento")]
    [Tooltip("Punto desde donde se lanza (posición del objeto sostenido)")]
    public Transform puntoLanzamiento;

    // Referencias internas
    private PredictorTrayectoria predictorTrayectoria;
    private RecogerObjetos sistemaRecoger;
    private ObjetoRecogible objetoActual;

    // Estado del lanzamiento
    private bool estaCargando = false;
    private float fuerzaActual = 0f;

    void Start()
    {
        // Obtener componentes
        predictorTrayectoria = GetComponent<PredictorTrayectoria>();
        sistemaRecoger = GetComponent<RecogerObjetos>();

        // Obtener cámara si no está asignada
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Si no hay punto de lanzamiento, usar la posición de la cámara
        if (puntoLanzamiento == null)
        {
            GameObject puntoObj = new GameObject("PuntoLanzamiento");
            puntoObj.transform.parent = camaraJugador.transform;
            puntoObj.transform.localPosition = new Vector3(0f, -0.3f, 1f);
            puntoLanzamiento = puntoObj.transform;
        }
    }

    void Update()
    {
        // Solo funciona si tenemos un objeto recogido
        if (objetoActual == null) return;

        // Detectar cuando se presiona el botón izquierdo del mouse
        if (Input.GetMouseButtonDown(0))
        {
            IniciarCarga();
        }

        // Mientras se mantiene presionado, cargar fuerza
        if (Input.GetMouseButton(0) && estaCargando)
        {
            CargarFuerza();
        }

        // Al soltar, lanzar
        if (Input.GetMouseButtonUp(0) && estaCargando)
        {
            Lanzar();
        }
    }

    /// <summary>
    /// Inicia el proceso de carga de fuerza
    /// </summary>
    void IniciarCarga()
    {
        estaCargando = true;
        fuerzaActual = fuerzaMinima;

        // Mostrar trayectoria inicial
        ActualizarTrayectoria();
    }

    /// <summary>
    /// Aumenta la fuerza progresivamente mientras se mantiene presionado
    /// </summary>
    void CargarFuerza()
    {
        // Aumentar fuerza con el tiempo
        fuerzaActual += velocidadCarga * Time.deltaTime;

        // Limitar a la fuerza máxima
        fuerzaActual = Mathf.Min(fuerzaActual, fuerzaMaxima);

        // Actualizar la trayectoria en tiempo real
        ActualizarTrayectoria();
    }

    /// <summary>
    /// Actualiza la visualización de la trayectoria
    /// </summary>
    void ActualizarTrayectoria()
    {
        if (predictorTrayectoria != null)
        {
            Vector3 velocidadInicial = CalcularVelocidadInicial();
            predictorTrayectoria.MostrarTrayectoria(puntoLanzamiento.position, velocidadInicial);
        }
    }

    /// <summary>
    /// Calcula el vector de velocidad inicial basado en la fuerza actual
    /// </summary>
    Vector3 CalcularVelocidadInicial()
    {
        // La dirección es hacia donde apunta la cámara
        Vector3 direccion = camaraJugador.transform.forward;

        // La velocidad depende de la fuerza y la masa del objeto
        float masa = objetoActual != null ? objetoActual.ObtenerMasa() : 1f;

        // Velocidad = Fuerza / Masa (simplificado)
        Vector3 velocidad = direccion * (fuerzaActual / masa);

        return velocidad;
    }

    /// <summary>
    /// Lanza el objeto con la fuerza cargada
    /// </summary>
    void Lanzar()
    {
        if (objetoActual == null) return;

        // Calcular fuerza de lanzamiento
        Vector3 direccionLanzamiento = camaraJugador.transform.forward;
        Vector3 fuerzaLanzamiento = direccionLanzamiento * fuerzaActual;

        // Lanzar el objeto
        objetoActual.Lanzar(fuerzaLanzamiento);

        // Ocultar la trayectoria
        predictorTrayectoria.OcultarTrayectoria();

        // Limpiar referencias
        objetoActual = null;
        estaCargando = false;
        fuerzaActual = 0f;

        // Informar al sistema de recogida
        if (sistemaRecoger != null)
        {
            sistemaRecoger.LimpiarObjetoActual();
        }

        Debug.Log("Objeto lanzado con fuerza: " + fuerzaActual);
    }

    /// <summary>
    /// Establece el objeto actual que se va a lanzar
    /// Llamado por RecogerObjetos cuando se recoge algo
    /// </summary>
    public void EstablecerObjetoActual(ObjetoRecogible objeto)
    {
        objetoActual = objeto;
    }

    /// <summary>
    /// Obtiene la fuerza actual (útil para UI)
    /// </summary>
    public float ObtenerFuerzaActual()
    {
        return fuerzaActual;
    }

    /// <summary>
    /// Obtiene el porcentaje de carga (0 a 1)
    /// </summary>
    public float ObtenerPorcentajeCarga()
    {
        return Mathf.InverseLerp(fuerzaMinima, fuerzaMaxima, fuerzaActual);
    }

    // Visualización en el editor
    void OnDrawGizmos()
    {
        if (estaCargando && camaraJugador != null)
        {
            // Dibujar la dirección de lanzamiento
            Gizmos.color = Color.red;
            Vector3 origen = puntoLanzamiento != null ? puntoLanzamiento.position : camaraJugador.transform.position;
            Vector3 direccion = camaraJugador.transform.forward * 2f;
            Gizmos.DrawRay(origen, direccion);
        }
    }
}