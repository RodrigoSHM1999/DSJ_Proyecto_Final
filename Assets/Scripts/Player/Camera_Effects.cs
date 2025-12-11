using UnityEngine;

/// <summary>
/// Efectos de cámara: Head Bobbing y Sonidos de Pasos
/// </summary>
public class CameraEffects : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerControllerSimple playerScript;

    [Header("Head Bobbing")]
    public bool activarBobbing = true;
    public float frecuenciaCaminar = 1.8f;
    public float frecuenciaCorrer = 2.5f;
    public float frecuenciaAgachado = 1.2f;
    public float amplitudCaminar = 0.05f;
    public float amplitudCorrer = 0.10f;
    public float amplitudAgachado = 0.03f;

    [Header("Sonido de Pasos")]
    public AudioSource audioSource;
    public AudioClip sonidoPaso; // Solo un sonido
    public float volumenPasos = 0.5f;
    public float tiempoEntrePasosCaminar = 0.5f;  // Segundos entre pasos al caminar
    public float tiempoEntrePasosCorrer = 0.3f;    // Segundos entre pasos al correr
    public float tiempoEntrePasosAgachado = 0.7f;  // Segundos entre pasos agachado

    private float tiempoBob = 0f;
    private Vector3 posicionOriginal;
    private CharacterController controller;
    private float tiempoUltimoPaso = 0f;

    void Start()
    {
        posicionOriginal = transform.localPosition;

        if (playerScript != null)
        {
            controller = playerScript.GetComponent<CharacterController>();
        }

        if (audioSource != null)
        {
            audioSource.volume = volumenPasos;
            audioSource.loop = false;
        }
    }

    void Update()
    {
        if (controller == null) return;

        // Obtener input de movimiento
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");
        bool hayInput = Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f;

        // Verificar si está en el suelo y moviéndose
        bool estaMoviendo = hayInput && controller.isGrounded;

        if (estaMoviendo)
        {
            if (activarBobbing)
            {
                AplicarHeadBobbing();
            }
            ReproducirPasos();
        }
        else
        {
            // Volver suavemente a la posición original
            tiempoBob = 0f;
            if (activarBobbing)
            {
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    posicionOriginal,
                    Time.deltaTime * 8f
                );
            }
        }
    }

    void AplicarHeadBobbing()
    {
        // Detectar estado del jugador
        bool estaCorriendo = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0;
        bool estaAgachado = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        // Configurar parámetros según estado
        float frecuencia, amplitud;

        if (estaAgachado)
        {
            frecuencia = frecuenciaAgachado;
            amplitud = amplitudAgachado;
        }
        else if (estaCorriendo)
        {
            frecuencia = frecuenciaCorrer;
            amplitud = amplitudCorrer;
        }
        else
        {
            frecuencia = frecuenciaCaminar;
            amplitud = amplitudCaminar;
        }

        // Incrementar tiempo bob
        tiempoBob += Time.deltaTime * frecuencia * 5f;

        // Calcular offsetY (arriba/abajo)
        float offsetY = Mathf.Sin(tiempoBob) * amplitud;

        // Calcular offsetX (izquierda/derecha)
        float offsetX = Mathf.Cos(tiempoBob * 0.5f) * amplitud * 0.3f;

        // Aplicar a la posición local de la cámara
        Vector3 nuevaPosicion = posicionOriginal;
        nuevaPosicion.y += offsetY;
        nuevaPosicion.x += offsetX;

        transform.localPosition = nuevaPosicion;
    }

    void ReproducirPasos()
    {
        if (audioSource == null || sonidoPaso == null) return;

        // Detectar estado del jugador
        bool estaCorriendo = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0;
        bool estaAgachado = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        // Determinar intervalo según estado
        float intervalo;
        if (estaAgachado)
        {
            intervalo = tiempoEntrePasosAgachado;
        }
        else if (estaCorriendo)
        {
            intervalo = tiempoEntrePasosCorrer;
        }
        else
        {
            intervalo = tiempoEntrePasosCaminar;
        }

        // Reproducir si ha pasado suficiente tiempo
        if (Time.time - tiempoUltimoPaso >= intervalo)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Variación sutil
            audioSource.PlayOneShot(sonidoPaso, volumenPasos);
            tiempoUltimoPaso = Time.time;
        }
    }
}