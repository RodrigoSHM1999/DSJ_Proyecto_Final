using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movimiento_NPC : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; // Arrastra aquí el transform del player

    [Header("Configuración")]
    public float rangoDeteccion = 10f; // Distancia para detectar al player
    public float rangoParada = 2f; // Distancia mínima al player

    [Header("Patrullaje")]
    public Transform[] puntosPatrulla; // Array de puntos de patrulla
    private int indicePuntoActual = 0;

    private NavMeshAgent agent;
    private Animator anim;

    // Estados
    private bool persiguiendoPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Comenzar patrullando si hay puntos
        if (puntosPatrulla.Length > 0)
        {
            IrASiguientePunto();
        }
    }

    void Update()
    {
        // Calcular distancia al player
        float distanciaAlPlayer = Vector3.Distance(transform.position, player.position);

        // DETECCIÓN: Si el player está cerca
        if (distanciaAlPlayer <= rangoDeteccion)
        {
            persiguiendoPlayer = true;

            // PERSECUCIÓN CONSTANTE: Actualizar destino cada frame
            agent.SetDestination(player.position);
            agent.stoppingDistance = rangoParada;
        }
        else
        {
            // Si el player se alejó, volver a patrullar
            if (persiguiendoPlayer)
            {
                persiguiendoPlayer = false;
                IrASiguientePunto();
            }

            // Verificar si llegó al punto de patrulla
            if (!agent.pathPending && agent.remainingDistance <= 0.5f)
            {
                IrASiguientePunto();
            }
        }

        // Animaciones (opcional)
        ActualizarAnimaciones();
    }

    void IrASiguientePunto()
    {
        if (puntosPatrulla.Length == 0) return;

        agent.SetDestination(puntosPatrulla[indicePuntoActual].position);
        agent.stoppingDistance = 0f;

        // Ir al siguiente punto (circular)
        indicePuntoActual = (indicePuntoActual + 1) % puntosPatrulla.Length;
    }

    void ActualizarAnimaciones()
    {
        if (anim == null) return;

        Vector3 velocidadLocal = transform.InverseTransformDirection(agent.velocity);
        float velX = velocidadLocal.x / agent.speed;
        float velY = velocidadLocal.z / agent.speed;

        anim.SetFloat("VelX", velX, 0.1f, Time.deltaTime);
        anim.SetFloat("VelY", velY, 0.1f, Time.deltaTime);
    }

    // Visualizar el rango de detección en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoParada);
    }
}