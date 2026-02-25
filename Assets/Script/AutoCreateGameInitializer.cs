using UnityEngine;

/// <summary>
/// Crea automáticamente el GameInitializer si no existe en la escena.
/// Coloca este script en cualquier objeto de la escena (como un GameObject vacío).
/// </summary>
public class AutoCreateGameInitializer : MonoBehaviour
{
    void Start()
    {
        // Verificar si ya existe un GameInitializer
        GameInitializer existing = FindObjectOfType<GameInitializer>();
        if (existing == null)
        {
            // Crear un objeto con GameInitializer
            GameObject gameInitObj = new GameObject("GameInitializer");
            GameInitializer gameInit = gameInitObj.AddComponent<GameInitializer>();
            Debug.Log("[AutoCreateGameInitializer] GameInitializer creado automáticamente");
        }
        else
        {
            Debug.Log("[AutoCreateGameInitializer] GameInitializer ya existe en la escena");
        }
        
        // Destruir este script después de usarlo
        Destroy(this);
    }
}
