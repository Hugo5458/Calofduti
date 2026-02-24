using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Puntuación")]
    public int scoreValue = 100;
    
    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    
    [Header("Efectos")]
    public GameObject deathEffect;
    
    private AudioSource audioSource;
    private bool isDead = false;
    private Animator animator;
    private ZombieAI zombieAI;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        zombieAI = GetComponent<ZombieAI>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // Mostrar popup de daño flotante encima de la cabeza
        Vector3 headPos = transform.position + Vector3.up * 2.2f; // Encima de la cabeza
        DamagePopup.Spawn(headPos, damage, damage >= 50f);
        
        // Sonido de daño
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        // Animación de daño (a través de GhoulAnimationController)
        GhoulAnimationController animCtrl = GetComponent<GhoulAnimationController>();
        if (animCtrl != null)
        {
            animCtrl.PlayHitAnimation();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // Cambiar layer para no bloquear raycast de balas (objeto y todos sus hijos)
        ChangeLayerRecursively(gameObject, LayerMask.NameToLayer("Ignore Raycast"));
        
        // Añadir puntuación
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.ZombieKilled();
        }
        
        // Sonido de muerte
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Animación de muerte
        GhoulAnimationController animCtrl = GetComponent<GhoulAnimationController>();
        if (animCtrl != null)
        {
            animCtrl.PlayDeathAnimation();
        }
        if (zombieAI != null)
        {
            zombieAI.PlayDeathAnimation();
        }
        
        // Desactivar IA
        if (zombieAI != null)
        {
            zombieAI.enabled = false;
        }
        
        // Desactivar collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Desactivar CharacterController para no bloquear a otros
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }
        
        // Desactivar NavMeshAgent si existe
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Efecto de muerte
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Destruir después de un tiempo
        Destroy(gameObject, 3f);
    }
    
    void ChangeLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        
        obj.layer = newLayer;
        
        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, newLayer);
        }
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}
