using UnityEngine;
using System.Collections.Generic;

public class BatController : MonoBehaviour
{
    [Header("Configuración del Enjambre")]
    public GameObject batPrefab;  // Arrastra aquí tu prefab del murciélago
    public Transform target;      // La esfera roja (meta)
    public int populationSize = 20;

    // Variables globales del algoritmo BA
    public Vector3 globalBestPosition;
    public float globalBestFitness = Mathf.Infinity;

    private List<BatAgent> bats = new List<BatAgent>();

    void Start()
    {
        // Inicializamos la mejor posición en el objetivo para empezar
        if (target != null) globalBestPosition = target.position;
        InitializePopulation();
    }

    void Update()
    {
        if (target == null) return;

        // El objetivo real es la mejor posición conocida
        // (En un caso real de optimización, esto no se sabría, se descubriría. 
        // Pero para la demo visual, el target atrae al enjambre).
        if (Vector3.Distance(globalBestPosition, target.position) > 0.1f)
        {
            globalBestPosition = target.position;
        }

        RunBatAlgorithm();
    }

    void InitializePopulation()
    {
        for (int i = 0; i < populationSize; i++)
        {
            // Crear murciélagos en una nube aleatoria
            Vector3 randomPos = transform.position + Random.insideUnitSphere * 10;
            GameObject obj = Instantiate(batPrefab, randomPos, Quaternion.identity);

            // Obtener el script del agente y guardarlo en la lista
            BatAgent bat = obj.GetComponent<BatAgent>();
            if (bat != null)
            {
                bat.Initialize(this); // Le pasamos el controlador para que sepa quién manda
                bats.Add(bat);
            }
        }
    }

    void RunBatAlgorithm()
    {
        foreach (var bat in bats)
        {
            // --- AQUÍ ESTÁ LA MAGIA MATEMÁTICA DEL PAPER ---

            // 1. Ajustar frecuencia (f = min + (max-min)*beta)
            bat.frequency = Random.Range(bat.minFreq, bat.maxFreq);

            // 2. Calcular nueva velocidad basada en el Mejor Global
            // v = v_old + (best_pos - current_pos) * frequency
            Vector3 difference = globalBestPosition - bat.transform.position;
            bat.velocity += difference * bat.frequency * Time.deltaTime;

            // 3. Búsqueda Local (Random walk si el pulso lo permite)
            // Esto simula la "locura" momentánea para buscar alrededor
            if (Random.value > bat.pulseRate)
            {
                bat.velocity += Random.insideUnitSphere * bat.loudness;
            }

            // 4. Actualizar Fitness y Lógica de Aceptación
            bat.UpdateFitness(target.position);

            // 5. Si este murciélago encontró un camino mejor que TODOS los demás:
            if (bat.fitness < globalBestFitness)
            {
                globalBestFitness = bat.fitness;
                globalBestPosition = bat.transform.position;

                // Reducir sonoridad (se vuelve más sigiloso/preciso)
                bat.loudness *= 0.9f;
                // Aumentar tasa de pulso
                bat.pulseRate = 1 - Mathf.Exp(-0.1f * Time.time);
            }
        }
    }
}