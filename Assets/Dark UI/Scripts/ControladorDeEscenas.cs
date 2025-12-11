using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorDeEscenas : MonoBehaviour
{
    public void CambiarEscena(string nombreDeLaEscena)
    {
        // Usamos la variable, NO el texto fijo.
        // Así el botón va a donde tú le digas en el editor.
        SceneManager.LoadScene("Scene 1"); 
    }
}