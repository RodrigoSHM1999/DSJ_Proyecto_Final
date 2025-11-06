using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovementUI : MonoBehaviour
{
    [Header("Referencias del Personaje")]
    [Tooltip("Arrastra aquí el GameObject del player")]
    public Transform personaje;
    
    [Header("Referencias UI - Velocidad")]
    [Tooltip("Texto que mostrará la velocidad")]
    public TextMeshProUGUI textoVelocidad;
    
    [Tooltip("Barra de velocidad (Image con tipo Filled)")]
    public Image barraVelocidad;
    
    [Tooltip("Velocidad máxima para la barra (100%)")]
    public float velocidadMaxima = 10f;
    
    [Header("Referencias UI - Dirección")]
    [Tooltip("Flecha que indica dirección (Image)")]
    public RectTransform flechaDireccion;
    
    [Tooltip("Texto que muestra la dirección")]
    public TextMeshProUGUI textoDireccion;
    
    [Header("Colores de la Barra")]
    public Color colorLento = Color.green;
    public Color colorMedio = Color.yellow;
    public Color colorRapido = Color.red;
    
    private Vector3 posicionAnterior;
    private Vector3 direccionMovimiento;
    private float velocidadActual;
    
    void Start()
    {
        if (personaje == null)
        {
            Debug.LogError("¡No asignaste el personaje! Arrástralo en el Inspector.");
            enabled = false;
            return;
        }
        
        posicionAnterior = personaje.position;
        
        // Configurar barra si existe
        if (barraVelocidad != null)
        {
            barraVelocidad.fillAmount = 0f;
            barraVelocidad.type = Image.Type.Filled;
            barraVelocidad.fillMethod = Image.FillMethod.Horizontal;
        }
        
        Debug.Log($"UI de movimiento iniciado para {personaje.name}");
    }
    
    void Update()
    {
        CalcularMovimiento();
        ActualizarUIVelocidad();
        ActualizarUIDireccion();
    }
    
    void CalcularMovimiento()
    {
        // Calcular velocidad
        Vector3 desplazamiento = personaje.position - posicionAnterior;
        velocidadActual = desplazamiento.magnitude / Time.deltaTime;
        
        // Calcular dirección
        if (desplazamiento.magnitude > 0.01f)
        {
            direccionMovimiento = desplazamiento.normalized;
        }
        
        posicionAnterior = personaje.position;
    }
    
    void ActualizarUIVelocidad()
    {
        // Actualizar texto de velocidad
        if (textoVelocidad != null)
        {
            textoVelocidad.text = $"Velocidad: {velocidadActual:F1} u/s";
        }
        
        // Actualizar barra de velocidad
        if (barraVelocidad != null)
        {
            float porcentaje = Mathf.Clamp01(velocidadActual / velocidadMaxima);
            barraVelocidad.fillAmount = porcentaje;
            
            // Cambiar color según velocidad
            if (porcentaje < 0.33f)
            {
                barraVelocidad.color = colorLento;
            }
            else if (porcentaje < 0.66f)
            {
                barraVelocidad.color = colorMedio;
            }
            else
            {
                barraVelocidad.color = colorRapido;
            }
        }
    }
    
    void ActualizarUIDireccion()
    {
        // Actualizar flecha de dirección
        if (flechaDireccion != null && direccionMovimiento.magnitude > 0.01f)
        {
            // Convertir dirección 3D a ángulo 2D
            float angulo = Mathf.Atan2(direccionMovimiento.z, direccionMovimiento.x) * Mathf.Rad2Deg;
            flechaDireccion.rotation = Quaternion.Euler(0, 0, angulo - 90f);
        }
        
        // Actualizar texto de dirección
        if (textoDireccion != null)
        {
            string direccionTexto = ObtenerDireccionCardinal(direccionMovimiento);
            textoDireccion.text = $"Dirección: {direccionTexto}";
        }
    }
    
    string ObtenerDireccionCardinal(Vector3 direccion)
    {
        if (direccion.magnitude < 0.01f)
        {
            return "Detenido";
        }
        
        float angulo = Mathf.Atan2(direccion.z, direccion.x) * Mathf.Rad2Deg;
        
        // Normalizar ángulo a 0-360
        if (angulo < 0) angulo += 360f;
        
        // Determinar dirección cardinal
        if (angulo >= 337.5f || angulo < 22.5f)
            return "Este →";
        else if (angulo >= 22.5f && angulo < 67.5f)
            return "Noreste ↗";
        else if (angulo >= 67.5f && angulo < 112.5f)
            return "Norte ↑";
        else if (angulo >= 112.5f && angulo < 157.5f)
            return "Noroeste ↖";
        else if (angulo >= 157.5f && angulo < 202.5f)
            return "Oeste ←";
        else if (angulo >= 202.5f && angulo < 247.5f)
            return "Suroeste ↙";
        else if (angulo >= 247.5f && angulo < 292.5f)
            return "Sur ↓";
        else
            return "Sureste ↘";
    }
    
    // Función pública para obtener velocidad (útil para otros scripts)
    public float ObtenerVelocidad()
    {
        return velocidadActual;
    }
}