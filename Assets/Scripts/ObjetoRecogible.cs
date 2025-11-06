using UnityEngine;

/// <summary>
/// Script para objetos que pueden ser recogidos y lanzados
/// Colocar este script en la piedra u otros objetos recogibles
/// </summary>
public class ObjetoRecogible : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    [Tooltip("Si está recogido actualmente")]
    public bool estaRecogido = false;

    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Verificar que tenga Rigidbody
        if (rb == null)
        {
            Debug.LogError("El objeto recogible necesita un Rigidbody!");
        }
    }

    /// <summary>
    /// Método llamado cuando el objeto es recogido
    /// </summary>
    public void Recoger()
    {
        estaRecogido = true;

        // Desactivar física mientras está recogido
        rb.isKinematic = true;
        rb.useGravity = false;

        // Desactivar colisión para que no choque con el jugador
        col.enabled = false;
    }

    /// <summary>
    /// Método llamado cuando el objeto es lanzado
    /// </summary>
    /// <param name="fuerza">Vector de fuerza a aplicar</param>
    public void Lanzar(Vector3 fuerza)
    {
        estaRecogido = false;

        // Reactivar física
        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;

        // Aplicar la fuerza
        rb.AddForce(fuerza, ForceMode.Impulse);
    }

    /// <summary>
    /// Obtiene la masa del objeto
    /// </summary>
    public float ObtenerMasa()
    {
        return rb != null ? rb.mass : 1f;
    }
}