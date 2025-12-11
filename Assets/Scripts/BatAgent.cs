using UnityEngine;

public class BatAgent : MonoBehaviour
{
    [Header("Parámetros Individuales (BA)")]
    public float minFreq = 0f;
    public float maxFreq = 2f;
    public float frequency;
    public float loudness = 0.5f;
    public float pulseRate = 0.5f;

    [Header("Estado Físico")]
    public Vector3 velocity;
    public float fitness;
    public float maxSpeed = 5f;

    // Para evitar choques (Flocking - Separation)
    private float separationRadius = 2.5f;

    public void Initialize(BatController controller)
    {
        // Velocidad inicial aleatoria
        velocity = Random.insideUnitSphere * maxSpeed;
        loudness = Random.Range(0.5f, 1.0f);
        pulseRate = Random.Range(0.1f, 0.5f);
    }

    public void UpdateFitness(Vector3 targetPos)
    {
        // En nuestro caso, Fitness = Distancia (Menos es mejor)
        fitness = Vector3.Distance(transform.position, targetPos);
    }

    void Update()
    {
        // --- AQUÍ FUSIONAMOS EL ALGORITMO CON EL MOVIMIENTO VISUAL ---

        // 1. Aplicar Anticolisión (Separación simple)
        // Esto evita que todos se fusionen en un solo punto
        Vector3 separation = CalculateSeparation();
        velocity += separation * Time.deltaTime * 2.0f;

        // 2. Limitar velocidad máxima (para que no se teletransporte)
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // 3. Mover al objeto (Integration)
        transform.position += velocity * Time.deltaTime;

        // 4. Rotar hacia donde va (Visual)
        if (velocity != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    // Lógica simple de separación (Flocking)
    Vector3 CalculateSeparation()
    {
        Vector3 separationForce = Vector3.zero;
        // Buscamos colisionadores cercanos en la capa "Default" o la que uses
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius);

        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject != this.gameObject && neighbor.GetComponent<BatAgent>())
            {
                Vector3 push = transform.position - neighbor.transform.position;
                separationForce += push.normalized / push.magnitude; // Más fuerte si está más cerca
            }
        }
        return separationForce;
    }
}