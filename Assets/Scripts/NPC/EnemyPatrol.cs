using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Configuración de Patrullaje")]
    [Tooltip("Arrastra aquí el GameObject que contiene los waypoints")]
    public Transform waypointGroup;
    
    [Tooltip("Distancia mínima para considerar que llegó al waypoint")]
    public float distanciaLlegada = 0.5f;
    
    [Tooltip("Tiempo de espera en cada waypoint (en segundos)")]
    public float tiempoEspera = 2f;
    
    [Header("Información (Solo lectura)")]
    [SerializeField] private int waypointActual = 0;
    [SerializeField] private bool esperando = false;
    
    private NavMeshAgent agente;
    private Transform[] waypoints;
    private float tiempoEsperaActual = 0f;

    void Start()
    {
        // Obtener el NavMeshAgent del enemigo
        agente = GetComponent<NavMeshAgent>();
        
        if (agente == null)
        {
            Debug.LogError("¡No se encontró NavMeshAgent! Agrega uno al fantasma.");
            enabled = false;
            return;
        }
        
        // Verificar que tenemos waypoints
        if (waypointGroup == null)
        {
            Debug.LogError("¡No asignaste el WaypointGroup! Arrástralo en el Inspector.");
            enabled = false;
            return;
        }
        
        // Obtener todos los waypoints hijos
        int cantidadWaypoints = waypointGroup.childCount;
        
        if (cantidadWaypoints == 0)
        {
            Debug.LogError("¡El WaypointGroup no tiene waypoints hijos!");
            enabled = false;
            return;
        }
        
        waypoints = new Transform[cantidadWaypoints];
        
        for (int i = 0; i < cantidadWaypoints; i++)
        {
            waypoints[i] = waypointGroup.GetChild(i);
        }
        
        Debug.Log($"Sistema de patrullaje iniciado con {waypoints.Length} waypoints");
        
        // Ir al primer waypoint
        IrAlSiguienteWaypoint();
    }

    void Update()
    {
        // Si estamos esperando, contar el tiempo
        if (esperando)
        {
            tiempoEsperaActual += Time.deltaTime;
            
            if (tiempoEsperaActual >= tiempoEspera)
            {
                esperando = false;
                tiempoEsperaActual = 0f;
                IrAlSiguienteWaypoint();
            }
            return;
        }
        
        // Verificar si llegamos al waypoint
        if (!agente.pathPending && agente.remainingDistance <= distanciaLlegada)
        {
            // Llegamos al waypoint, esperar antes de continuar
            esperando = true;
            Debug.Log($"Llegué al waypoint {waypointActual + 1}. Esperando...");
        }
    }
    
    void IrAlSiguienteWaypoint()
    {
        if (waypoints.Length == 0) return;
        
        // Ir al waypoint actual
        agente.SetDestination(waypoints[waypointActual].position);
        Debug.Log($"Yendo hacia waypoint {waypointActual + 1}");
        
        // Avanzar al siguiente waypoint (circularmente)
        waypointActual = (waypointActual + 1) % waypoints.Length;
    }
    
    // Función auxiliar para visualizar los waypoints en el editor
    void OnDrawGizmos()
    {
        if (waypointGroup == null) return;
        
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < waypointGroup.childCount; i++)
        {
            Transform waypoint = waypointGroup.GetChild(i);
            
            // Dibujar esfera en cada waypoint
            Gizmos.DrawWireSphere(waypoint.position, 0.5f);
            
            // Dibujar línea al siguiente waypoint
            Transform siguiente = waypointGroup.GetChild((i + 1) % waypointGroup.childCount);
            Gizmos.DrawLine(waypoint.position, siguiente.position);
        }
    }
}