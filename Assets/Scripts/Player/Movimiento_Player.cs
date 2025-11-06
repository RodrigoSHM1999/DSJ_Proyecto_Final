using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Movimiento_Player : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 5f;

    [Header("Mouse Look")]
    public float sensibilidadMouse = 2f;
    public Transform camaraJugador;

    private Rigidbody rb;
    private float rotacionX = 0f;
    private bool estaEnSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Evita que la cápsula ruede
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MirarAlrededor();

        // Saltar solo si está en el suelo
        if (estaEnSuelo && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        MoverJugador();
    }

    void MoverJugador()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Movimiento relativo a la rotación del jugador
        Vector3 direccion = transform.right * x + transform.forward * z;

        // Mantener velocidad vertical (gravedad)
        Vector3 velocidadActual = rb.velocity;
        Vector3 nuevaVelocidad = direccion * velocidad;
        nuevaVelocidad.y = velocidadActual.y;

        rb.velocity = nuevaVelocidad;

        // Detectar si está en el suelo usando colisiones
        RaycastHit hit;
        estaEnSuelo = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, 1.1f);
    }

    void MirarAlrededor()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -90f, 90f);

        camaraJugador.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
