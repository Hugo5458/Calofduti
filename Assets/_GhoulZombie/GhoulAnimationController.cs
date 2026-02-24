using UnityEngine;

public class GhoulAnimationController : MonoBehaviour
{
    private Animator animator;
    private ZombieAI zombieAI;
    private ZombieHealth zombieHealth;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        zombieAI = GetComponent<ZombieAI>();
        zombieHealth = GetComponent<ZombieHealth>();
        
        // Asegurar que el Animator está configurado
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
    }
    
    void Update()
    {
        if (animator == null) return;
        
        // Si está muerto, no hacer más animaciones
        if (zombieHealth != null && zombieHealth.IsDead())
        {
            return;
        }
        
        // Controlar animación de movimiento
        if (zombieAI != null)
        {
            // Si está persiguiendo al jugador, poner animación de caminar
            bool isMoving = zombieAI.IsChasing();
            animator.SetFloat("Speed", isMoving ? 1f : 0f);
        }
    }
    
    /// <summary>
    /// Reproduce la animación de ataque
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    
    /// <summary>
    /// Reproduce la animación de muerte
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("Dead", true);
        }
    }
    
    /// <summary>
    /// Reproduce la animación de recibir daño
    /// </summary>
    public void PlayHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }
}
