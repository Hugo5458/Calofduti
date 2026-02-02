using UnityEngine;

/// <summary>
/// Zombie simple sin IA - solo persigue al jugador en línea recta.
/// No requiere NavMesh.
/// </summary>
public class ZombieAI : MonoBehaviour
{
    [Header("Estadísticas")]
    public float damage = 10f;
    public float attackRate = 1f;
    public float detectionRange = 50f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    
    [Header("Movimiento")]
    public float rotationSpeed = 5f;
    
    [Header("Referencias")]
    public Transform player;
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    
    private Animator animator;
    private AudioSource audioSource;
    private ZombieHealth health;
    private Rigidbody rb;
    
    private float nextAttackTime = 0f;
    private bool isChasing = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<ZombieHealth>();
        rb = GetComponent<Rigidbody>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Buscar al jugador si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    
    void Update()
    {
        if (health != null && health.IsDead()) return;
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Mirar hacia el jugador
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Perseguir o atacar
        if (distanceToPlayer <= attackRange)
        {
            // Atacar
            isChasing = false;
            Attack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // Perseguir - moverse hacia el jugador
            isChasing = true;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            isChasing = false;
        }
        
        // Actualizar animaciones
        if (animator != null)
        {
            animator.SetFloat("Speed", isChasing ? moveSpeed : 0f);
        }
    }
    
    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            
            // Animación de ataque
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // Sonido de ataque
            if (attackSounds != null && attackSounds.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)]);
            }
            
            // Aplicar daño al jugador
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
