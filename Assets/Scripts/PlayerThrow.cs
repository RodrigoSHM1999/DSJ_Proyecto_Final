using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject piedraPrefab;
    public Transform puntoDeLanzamiento; // Un objeto vacío delante del jugador
    public LineRenderer lineRendererPrediccion;

    [Header("Configuración Lanzamiento")]
    public float fuerzaMinima = 10f;
    public float fuerzaMaxima = 30f;
    public float tiempoMaximoCarga = 2f;
    
    [Header("Configuración Predicción")]
    public int puntosDeLinea = 30;
    public float tiempoEntrePuntos = 0.1f;

    // Variables privadas
    private float fuerzaActual;
    private float tiempoDeCarga;
    private bool estaCargando = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Empezar a cargar
            estaCargando = true;
            tiempoDeCarga = 0f;
            lineRendererPrediccion.enabled = true;
        }

        if (estaCargando && Input.GetMouseButton(0))
        {
            // Sigue cargando
            tiempoDeCarga += Time.deltaTime;
            tiempoDeCarga = Mathf.Clamp(tiempoDeCarga, 0f, tiempoMaximoCarga);
            
            float t = tiempoDeCarga / tiempoMaximoCarga;
            fuerzaActual = Mathf.Lerp(fuerzaMinima, fuerzaMaxima, t);

            // TODO: Llamar a la función de predicción
            DibujarPrediccion(puntoDeLanzamiento.position, puntoDeLanzamiento.forward * fuerzaActual);

            // TODO: Actualizar UI (hablar con Joshua)
            // UIManager.Instancia.ActualizarBarraFuerza(t); 
        }

        if (estaCargando && Input.GetMouseButtonUp(0))
        {
            // Lanzar
            LanzarPiedra();
            estaCargando = false;
            lineRendererPrediccion.enabled = false;

            // TODO: Reiniciar UI y restar piedra (hablar con Joshua)
            // UIManager.Instancia.ActualizarBarraFuerza(0);
            // UIManager.Instancia.RestarPiedra(); 
        }
    }

    void LanzarPiedra()
    {
        Debug.Log("¡Lanzando con fuerza: " + fuerzaActual + "!");
        GameObject piedraObj = Instantiate(piedraPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
        Rigidbody rb = piedraObj.GetComponent<Rigidbody>();
        
        rb.AddForce(puntoDeLanzamiento.forward * fuerzaActual, ForceMode.Impulse);
    }

    void DibujarPrediccion(Vector3 posInicial, Vector3 velInicial)
    {
    lineRendererPrediccion.positionCount = puntosDeLinea;
    List<Vector3> puntos = new List<Vector3>();
    
    Vector3 posActual = posInicial;
    Vector3 velActual = velInicial;
    
    bool colisionDetectada = false;

    for (int i = 0; i < puntosDeLinea; i++)
    {
        // 1. Si ya chocamos en el paso anterior, solo repite el último punto.
        if (colisionDetectada)
        {
            puntos.Add(puntos[puntos.Count - 1]);
            continue;
        }

        // 2. Añade el punto actual
        puntos.Add(posActual);

        // 3. Calcula la posición del *siguiente* punto
        Vector3 posSiguiente = posActual + velActual * tiempoEntrePuntos;
        
        // 4. Aplica la gravedad a la velocidad (para el próximo bucle)
        velActual += Physics.gravity * tiempoEntrePuntos;

        // 5. ¡Raycast de detección!
        RaycastHit hit;
        // Lanza un rayo desde la posición actual a la siguiente posición
        if (Physics.Raycast(posActual, (posSiguiente - posActual).normalized, out hit, (posSiguiente - posActual).magnitude))
        {
            // ¡COLISIÓN!
            puntos.Add(hit.point); // Añade el punto exacto del golpe
            colisionDetectada = true; // Marca que hemos chocado
        }
        else
        {
            // No hay colisión, sigue normal
            posActual = posSiguiente;
        }
    }

    // 6. Cambia el color de TODA la línea según si hubo colisión
    if (colisionDetectada)
    {
        lineRendererPrediccion.startColor = Color.red;
        lineRendererPrediccion.endColor = Color.red;
    }
    else
    {
        lineRendererPrediccion.startColor = Color.green;
        lineRendererPrediccion.endColor = Color.green;
    }

    // 7. Asigna todos los puntos al LineRenderer
    lineRendererPrediccion.SetPositions(puntos.ToArray());
    }
}