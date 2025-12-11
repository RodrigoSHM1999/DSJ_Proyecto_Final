using UnityEngine;

public class EnemigoDetectorJugador : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<ReiniciarEscena>().Reiniciar();
        }
    }
}
