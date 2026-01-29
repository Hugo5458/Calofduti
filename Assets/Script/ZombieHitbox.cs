using UnityEngine;

/// <summary>
/// Detector de hits en partes del cuerpo del zombie para daño multiplicado.
/// Añadir a cada parte del cuerpo del zombie (cabeza, torso, etc.)
/// </summary>
public class ZombieHitbox : MonoBehaviour
{
    [Header("Configuración de Hitbox")]
    public ZombieHealth zombieHealth;
    public float damageMultiplier = 1f;
    
    [Header("Tipos de Hitbox")]
    public HitboxType hitboxType = HitboxType.Body;
    
    public enum HitboxType
    {
        Head,       // x2 daño
        Body,       // x1 daño
        Limb        // x0.5 daño
    }
    
    void Start()
    {
        // Buscar ZombieHealth en el padre si no está asignado
        if (zombieHealth == null)
        {
            zombieHealth = GetComponentInParent<ZombieHealth>();
        }
        
        // Establecer multiplicador según tipo
        switch (hitboxType)
        {
            case HitboxType.Head:
                damageMultiplier = 2f;
                break;
            case HitboxType.Body:
                damageMultiplier = 1f;
                break;
            case HitboxType.Limb:
                damageMultiplier = 0.5f;
                break;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (zombieHealth != null)
        {
            zombieHealth.TakeDamage(damage * damageMultiplier);
            
            // Bonus de puntos por headshot
            if (hitboxType == HitboxType.Head)
            {
                GameManager gm = FindObjectOfType<GameManager>();
                if (gm != null)
                {
                    gm.AddScore(50); // Bonus headshot
                }
            }
        }
    }
}
