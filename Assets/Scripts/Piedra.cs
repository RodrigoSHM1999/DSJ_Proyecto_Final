using UnityEngine;

public class Piedra : MonoBehaviour
{
    public AudioClip sonidoImpacto;
    private bool haImpactado = false;

    // Puedes añadir partículas aquí si quieres
    // public GameObject particulasImpacto; 

    void OnCollisionEnter(Collision collision)
    {
        // Evita que se llame múltiples veces (ej. si rebota)
        if (haImpactado) return; 
        haImpactado = true;
        
        // 1. Reproducir sonido en el punto de impacto
        if (sonidoImpacto != null)
        {
            // PlayClipAtPoint crea un objeto temporal que reproduce el sonido
            // y se destruye solo. Perfecto para esto.
            AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position);
        }

        // 2. (Opcional) Instanciar partículas de polvo/chispas
        // if (particulasImpacto != null)
        // {
        //    Instantiate(particulasImpacto, transform.position, Quaternion.identity);
        // }

        // 3. !!! PUNTO CLAVE DE INTEGRACIÓN (Hablar con Rodrigo) !!!
        // Aquí es donde "avisas" al enemigo.
        // Rodrigo debe tener una función pública en su script de IA, por ejemplo:
        //
        // EnemyAI enemigo = FindObjectOfType<EnemyAI>(); // (Forma simple de encontrarlo)
        // if (enemigo != null)
        // {
        //    enemigo.EscucharDistraccion(transform.position);
        // }
        //
        // Por ahora, pon un Debug.Log:
        Debug.Log("¡DISTFACCIÓN! Enemigo debe venir a " + transform.position);


        // 4. Deshabilita el rastro y el collider para que no siga volando
        if (GetComponent<TrailRenderer>() != null)
        {
            GetComponent<TrailRenderer>().enabled = false;
        }
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true; // Deja de moverse

        // 5. Destruye la piedra después de 2 segundos
        Destroy(gameObject, 2.0f);
    }
}