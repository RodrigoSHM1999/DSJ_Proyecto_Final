using UnityEngine;

/// <summary>
/// Controlador SIMPLE del jugador FPS
/// Movimiento básico: caminar, correr, saltar, agacharse
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerControllerSimple : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 8f;
    public float velocidadAgachado = 2.5f;

    [Header("Salto")]
    public float fuerzaSalto = 8f;
    public float gravedad = 20f;

    [Header("Agacharse")]
    public float alturaStand = 2f;
    public float alturaCrouch = 1f;

    private CharacterController controller;
    private float velocidadVertical = 0f;
    private bool agachado = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        alturaStand = controller.height;
    }

    void Update()
    {
        // Movimiento horizontal
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Velocidad según estado
        float velocidad = velocidadCaminar;

        // Correr
        if (Input.GetKey(KeyCode.LeftShift) && moveZ > 0 && !agachado)
        {
            velocidad = velocidadCorrer;
        }

        // Agacharse
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
        {
            agachado = !agachado;
            controller.height = agachado ? alturaCrouch : alturaStand;
        }

        if (agachado)
        {
            velocidad = velocidadAgachado;
        }

        // Calcular y aplicar movimiento horizontal
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * velocidad * Time.deltaTime);

        // Detección mejorada de suelo
        bool enSuelo = controller.isGrounded || velocidadVertical < 0.1f;

        // Salto
        if (Input.GetButtonDown("Jump") && enSuelo && !agachado)
        {
            velocidadVertical = fuerzaSalto;
        }

        // Aplicar gravedad
        if (controller.isGrounded && velocidadVertical < 0)
        {
            velocidadVertical = -2f;
        }
        else
        {
            velocidadVertical -= gravedad * Time.deltaTime;
        }

        // Aplicar movimiento vertical
        controller.Move(Vector3.up * velocidadVertical * Time.deltaTime);
    }
}