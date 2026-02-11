using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Desactiva automáticamente los colliders de vegetación (césped, brotes, árboles decorativos).
/// Busca por nombres en toda la escena, no necesita asignar padres manualmente.
/// </summary>
public class VegetationSetup : MonoBehaviour
{
    [Header("Búsqueda automática")]
    [Tooltip("Palabras clave para detectar vegetación por nombre (separadas por coma)")]
    public string keywords = "grass,cesped,césped,hierba,brote,plant,vegetation,vegetacion,weed,bush,arbusto,flora,flower,flor,leaf,hoja,tomillo,romero,cespedraro";
    
    [Header("Opciones")]
    [Tooltip("También desactivar colliders de árboles")]
    public bool includeTreeKeywords = false;
    
    [Tooltip("Palabras clave extra para árboles (separadas por coma)")]
    public string treeKeywords = "tree,arbol,árbol";
    
    [Header("Info (solo lectura)")]
    [SerializeField] private int collidersDisabled = 0;
    [SerializeField] private int objectsFound = 0;

    void Start()
    {
        DisableVegetationColliders();
    }

    public void DisableVegetationColliders()
    {
        collidersDisabled = 0;
        objectsFound = 0;
        
        // Preparar lista de palabras clave
        List<string> searchWords = new List<string>();
        foreach (string word in keywords.Split(','))
        {
            string trimmed = word.Trim().ToLower();
            if (trimmed.Length > 0) searchWords.Add(trimmed);
        }
        if (includeTreeKeywords)
        {
            foreach (string word in treeKeywords.Split(','))
            {
                string trimmed = word.Trim().ToLower();
                if (trimmed.Length > 0) searchWords.Add(trimmed);
            }
        }
        
        Debug.Log("[VegetationSetup] Buscando objetos con palabras clave: " + string.Join(", ", searchWords));
        
        // Buscar en TODOS los objetos de la escena
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            string nameLower = obj.name.ToLower();
            bool match = false;
            
            foreach (string word in searchWords)
            {
                if (nameLower.Contains(word))
                {
                    match = true;
                    break;
                }
            }
            
            // También revisar el nombre del padre
            if (!match && obj.transform.parent != null)
            {
                string parentName = obj.transform.parent.name.ToLower();
                foreach (string word in searchWords)
                {
                    if (parentName.Contains(word))
                    {
                        match = true;
                        break;
                    }
                }
            }
            
            if (match)
            {
                objectsFound++;
                Collider[] colliders = obj.GetComponents<Collider>();
                foreach (Collider col in colliders)
                {
                    if (col.enabled)
                    {
                        col.enabled = false;
                        collidersDisabled++;
                    }
                }
            }
        }
        
        Debug.Log("[VegetationSetup] Resultado: " + objectsFound + " objetos de vegetación encontrados, " + collidersDisabled + " colliders desactivados.");
    }
}
