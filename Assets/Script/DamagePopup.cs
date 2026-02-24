using UnityEngine;
using TMPro;

/// <summary>
/// Popup de daño flotante que aparece encima de los zombies al recibir daño.
/// Se auto-genera con DamagePopup.Spawn()
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;
    private float scaleSpeed = 1f;
    
    private static GameObject popupPrefab;
    
    /// <summary>
    /// Crea un popup de daño en la posición indicada
    /// </summary>
    public static DamagePopup Spawn(Vector3 position, float damageAmount, bool isCritical = false)
    {
        // Crear el GameObject
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.position = position + Vector3.up * 0.5f; // Un poco por encima
        
        DamagePopup popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup(damageAmount, isCritical);
        
        return popup;
    }
    
    void Setup(float damageAmount, bool isCritical)
    {
        // Crear TextMeshPro
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        textMesh.fontSize = isCritical ? 8f : 5f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        
        // Color según tipo de daño
        if (isCritical)
        {
            textColor = new Color(1f, 0.2f, 0f, 1f); // Naranja/rojo para crítico
            textMesh.fontSize = 8f;
        }
        else if (damageAmount >= 50f)
        {
            textColor = new Color(1f, 0.85f, 0f, 1f); // Amarillo para daño alto
        }
        else
        {
            textColor = new Color(1f, 1f, 1f, 1f); // Blanco normal
        }
        
        textMesh.color = textColor;
        textMesh.outlineWidth = 0.3f;
        textMesh.outlineColor = Color.black;
        
        // Configurar movimiento aleatorio hacia arriba
        float randomX = Random.Range(-0.5f, 0.5f);
        moveVector = new Vector3(randomX, 2f, 0f);
        
        disappearTimer = 1.2f;
        
        // Auto-destruir
        Destroy(gameObject, 1.5f);
    }
    
    void Update()
    {
        // Mover hacia arriba
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 3f * Time.deltaTime; // Desacelerar
        
        // Siempre mirar hacia la cámara
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
        
        // Fade out
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0.5f)
        {
            // Fase de desaparición
            textColor.a -= 2f * Time.deltaTime;
            textMesh.color = textColor;
            
            // Encoger
            float shrinkSpeed = 3f;
            transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
            if (transform.localScale.x < 0.1f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Fase de aparición — escalar un poco
            if (transform.localScale.x < 1f)
            {
                transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
            }
        }
    }
}
