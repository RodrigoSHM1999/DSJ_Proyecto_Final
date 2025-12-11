using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

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

    [Tooltip("Material para las partículas (arrastra aquí un material con Default-Particle)")]
    public Material materialParticulas;

    [Tooltip("Separación entre marcadores")]
    public float separacionMarcadores = 1.5f;

    [Tooltip("Altura sobre el suelo")]
    public float alturaMarcador = 0.1f;

    [Tooltip("Color de la guía")]
    public Color colorGuia = new Color(0.8f, 0.9f, 1f, 1f);

    [Tooltip("Tamaño de los marcadores")]
    public float tamañoMarcador = 0.5f;

    [Header("Efectos")]
    [Tooltip("¿Usar partículas en lugar de objetos?")]
    public bool usarParticulas = true;

    [Tooltip("Cantidad de partículas por marcador")]
    public int particulasPorMarcador = 15;

    [Tooltip("Intensidad del brillo emisivo")]
    [Range(1f, 5f)]
    public float intensidadBrillo = 2.5f;

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
        if (Input.GetKeyDown(teclaGuia))
        {
            if (objetivo == null)
            {
                Debug.LogWarning("¡No hay objetivo asignado!");
                return;
            }

            ActivarGuia();
        }

        if (guiaActiva && Time.time - tiempoActivacion >= duracionGuia)
        {
            DesactivarGuia();
        }
    }

    void ActivarGuia()
    {
        DesactivarGuia();

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

        CrearMarcadoresEnCamino();

        guiaActiva = true;
        tiempoActivacion = Time.time;
    }

    void CrearMarcadoresEnCamino()
    {
        Vector3[] esquinas = caminoCalculado.corners;

        for (int i = 0; i < esquinas.Length - 1; i++)
        {
            Vector3 inicio = esquinas[i];
            Vector3 fin = esquinas[i + 1];

            float distanciaSegmento = Vector3.Distance(inicio, fin);
            int numMarcadores = Mathf.CeilToInt(distanciaSegmento / separacionMarcadores);

            for (int j = 0; j <= numMarcadores; j++)
            {
                float t = j / (float)numMarcadores;
                Vector3 posicion = Vector3.Lerp(inicio, fin, t);
                posicion.y = alturaMarcador;

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
            marcador = new GameObject("GuiaMarcador_Particulas");
            marcador.transform.position = posicion;

            ParticleSystem ps = marcador.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.25f);
            main.startColor = colorGuia;
            main.maxParticles = particulasPorMarcador;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.15f;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = tamañoMarcador * 0.25f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient gradient = new Gradient();
            Color colorBrillante = colorGuia * intensidadBrillo;
            colorBrillante.r = Mathf.Clamp01(colorBrillante.r);
            colorBrillante.g = Mathf.Clamp01(colorBrillante.g);
            colorBrillante.b = Mathf.Clamp01(colorBrillante.b);

            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(colorGuia, 0.0f),
                    new GradientColorKey(colorBrillante, 0.5f),
                    new GradientColorKey(colorGuia, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.0f, 0.0f),
                    new GradientAlphaKey(0.9f, 0.2f),
                    new GradientAlphaKey(0.9f, 0.8f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );

            colorOverLifetime.color = gradient;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0, new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(0.5f, 0.15f),
                new Keyframe(1, 0)
            ));

            // ✅ USAR MATERIAL ASIGNADO O CREAR UNO POR DEFECTO
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 10;

            if (materialParticulas != null)
            {
                // ✅ Si asignaste un material, úsalo
                renderer.material = materialParticulas;
                Debug.Log("✅ Usando material asignado en Inspector");
            }
            else
            {
                // Si no, crear uno básico
                Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
                if (mat.shader == null)
                {
                    mat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
                }
                mat.color = Color.white;
                renderer.material = mat;
                Debug.LogWarning("⚠️ No hay material asignado, usando shader por defecto (cuadrados)");
            }
        }
        else
        {
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

                Renderer rend = marcador.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));

                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                Color colorTranslucido = colorGuia;
                colorTranslucido.a = 0.6f;
                mat.SetColor("_Color", colorTranslucido);

                Color emisionColor = colorGuia * intensidadBrillo;
                mat.SetColor("_EmissionColor", emisionColor);
                mat.EnableKeyword("_EMISSION");

                rend.material = mat;
                Destroy(marcador.GetComponent<Collider>());
            }

            AutoDestroy autoDestroy = marcador.AddComponent<AutoDestroy>();
            autoDestroy.tiempoVida = duracionGuia;
        }

        marcadoresActivos.Add(marcador);
    }

    void DesactivarGuia()
    {
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

    public void CambiarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        Debug.Log($"[PathGuide] Objetivo cambiado a: {nuevoObjetivo.name}");
    }
}

public class AutoDestroy : MonoBehaviour
{
    public float tiempoVida = 5f;

    void Start()
    {
        Destroy(gameObject, tiempoVida);
    }
}