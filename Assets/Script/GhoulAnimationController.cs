using UnityEngine;

/// <summary>
/// Controlador de animaciones para zombies.
/// Gestiona las animaciones de: Idle, Walk/Run, Attack, Hit, Death
/// Compatible con los Animator Controllers: Zombie.controller, ZombieController.controller, GhoulAnimatorController.controller
/// 
/// Parámetros del Animator esperados:
/// - "Speed" (float): velocidad de movimiento (0=idle, 1=walk/run)
/// - "Attack" (trigger): activa animación de ataque
/// - "Hit" (trigger): activa animación de recibir daño  
/// - "Dead" (bool): activa animación de muerte
/// </summary>
public class GhoulAnimationController : MonoBehaviour
{
    private Animator animator;
    private ZombieAI zombieAI;
    private ZombieHealth zombieHealth;
    private CharacterController characterController;
    
    // Cache del estado
    private bool hasDied = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        zombieAI = GetComponent<ZombieAI>();
        zombieHealth = GetComponent<ZombieHealth>();
        characterController = GetComponent<CharacterController>();
        
        // Si no hay Animator, intentar buscarlo en los hijos
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogWarning($"[GhoulAnimationController] {gameObject.name}: No se encontró Animator.");
        }
        else
        {
            animator.applyRootMotion = false; // Importante: ZombieAI controla el movimiento
            Debug.Log($"[GhoulAnimationController] {gameObject.name}: Animator configurado correctamente.");
        }
    }
    
    void Update()
    {
        if (animator == null) return;
        
        // Si está muerto, no hacer más animaciones
        if (zombieHealth != null && zombieHealth.IsDead())
        {
            if (!hasDied)
            {
                PlayDeathAnimation();
                hasDied = true;
            }
            return;
        }
        
        // Controlar animación de movimiento según velocidad
        if (zombieAI != null)
        {
            bool isMoving = zombieAI.IsChasing();
            
            // Intentar diferentes nombres de parámetros (compatibilidad)
            SetFloatSafe("Speed", isMoving ? 1f : 0f);
            SetFloatSafe("speed", isMoving ? 1f : 0f);
            SetFloatSafe("walkSpeed", isMoving ? 1f : 0f);
        }
        else if (characterController != null)
        {
            // Fallback: usar velocidad del CharacterController
            float speed = characterController.velocity.magnitude;
            SetFloatSafe("Speed", speed > 0.1f ? 1f : 0f);
        }
    }
    
    /// <summary>
    /// Reproduce la animación de ataque
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (animator == null) return;
        
        SetTriggerSafe("Attack");
        SetTriggerSafe("attack");
    }
    
    /// <summary>
    /// Reproduce la animación de muerte
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (animator == null) return;
        
        SetBoolSafe("Dead", true);
        SetBoolSafe("dead", true);
        SetBoolSafe("isDead", true);
        
        // También probar con trigger por si el controller usa trigger en vez de bool
        SetTriggerSafe("Die");
        SetTriggerSafe("die");
        SetTriggerSafe("Death");
    }
    
    /// <summary>
    /// Reproduce la animación de recibir daño
    /// </summary>
    public void PlayHitAnimation()
    {
        if (animator == null) return;
        
        SetTriggerSafe("Hit");
        SetTriggerSafe("hit");
        SetTriggerSafe("TakeDamage");
    }
    
    // Helpers seguros que no crashean si el parámetro no existe
    void SetFloatSafe(string paramName, float value)
    {
        try { animator.SetFloat(paramName, value); } catch { }
    }
    
    void SetTriggerSafe(string paramName)
    {
        try { animator.SetTrigger(paramName); } catch { }
    }
    
    void SetBoolSafe(string paramName, bool value)
    {
        try { animator.SetBool(paramName, value); } catch { }
    }
}
