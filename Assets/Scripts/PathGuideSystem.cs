using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class PathGuideSystem : MonoBehaviour
{
    [Header("Configuración de Jugador")]
    [Tooltip("Arrastra aquí el GameObject del jugador")]
    public Transform jugador;
    
    [Tooltip("Arrastra aquí el objetivo al que quieres llegar")]
    public Transform objetivo;
    
    [Header("Controles")]
    [Tooltip("Tecla para activar la guía")]
    public KeyCode teclaGuia = KeyCode.H;
    
    [Tooltip("Duración de la guía en segundos")]
    public float duracionGuia = 8f;
    
    [Header("Configuración Visual")]
    [Tooltip("Prefab de marcador (se creará automáticamente si está vacío)")]
    public GameObject prefabMarcador;
    
    [Tooltip("Separación entre marcadores")]
    public float separacionMarcadores = 1.5f;
    
    [Tooltip("Altura sobre el suelo")]
    public float alturaMarcador = 0.1f;
    
    [Tooltip("Color de la guía")]
    public Color colorGuia = new Color(0, 1, 1, 1); // Cyan por defecto
    
    [Tooltip("Tamaño de los marcadores")]
    public float tamañoMarcador = 0.5f;
    
    [Header("Efectos")]
    [Tooltip("¿Usar partículas en lugar de objetos?")]
    public bool usarParticulas = false;
    
    [Tooltip("Cantidad de partículas por marcador")]
    public int particulasPorMarcador = 15;
    
    [Header("Debug")]
    [SerializeField] private bool guiaActiva = false;
    [SerializeField] private int cantidadMarcadores = 0;
    
    private List<GameObject> marcadoresActivos = new List<GameObject>();
    private NavMeshPath caminoCalculado;
    private float tiempoActivacion;
  
    void Start()
    {
       
        if (jugador == null)
        {
            Debug.LogError("¡No asignaste el jugador! Arrastra el player en el Inspector.");
            enabled = false;
            return;
        }
        
        if (objetivo == null)
        {
            Debug.LogWarning("No hay objetivo asignado. Asigna uno para usar la guía.");
        }
        
        caminoCalculado = new NavMeshPath();
        
        Debug.Log($"[PathGuide] Sistema listo. Presiona '{teclaGuia}' para activar la guía.");
    }
    
    void Update()
    {
        // Detectar tecla de guía
        if (Input.GetKeyDown(teclaGuia))
        {
            if (objetivo == null)
            {
                Debug.LogWarning("¡No hay objetivo asignado!");
                return;
            }
            
            ActivarGuia();
        }
        
        // Desactivar guía después del tiempo
        if (guiaActiva && Time.time - tiempoActivacion >= duracionGuia)
        {
            DesactivarGuia();
        }
    }
    
    void ActivarGuia()
    {
        // Limpiar guía anterior si existe
        DesactivarGuia();
        
        // Calcular camino usando NavMesh
        bool caminoEncontrado = NavMesh.CalculatePath(
            jugador.position, 
            objetivo.position, 
            NavMesh.AllAreas, 
            caminoCalculado
        );
        
        if (!caminoEncontrado || caminoCalculado.corners.Length < 2)
        {
            Debug.LogWarning("No se pudo calcular un camino al objetivo.");
            return;
        }
        
        Debug.Log($"[PathGuide] ✅ Guía activada con {caminoCalculado.corners.Length} puntos clave.");
        
        // Crear marcadores a lo largo del camino
        CrearMarcadoresEnCamino();
        
        guiaActiva = true;
        tiempoActivacion = Time.time;
    }
    
    void CrearMarcadoresEnCamino()
    {
        Vector3[] esquinas = caminoCalculado.corners;
        
        // Recorrer cada segmento del camino
        for (int i = 0; i < esquinas.Length - 1; i++)
        {
            Vector3 inicio = esquinas[i];
            Vector3 fin = esquinas[i + 1];
            
            float distanciaSegmento = Vector3.Distance(inicio, fin);
            int numMarcadores = Mathf.CeilToInt(distanciaSegmento / separacionMarcadores);
            
            // Crear marcadores a lo largo del segmento
            for (int j = 0; j <= numMarcadores; j++)
            {
                float t = j / (float)numMarcadores;
                Vector3 posicion = Vector3.Lerp(inicio, fin, t);
                posicion.y = alturaMarcador; // Altura sobre el suelo
                
                CrearMarcador(posicion);
            }
        }
        
        cantidadMarcadores = marcadoresActivos.Count;
        Debug.Log($"[PathGuide] Creados {cantidadMarcadores} marcadores.");
    }
    
    void CrearMarcador(Vector3 posicion)
    {
        GameObject marcador;
        
        if (usarParticulas)
        {
            // Crear marcador con partículas
            marcador = new GameObject("GuiaMarcador_Particulas");
            marcador.transform.position = posicion;
            
            ParticleSystem ps = marcador.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.startLifetime = duracionGuia;
            main.startSpeed = 0.3f;
            main.startSize = 0.1f;
            main.startColor = colorGuia;
            main.maxParticles = particulasPorMarcador;
            main.loop = false;
            
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, particulasPorMarcador)
            });
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = tamañoMarcador * 0.5f;
            
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(colorGuia, 0.0f),
                    new GradientColorKey(colorGuia, 0.7f),
                    new GradientColorKey(colorGuia, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 0.2f),
                    new GradientAlphaKey(1.0f, 0.8f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            
            colorOverLifetime.color = gradient;
            
            // Configurar renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            // Intentar asignar un material que funcione
            Material particleMat = new Material(Shader.Find("Particles/Standard Unlit"));
            if (particleMat.shader == null)
            {
                particleMat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            }
            if (particleMat.shader != null)
            {
                particleMat.SetColor("_Color", colorGuia);
                renderer.material = particleMat;
            }
        }
        else
        {
            // Crear marcador con objeto (esfera brillante)
            if (prefabMarcador != null)
            {
                marcador = Instantiate(prefabMarcador, posicion, Quaternion.identity);
            }
            else
            {
                marcador = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marcador.name = "GuiaMarcador_Esfera";
                marcador.transform.position = posicion;
                marcador.transform.localScale = Vector3.one * tamañoMarcador;
                
                // Configurar material emisivo
                Renderer rend = marcador.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetColor("_Color", colorGuia);
                mat.SetColor("_EmissionColor", colorGuia * 2f);
                mat.EnableKeyword("_EMISSION");
                rend.material = mat;
                
                // Quitar colisión
                Destroy(marcador.GetComponent<Collider>());
            }
            
            // Agregar script de autodestrucción
            AutoDestroy autoDestroy = marcador.AddComponent<AutoDestroy>();
            autoDestroy.tiempoVida = duracionGuia;
        }
        
        marcadoresActivos.Add(marcador);
    }
    
    void DesactivarGuia()
    {
        // Destruir todos los marcadores
        foreach (GameObject marcador in marcadoresActivos)
        {
            if (marcador != null)
            {
                Destroy(marcador);
            }
        }
        
        marcadoresActivos.Clear();
        guiaActiva = false;
        cantidadMarcadores = 0;
    }
    
    void OnDrawGizmos()
    {
        // Visualizar el camino en el editor
        if (caminoCalculado != null && caminoCalculado.corners.Length > 1)
        {
            Gizmos.color = colorGuia;
            
            for (int i = 0; i < caminoCalculado.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(caminoCalculado.corners[i], caminoCalculado.corners[i + 1]);
                Gizmos.DrawWireSphere(caminoCalculado.corners[i], 0.3f);
            }
        }
    }
    
    // Función pública para cambiar objetivo dinámicamente
    public void CambiarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        Debug.Log($"[PathGuide] Objetivo cambiado a: {nuevoObjetivo.name}");
    }
}

// Script auxiliar para autodestruir objetos
public class AutoDestroy : MonoBehaviour
{
    public float tiempoVida = 5f;
    
    void Start()
    {
        Destroy(gameObject, tiempoVida);
    }
}