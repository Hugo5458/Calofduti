using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Configuración de Mira")]
    public float size = 20f;
    public float thickness = 2f;
    public float gap = 10f;
    public Color color = Color.white;
    public bool showDot = true;
    public float dotSize = 4f;
    
    [Header("Dinámico")]
    public bool dynamic = true;
    public float spreadAmount = 10f;
    public float spreadSpeed = 5f;
    
    private float currentSpread = 0f;
    private Texture2D texture;
    
    void Start()
    {
        // Crear textura blanca para dibujar
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
    }
    
    void Update()
    {
        // Spread dinámico cuando se mueve o dispara
        if (dynamic)
        {
            bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
            bool isShooting = Input.GetButton("Fire1");
            
            float targetSpread = 0f;
            if (isMoving) targetSpread += spreadAmount * 0.5f;
            if (isShooting) targetSpread += spreadAmount;
            
            currentSpread = Mathf.Lerp(currentSpread, targetSpread, Time.deltaTime * spreadSpeed);
        }
    }
    
    void OnGUI()
    {
        if (texture == null) return;
        
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float adjustedGap = gap + currentSpread;
        
        GUI.color = color;
        
        // Línea superior
        GUI.DrawTexture(new Rect(centerX - thickness / 2, centerY - adjustedGap - size, thickness, size), texture);
        
        // Línea inferior
        GUI.DrawTexture(new Rect(centerX - thickness / 2, centerY + adjustedGap, thickness, size), texture);
        
        // Línea izquierda
        GUI.DrawTexture(new Rect(centerX - adjustedGap - size, centerY - thickness / 2, size, thickness), texture);
        
        // Línea derecha
        GUI.DrawTexture(new Rect(centerX + adjustedGap, centerY - thickness / 2, size, thickness), texture);
        
        // Punto central
        if (showDot)
        {
            GUI.DrawTexture(new Rect(centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize), texture);
        }
    }
}
