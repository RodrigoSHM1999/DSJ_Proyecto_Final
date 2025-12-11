using UnityEngine;

/// <summary>
/// Controlador de linterna para el jugador
/// Permite activar/desactivar la luz con la tecla F
/// La linterna rota siguiendo la dirección de la cámara/mouse
/// RF03 - Sistema de linterna
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [Header("Configuración de Linterna")]
    [SerializeField] private Light flashlight; // Referencia al Spot Light
    [SerializeField] private Transform cameraTransform; // Referencia a la cámara principal
    [SerializeField] private KeyCode toggleKey = KeyCode.F; // Tecla para activar/desactivar
    [SerializeField] private bool startOn = true; // ¿Empieza encendida?

    [Header("Efectos de Sonido (Opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound; // Sonido al encender/apagar

    [Header("Estado")]
    [SerializeField] private bool isOn; // Estado actual de la linterna

    private void Start()
    {
        // Validar que tenemos la referencia a la luz
        if (flashlight == null)
        {
            Debug.LogError("FlashlightController: No se asignó el Spot Light en el Inspector!");
            return;
        }

        // Si no se asignó la cámara, intentar encontrar la Main Camera
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
                Debug.Log("FlashlightController: Cámara principal encontrada automáticamente");
            }
            else
            {
                Debug.LogWarning("FlashlightController: No se encontró la cámara principal. Asigna manualmente en el Inspector.");
            }
        }

        // Configurar estado inicial
        isOn = startOn;
        flashlight.enabled = isOn;

        Debug.Log($"Linterna inicializada. Estado: {(isOn ? "Encendida" : "Apagada")}");
    }

    private void LateUpdate()
    {
        // Sincronizar rotación de la linterna con la cámara
        if (cameraTransform != null && flashlight != null)
        {
            flashlight.transform.rotation = cameraTransform.rotation;
        }
    }

    private void Update()
    {
        // Detectar input para toggle
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }

    /// <summary>
    /// Alterna el estado de la linterna (encendida/apagada)
    /// </summary>
    public void ToggleFlashlight()
    {
        if (flashlight == null) return;

        isOn = !isOn;
        flashlight.enabled = isOn;

        // Reproducir sonido si está configurado
        PlayToggleSound();

        Debug.Log($"Linterna {(isOn ? "encendida" : "apagada")}");
    }

    /// <summary>
    /// Enciende la linterna
    /// </summary>
    public void TurnOn()
    {
        if (flashlight == null) return;

        isOn = true;
        flashlight.enabled = true;
        PlayToggleSound();

        Debug.Log("Linterna encendida");
    }

    /// <summary>
    /// Apaga la linterna
    /// </summary>
    public void TurnOff()
    {
        if (flashlight == null) return;

        isOn = false;
        flashlight.enabled = false;
        PlayToggleSound();

        Debug.Log("Linterna apagada");
    }

    /// <summary>
    /// Reproduce el sonido de toggle si está configurado
    /// </summary>
    private void PlayToggleSound()
    {
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }

    /// <summary>
    /// Obtiene el estado actual de la linterna
    /// </summary>
    public bool IsOn()
    {
        return isOn;
    }

    /// <summary>
    /// Establece el estado de la linterna sin reproducir sonido
    /// Útil para inicialización o eventos externos
    /// </summary>
    public void SetState(bool state)
    {
        if (flashlight == null) return;

        isOn = state;
        flashlight.enabled = state;
    }
}