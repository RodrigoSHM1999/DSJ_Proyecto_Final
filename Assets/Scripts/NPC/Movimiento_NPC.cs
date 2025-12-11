using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA del Fantasma - VERSIÓN BASE
/// RF05: Patrullaje
/// RF06: Detección por visión y proximidad
/// RF07: Persecución en tiempo real
/// 
/// ESCALABLE: Preparado para agregar RF08, RF09, RF10 después
/// </summary>
public class IA_Fantasma_Base : MonoBehaviour
{
    [Header("=== REFERENCIAS OBLIGATORIAS ===")]
    [Tooltip("⚠️ CRÍTICO: Arrastra aquí el GameObject del Player")]
    public Transform player;

    [Header("=== PATRULLAJE (RF05) ===")]
    [Tooltip("Puntos de patrulla que seguirá el fantasma")]
    public Transform[] puntosPatrulla;

    [Tooltip("Velocidad durante patrullaje")]
    public float velocidadPatrulla = 2f;

    [Tooltip("Tiempo de espera en cada punto")]
    public float tiempoEsperaEnPunto = 2f;

    [Header("=== DETECCIÓN (RF06) ===")]
    [Tooltip("Distancia máxima para ver al jugador")]
    public float rangoVision = 15f;

    [Tooltip("Ángulo de visión del fantasma (90 = cono frontal)")]
    public float anguloVision = 90f;

    [Tooltip("Distancia para detectar sin necesidad de ver (proximidad)")]
    public float rangoProximidad = 8f;

    [Tooltip("Capas que bloquean la visión (selecciona Wall, Ground, etc)")]
    public LayerMask capasObstaculos;

    [Header("=== PERSECUCIÓN (RF07) ===")]
    [Tooltip("Velocidad durante persecución - DEBE SER > velocidad correr del player")]
    public float velocidadPersecucion = 5.5f;

    [Tooltip("Distancia mínima para atrapar al jugador")]
    public float distanciaCaptura = 1.5f;

    [Header("=== AUDIO (RF13) ===")]
    [Tooltip("AudioSource para música de persecución (opcional)")]
    public AudioSource musicaPersecucion;

    [Header("=== ESTADO ACTUAL (SOLO LECTURA) ===")]
    [Tooltip("Estado actual del fantasma")]
    public EstadoFantasma estadoActual = EstadoFantasma.Patrullando;

    [Header("=== DEBUG ===")]
    [Tooltip("Mostrar rangos en Scene view")]
    public bool mostrarDebug = true;

    // Estados del fantasma
    public enum EstadoFantasma
    {
        Patrullando,    // RF05: Siguiendo ruta de patrulla
        Persiguiendo    // RF07: Persiguiendo al jugador en tiempo real
    }

    // Componentes
    private NavMeshAgent agent;
    private Animator anim;

    // Control de patrullaje
    private int indicePuntoActual = 0;
    private bool esperandoEnPunto = false;

    void Start()
    {
        // Obtener componentes
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // ⚠️ VALIDACIÓN CRÍTICA
        if (player == null)
        {
            Debug.LogError("❌ ERROR CRÍTICO: No se asignó el Player en el Inspector del fantasma '" + gameObject.name + "'");
            Debug.LogError("→ Solución: Arrastra el GameObject del Player al campo 'Player' en el Inspector");
            enabled = false; // Desactivar script para evitar errores
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("❌ ERROR: El fantasma NO está sobre el NavMesh. Verifica que el NavMesh esté bakeado.");
            enabled = false;
            return;
        }

        // Configurar velocidad inicial
        agent.speed = velocidadPatrulla;

        // Iniciar patrullaje
        if (puntosPatrulla.Length > 0)
        {
            IrASiguientePuntoPatrulla();
            Debug.Log("✅ Fantasma iniciado - Comenzando patrullaje");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay puntos de patrulla. El fantasma solo perseguirá si detecta al player.");
        }

        // Detener música de persecución al inicio
        if (musicaPersecucion != null)
        {
            musicaPersecucion.Stop();
        }
    }

    void Update()
    {
        // Debug: Mostrar estado cada medio segundo
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"📊 Estado: {estadoActual} | Velocidad actual: {agent.velocity.magnitude:F2} m/s");
        }

        // Máquina de estados simple
        switch (estadoActual)
        {
            case EstadoFantasma.Patrullando:
                ActualizarPatrullaje();
                break;

            case EstadoFantasma.Persiguiendo:
                ActualizarPersecucion();
                break;
        }

        // Actualizar animaciones si existen
        ActualizarAnimaciones();
    }

    #region PATRULLAJE (RF05)

    /// <summary>
    /// RF05: Lógica de patrullaje por puntos
    /// </summary>
    void ActualizarPatrullaje()
    {
        // CRÍTICO: Solo ejecutar si está en estado Patrullando
        // Evita que vuelva a detectar cuando ya está persiguiendo
        if (estadoActual != EstadoFantasma.Patrullando)
        {
            return;
        }

        // Verificar constantemente si puede detectar al jugador
        if (PuedeDetectarJugador())
        {
            IniciarPersecucion();
            return;
        }

        // Si está esperando en un punto, no hacer nada más
        if (esperandoEnPunto) return;

        // Si llegó al punto de patrulla actual
        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            StartCoroutine(EsperarEnPuntoPatrulla());
        }
    }

    void IrASiguientePuntoPatrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        agent.speed = velocidadPatrulla;
        agent.SetDestination(puntosPatrulla[indicePuntoActual].position);
        agent.stoppingDistance = 0.5f;

        // Avanzar al siguiente punto (circular)
        indicePuntoActual = (indicePuntoActual + 1) % puntosPatrulla.Length;
    }

    IEnumerator EsperarEnPuntoPatrulla()
    {
        esperandoEnPunto = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(tiempoEsperaEnPunto);

        agent.isStopped = false;
        IrASiguientePuntoPatrulla();
        esperandoEnPunto = false;
    }

    #endregion

    #region DETECCIÓN (RF06)

    /// <summary>
    /// RF06: Sistema de detección por visión y proximidad
    /// Verifica: distancia, ángulo y línea de visión (sin obstáculos)
    /// </summary>
    bool PuedeDetectarJugador()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

        // DETECCIÓN POR PROXIMIDAD: Si está muy cerca, detecta automáticamente
        if (distanciaAlJugador <= rangoProximidad)
        {
            return true;
        }

        // DETECCIÓN POR VISIÓN: Dentro del rango de visión
        if (distanciaAlJugador <= rangoVision)
        {
            // Calcular dirección hacia el jugador
            Vector3 direccionAlJugador = (player.position - transform.position).normalized;

            // Calcular ángulo entre donde mira el fantasma y donde está el jugador
            float angulo = Vector3.Angle(transform.forward, direccionAlJugador);

            // Si el jugador está dentro del cono de visión
            if (angulo <= anguloVision / 2f)
            {
                // Verificar que no haya obstáculos entre el fantasma y el jugador
                if (TieneLineaDeVisionHaciaJugador())
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica si hay línea de visión directa al jugador (sin paredes/obstáculos)
    /// </summary>
    bool TieneLineaDeVisionHaciaJugador()
    {
        // Punto de origen: ojos del fantasma (altura)
        Vector3 origenRayo = transform.position + Vector3.up * 1.5f;

        // Dirección hacia el jugador
        Vector3 direccion = player.position - origenRayo;
        float distancia = direccion.magnitude;

        // Lanzar raycast para ver si hay obstáculos
        if (Physics.Raycast(origenRayo, direccion.normalized, out RaycastHit hit, distancia, capasObstaculos))
        {
            // Si el raycast golpeó algo antes de llegar al jugador, no hay visión
            return false;
        }

        // No hay obstáculos, hay línea de visión clara
        return true;
    }

    #endregion

    #region PERSECUCIÓN (RF07)

    /// <summary>
    /// RF07: Inicia la persecución del jugador
    /// </summary>
    void IniciarPersecucion()
    {
        estadoActual = EstadoFantasma.Persiguiendo;

        // IMPORTANTE: Cancelar cualquier estado de patrullaje
        esperandoEnPunto = false;
        StopAllCoroutines(); // Detener espera en puntos

        // CRÍTICO: Resetear completamente el NavMeshAgent
        agent.ResetPath(); // Borrar ruta anterior
        agent.isStopped = false;

        // Configurar persecución
        agent.speed = velocidadPersecucion;
        agent.stoppingDistance = distanciaCaptura;

        // CRÍTICO: Proyectar posición del player al NavMesh
        NavMeshHit hit;
        Vector3 destinoInicial;

        if (NavMesh.SamplePosition(player.position, out hit, 10f, NavMesh.AllAreas))
        {
            destinoInicial = hit.position;
            Debug.Log($"   → Destino proyectado al NavMesh: {destinoInicial}");
        }
        else
        {
            destinoInicial = player.position;
            Debug.LogWarning($"   ⚠️ No se pudo proyectar, usando posición directa");
        }

        agent.SetDestination(destinoInicial);

        // Activar música de persecución
        if (musicaPersecucion != null && !musicaPersecucion.isPlaying)
        {
            musicaPersecucion.Play();
        }

        Debug.Log("🔴 ¡Fantasma detectó al jugador! Iniciando persecución...");
        Debug.Log($"   → Player en: {player.position}");
        Debug.Log($"   → Velocidad aumentada a: {agent.speed}");
    }

    /// <summary>
    /// RF07: Persecución en TIEMPO REAL
    /// Actualiza el destino cada frame para seguir al jugador constantemente
    /// </summary>
    void ActualizarPersecucion()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

        // CRÍTICO: Proyectar posición del player al NavMesh
        // Esto soluciona el problema cuando el player está fuera del NavMesh
        NavMeshHit hit;
        Vector3 destinoFinal;
        bool destinoValido = false;

        if (NavMesh.SamplePosition(player.position, out hit, 5f, NavMesh.AllAreas))
        {
            // Encontró punto válido en NavMesh cerca del player
            destinoFinal = hit.position;
            destinoValido = true;
        }
        else
        {
            // Si no encuentra cerca, buscar en radio mayor
            if (NavMesh.SamplePosition(player.position, out hit, 10f, NavMesh.AllAreas))
            {
                destinoFinal = hit.position;
                destinoValido = true;
            }
            else
            {
                // Último intento: usar posición directa
                destinoFinal = player.position;
                Debug.LogWarning($"⚠️ No se encontró NavMesh cerca del player, usando posición directa");
            }
        }

        // Establecer destino proyectado
        agent.SetDestination(destinoFinal);

        // Debug cada 60 frames (1 segundo aprox)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"🏃 PERSECUCIÓN:");
            Debug.Log($"   Distancia: {distanciaAlJugador:F1}m");
            Debug.Log($"   Player en: {player.position}");
            Debug.Log($"   Destino proyectado: {destinoFinal}");
            Debug.Log($"   Destino válido: {destinoValido}");
            Debug.Log($"   HasPath: {agent.hasPath}");
            Debug.Log($"   Velocidad: {agent.velocity.magnitude:F1}");
        }

        // Si atrapó al jugador
        if (distanciaAlJugador <= distanciaCaptura)
        {
            AtraparJugador();
            return;
        }

        // Si el jugador se alejó mucho y ya no puede verlo, volver a patrullar
        if (distanciaAlJugador > rangoVision * 2f && !PuedeDetectarJugador())
        {
            VolverAPatrulla();
        }
    }

    /// <summary>
    /// Cuando el fantasma atrapa al jugador
    /// </summary>
    void AtraparJugador()
    {
        Debug.Log("💀 ¡JUGADOR ATRAPADO!");

        // 🔮 AQUÍ SE CONECTARÁ CON:
        // - RF17: Sistema de vidas
        // - RF18: Screamer
        // - RF19: Reaparición

        // Por ahora, volver a patrullar
        VolverAPatrulla();
    }

    /// <summary>
    /// Vuelve al estado de patrullaje
    /// </summary>
    void VolverAPatrulla()
    {
        estadoActual = EstadoFantasma.Patrullando;
        agent.speed = velocidadPatrulla;
        agent.stoppingDistance = 0.5f;

        // Detener música de persecución
        if (musicaPersecucion != null && musicaPersecucion.isPlaying)
        {
            musicaPersecucion.Stop();
        }

        // Ir al punto de patrulla más cercano
        if (puntosPatrulla.Length > 0)
        {
            IrAPuntoMasCercano();
        }

        Debug.Log("✅ Fantasma vuelve a patrullar");
    }

    void IrAPuntoMasCercano()
    {
        float distanciaMinima = Mathf.Infinity;
        int indiceMinimo = 0;

        // Encontrar el punto de patrulla más cercano
        for (int i = 0; i < puntosPatrulla.Length; i++)
        {
            float distancia = Vector3.Distance(transform.position, puntosPatrulla[i].position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                indiceMinimo = i;
            }
        }

        indicePuntoActual = indiceMinimo;
        IrASiguientePuntoPatrulla();
    }

    #endregion

    #region ANIMACIONES

    /// <summary>
    /// Actualiza los parámetros del Animator si existe
    /// </summary>
    void ActualizarAnimaciones()
    {
        if (anim == null) return;

        // Calcular velocidad local del agente
        Vector3 velocidadLocal = transform.InverseTransformDirection(agent.velocity);

        // Normalizar velocidad (0 a 1)
        float velX = velocidadLocal.x / agent.speed;
        float velY = velocidadLocal.z / agent.speed;

        // Actualizar parámetros del Animator
        anim.SetFloat("VelX", velX, 0.1f, Time.deltaTime);
        anim.SetFloat("VelY", velY, 0.1f, Time.deltaTime);
    }

    #endregion

    #region DEBUG Y VISUALIZACIÓN

    void OnDrawGizmosSelected()
    {
        if (!mostrarDebug) return;

        // RANGO DE PROXIMIDAD (verde) - Detecta sin necesidad de ver
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, rangoProximidad);

        // RANGO DE VISIÓN (amarillo) - Alcance máximo de detección visual
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        // CONO DE VISIÓN (azul) - Ángulo de detección
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;

            // Líneas del cono de visión
            Vector3 direccionIzquierda = Quaternion.Euler(0, -anguloVision / 2f, 0) * transform.forward;
            Vector3 direccionDerecha = Quaternion.Euler(0, anguloVision / 2f, 0) * transform.forward;

            Gizmos.DrawRay(transform.position, direccionIzquierda * rangoVision);
            Gizmos.DrawRay(transform.position, direccionDerecha * rangoVision);
            Gizmos.DrawRay(transform.position, transform.forward * rangoVision);
        }

        // LÍNEA DE PERSECUCIÓN (rojo) - Hacia dónde va el fantasma
        if (Application.isPlaying && player != null && estadoActual == EstadoFantasma.Persiguiendo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // PUNTOS DE PATRULLA (cyan)
        if (puntosPatrulla != null && puntosPatrulla.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < puntosPatrulla.Length; i++)
            {
                if (puntosPatrulla[i] != null)
                {
                    Gizmos.DrawWireSphere(puntosPatrulla[i].position, 0.5f);

                    // Línea al siguiente punto
                    int siguienteIndice = (i + 1) % puntosPatrulla.Length;
                    if (puntosPatrulla[siguienteIndice] != null)
                    {
                        Gizmos.DrawLine(puntosPatrulla[i].position, puntosPatrulla[siguienteIndice].position);
                    }
                }
            }
        }
    }

    #endregion

    #region MÉTODOS PÚBLICOS PARA EXPANSIÓN FUTURA

    // 🔮 Estos métodos se usarán cuando implementes RF08, RF09, RF10

    /// <summary>
    /// RF08: Para detener el fantasma cuando el jugador lo mira
    /// IMPLEMENTAR DESPUÉS
    /// </summary>
    public void DetenerPorMirada()
    {
        // TODO: Implementar en RF08
        // agent.isStopped = true;
        // pausar música, etc.
    }

    /// <summary>
    /// RF08: Para reanudar el movimiento cuando el jugador deja de mirar
    /// IMPLEMENTAR DESPUÉS
    /// </summary>
    public void ReanudarMovimiento()
    {
        // TODO: Implementar en RF08
        // agent.isStopped = false;
        // reanudar música, etc.
    }

    /// <summary>
    /// RF10: Para que objetos lanzados notifiquen al fantasma
    /// IMPLEMENTAR DESPUÉS
    /// </summary>
    public void EscucharRuido(Vector3 posicionRuido)
    {
        // TODO: Implementar en RF10
        // Ir a investigar el punto de ruido
        Debug.Log($"👂 Ruido detectado en: {posicionRuido} (RF10 no implementado aún)");
    }

    #endregion
}