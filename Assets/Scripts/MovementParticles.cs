using UnityEngine;

public class MovementParticles : MonoBehaviour
{
    [Header("Configuración de Partículas")]
    [Tooltip("Sistema de partículas (se creará automáticamente si no existe)")]
    public ParticleSystem particulasMovimiento;
    
    [Tooltip("Velocidad mínima para activar partículas")]
    public float velocidadMinima = 0.1f;
    
    [Tooltip("Color de las partículas")]
    public Color colorParticulas = Color.cyan;
    
    [Header("Ajustes de Emisión")]
    [Tooltip("Cantidad de partículas por segundo al moverse")]
    public float tasaEmision = 30f;
    
    [Header("Tamaño de Partículas")]
    [Tooltip("Tamaño de las partículas (0.05 = muy pequeñas)")]
    public float tamañoParticulas = 0.05f;
    
    [Header("Debug")]
    [SerializeField] private float velocidadActual = 0f;
    [SerializeField] private bool particulasActivas = false;
    
    private ParticleSystem.EmissionModule emision;
    private Vector3 posicionAnterior;
    
    void Start()
    {
        // Si no hay sistema de partículas, crear uno
        if (particulasMovimiento == null)
        {
            CrearSistemaParticulas();
        }
        
        emision = particulasMovimiento.emission;
        posicionAnterior = transform.position;
        
        Debug.Log($"[MovementParticles] Sistema iniciado en {gameObject.name}");
        Debug.Log($"[MovementParticles] Velocidad mínima: {velocidadMinima}");
    }
    
    void Update()
    {
        // Calcular velocidad actual
        float distancia = Vector3.Distance(transform.position, posicionAnterior);
        velocidadActual = distancia / Time.deltaTime;
        posicionAnterior = transform.position;
        
        // Activar/desactivar partículas según velocidad
        if (velocidadActual >= velocidadMinima)
        {
            if (!particulasActivas)
            {
                emision.enabled = true;
                particulasActivas = true;
                Debug.Log($"[MovementParticles] ¡Partículas ACTIVADAS! Velocidad: {velocidadActual:F2}");
            }
            
            // Ajustar tasa de emisión según velocidad
            float factorVelocidad = Mathf.Clamp(velocidadActual / 5f, 0.5f, 2f);
            emision.rateOverTime = tasaEmision * factorVelocidad;
        }
        else
        {
            if (particulasActivas)
            {
                emision.enabled = false;
                particulasActivas = false;
            }
        }
    }
    
    void CrearSistemaParticulas()
    {
        // Crear GameObject hijo para las partículas
        GameObject particulasObj = new GameObject("Particulas_Movimiento");
        particulasObj.transform.SetParent(transform);
        particulasObj.transform.localPosition = Vector3.zero;
        
        // Agregar ParticleSystem
        particulasMovimiento = particulasObj.AddComponent<ParticleSystem>();
        
        // Configurar el sistema de partículas
        var main = particulasMovimiento.main;
        main.startLifetime = 0.4f;
        main.startSpeed = 1.5f;
        main.startSize = tamañoParticulas; // Usar el tamaño configurable
        main.startColor = colorParticulas;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;
        
        // Configurar forma de emisión (esfera pequeña)
        var shape = particulasMovimiento.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        
        // Configurar color sobre tiempo de vida (fade out)
        var colorOverLifetime = particulasMovimiento.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(colorParticulas, 0.0f), 
                new GradientColorKey(colorParticulas, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        
        colorOverLifetime.color = gradient;
        
        // Configurar tamaño sobre tiempo de vida (se hacen más pequeñas)
        var sizeOverLifetime = particulasMovimiento.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 1.0f);
        curve.AddKey(1.0f, 0.2f);
        
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        
        // Configurar emisión
        var emission = particulasMovimiento.emission;
        emission.enabled = false; // Empieza desactivado
        emission.rateOverTime = tasaEmision;
        
        // Configurar renderer
        var renderer = particulasMovimiento.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        Debug.Log($"[MovementParticles] ✅ Sistema creado con tamaño: {tamañoParticulas}");
    }
    
    // Función para obtener la velocidad actual (útil para el UI)
    public float ObtenerVelocidadActual()
    {
        return velocidadActual;
    }
    
    // Función para forzar activación (útil para debug)
    public void ForzarActivacion()
    {
        if (particulasMovimiento != null)
        {
            emision.enabled = true;
            Debug.Log("[MovementParticles] ¡Partículas forzadas!");
        }
    }
}