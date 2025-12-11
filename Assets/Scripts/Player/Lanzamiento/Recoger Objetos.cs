using UnityEngine;

/// <summary>
/// Sistema para detectar y recoger objetos cercanos
/// Coloca este script en el jugador (cámara en primera persona)
/// </summary>
public class RecogerObjetos : MonoBehaviour
{
    [Header("Configuración de Recogida")]
    [Tooltip("Tecla para recoger objetos")]
    public KeyCode teclaRecoger = KeyCode.E;

    [Tooltip("Distancia máxima para recoger objetos")]
    [Range(1f, 10f)]
    public float distanciaRecogida = 5f;

    [Tooltip("Usar filtro de capa (desmarca para detectar cualquier objeto)")]
    public bool usarFiltroCapa = false;

    [Tooltip("Capa de los objetos recogibles (solo si usarFiltroCapa está activado)")]
    public LayerMask capaObjetos;

    [Header("Posición del Objeto Recogido")]
    [Tooltip("Posición relativa donde se sostiene el objeto (frente a la cámara)")]
    public Vector3 posicionSostener = new Vector3(0f, -0.3f, 1f);

    [Header("Referencias")]
    [Tooltip("Cámara del jugador (se detecta automáticamente si está vacío)")]
    public Camera camaraJugador;

    // Referencias internas
    private ObjetoRecogible objetoActual;
    private Transform objetoRecogidoTransform;
    private SistemaLanzamiento sistemaLanzamiento;

    void Start()
    {
        // Obtener la cámara si no está asignada
        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }

        // Obtener el sistema de lanzamiento
        sistemaLanzamiento = GetComponent<SistemaLanzamiento>();

        if (sistemaLanzamiento == null)
        {
            Debug.LogError("No se encontró SistemaLanzamiento en el jugador!");
        }
    }

    void Update()
    {
        // Si no tenemos objeto, intentar recoger
        if (objetoActual == null)
        {
            if (Input.GetKeyDown(teclaRecoger))
            {
                IntentarRecoger();
            }
        }
        else
        {
            // Mantener el objeto en posición frente al jugador
            ActualizarPosicionObjeto();
        }
    }

    /// <summary>
    /// Intenta recoger un objeto cercano
    /// </summary>
    void IntentarRecoger()
    {
        RaycastHit hit;
        Vector3 origen = camaraJugador.transform.position;
        Vector3 direccion = camaraJugador.transform.forward;

        Debug.Log("=== Intentando recoger objeto ===");
        Debug.Log("Origen: " + origen);
        Debug.Log("Dirección: " + direccion);

        bool impacto = false;

        // Lanzar raycast con o sin filtro de capa
        if (usarFiltroCapa)
        {
            impacto = Physics.Raycast(origen, direccion, out hit, distanciaRecogida, capaObjetos);
        }
        else
        {
            impacto = Physics.Raycast(origen, direccion, out hit, distanciaRecogida);
        }
        if (impacto)
        {
            Debug.Log("✓ Raycast impactó con: " + hit.collider.name + " | Distancia: " + hit.distance.ToString("F2") + "m");
            Debug.Log("Capa del objeto: " + LayerMask.LayerToName(hit.collider.gameObject.layer));

            // Verificar si el objeto tiene el componente ObjetoRecogible
            ObjetoRecogible objeto = hit.collider.GetComponent<ObjetoRecogible>();

            if (objeto != null)
            {
                if (!objeto.estaRecogido)
                {
                    Debug.Log("✓ ¡Objeto válido encontrado! Recogiendo...");
                    RecogerObjeto(objeto);
                }
                else
                {
                    Debug.LogWarning("✗ El objeto ya está recogido");
                }
            }
            else
            {
                Debug.LogWarning("✗ El objeto '" + hit.collider.name + "' NO tiene el componente ObjetoRecogible.cs");
                Debug.LogWarning("Asegúrate de agregar el script ObjetoRecogible.cs a la piedra");
            }
        }
        else
        {
            Debug.Log("✗ Raycast NO impactó con ningún objeto");
            Debug.Log("Asegúrate de:");
            Debug.Log("  1. Estar mirando directamente a la piedra");
            Debug.Log("  2. Estar a menos de " + distanciaRecogida + " metros");
            Debug.Log("  3. Que la piedra tenga un Collider");
        }
    }

    /// <summary>
    /// Recoge el objeto especificado
    /// </summary>
    void RecogerObjeto(ObjetoRecogible objeto)
    {
        objetoActual = objeto;
        objetoRecogidoTransform = objeto.transform;

        // Llamar al método de recoger del objeto
        objeto.Recoger();

        // Informar al sistema de lanzamiento que tenemos un objeto
        if (sistemaLanzamiento != null)
        {
            sistemaLanzamiento.EstablecerObjetoActual(objeto);
        }

        Debug.Log("Objeto recogido: " + objeto.name);
    }

    /// <summary>
    /// Actualiza la posición del objeto recogido
    /// </summary>
    void ActualizarPosicionObjeto()
    {
        if (objetoRecogidoTransform != null)
        {
            // Calcular posición en el espacio mundial
            Vector3 posicionMundial = camaraJugador.transform.position
                                    + camaraJugador.transform.right * posicionSostener.x
                                    + camaraJugador.transform.up * posicionSostener.y
                                    + camaraJugador.transform.forward * posicionSostener.z;

            objetoRecogidoTransform.position = posicionMundial;

            // Opcional: rotar el objeto con la cámara
            objetoRecogidoTransform.rotation = camaraJugador.transform.rotation;
        }
    }

    /// <summary>
    /// Limpia la referencia del objeto actual (llamado después de lanzar)
    /// </summary>
    public void LimpiarObjetoActual()
    {
        objetoActual = null;
        objetoRecogidoTransform = null;
    }

    /// <summary>
    /// Verifica si actualmente hay un objeto recogido
    /// </summary>
    public bool TieneObjeto()
    {
        return objetoActual != null;
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        if (camaraJugador == null) return;

        // Dibujar la línea de detección
        Gizmos.color = Color.yellow;
        Vector3 origen = camaraJugador.transform.position;
        Vector3 fin = origen + camaraJugador.transform.forward * distanciaRecogida;
        Gizmos.DrawLine(origen, fin);

        // Dibujar una esfera en la posición donde se sostiene el objeto
        Gizmos.color = Color.green;
        Vector3 posicionSostenerMundial = camaraJugador.transform.position
                                        + camaraJugador.transform.right * posicionSostener.x
                                        + camaraJugador.transform.up * posicionSostener.y
                                        + camaraJugador.transform.forward * posicionSostener.z;
        Gizmos.DrawWireSphere(posicionSostenerMundial, 0.2f);
    }
}