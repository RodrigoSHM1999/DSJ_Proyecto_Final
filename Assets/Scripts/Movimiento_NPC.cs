using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movimiento_NPC : MonoBehaviour
{
    public Transform destino;
    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //anim = GetComponent<Animator>();
       
    }

    void Update()
    {
        agent.destination = destino.position;

        // Obtener la velocidad del agente
        Vector3 velocidadLocal = transform.InverseTransformDirection(agent.velocity);

        // Normalizar la velocidad si quieres que sea compatible con los mismos parámetros que el jugador
        float velX = velocidadLocal.x / agent.speed;
        float velY = velocidadLocal.z / agent.speed;

        // Opcional: suavizar valores para animaciones más naturales
        //anim.SetFloat("VelX", velX, 0.1f, Time.deltaTime);
        //anim.SetFloat("VelY", velY, 0.1f, Time.deltaTime);
        if(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath || agent.velocity.sqrMagnitude == 0f)
{
            
        }
    }
}
