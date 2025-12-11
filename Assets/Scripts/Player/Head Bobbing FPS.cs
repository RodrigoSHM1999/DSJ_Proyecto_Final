using UnityEngine;

/// <summary>
/// Control SIMPLE de cámara con mouse + suavizado
/// </summary>
public class MouseLookSimple : MonoBehaviour
{
    public float sensibilidad = 2f;
    public Transform cuerpoJugador;

    [Header("Suavizado")]
    public bool activarSuavizado = true;
    public float suavizado = 5f;

    private float rotacionX = 0f;
    private float rotacionXObjetivo = 0f;
    private float rotacionYObjetivo = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Input del mouse
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;

        if (activarSuavizado)
        {
            // Calcular rotación objetivo
            rotacionYObjetivo += mouseX;
            rotacionXObjetivo -= mouseY;
            rotacionXObjetivo = Mathf.Clamp(rotacionXObjetivo, -90f, 90f);

            // Interpolar suavemente hacia el objetivo
            rotacionX = Mathf.Lerp(rotacionX, rotacionXObjetivo, suavizado * Time.deltaTime);
            float rotacionY = Mathf.LerpAngle(cuerpoJugador.eulerAngles.y, rotacionYObjetivo, suavizado * Time.deltaTime);

            // Aplicar rotaciones
            transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
            cuerpoJugador.rotation = Quaternion.Euler(0f, rotacionY, 0f);
        }
        else
        {
            // Rotación directa sin suavizado
            cuerpoJugador.Rotate(Vector3.up * mouseX);

            rotacionX -= mouseY;
            rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);
            transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        }
    }
}