using UnityEngine;

/// <summary>
/// Script simplificado de Zombie que no requiere NavMesh.
/// Útil para prototipos rápidos o terrenos sin NavMesh bakeado.
/// </summary>
public class SimpleZombie : MonoBehaviour
{
    [Header("Estadísticas")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damage = 10f;
    public float attackRate = 1f;
    public float moveSpeed = 3f;
    public float detectionRange = 30f;
    public float attackRange = 2f;
    public int scoreValue = 100;
    
    [Header("Movimiento")]
    public float rotationSpeed = 5f;
    public float groundCheckDistance = 1.5f;
    public LayerMask groundLayer;
    
    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip attackSound;
    
    private Transform player;
    private AudioSource audioSource;
    private CharacterController controller;
    private bool isDead = false;
    private float nextAttackTime = 0f;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        controller = GetComponent<CharacterController>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Buscar jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Mirar hacia el jugador
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Moverse hacia el jugador
            if (distanceToPlayer > attackRange)
            {
                Vector3 moveDirection = direction * moveSpeed;
                
                // Aplicar gravedad
                if (controller != null)
                {
                    if (controller.isGrounded)
                    {
                        verticalVelocity = -2f;
                    }
                    else
                    {
                        verticalVelocity += gravity * Time.deltaTime;
                    }
                    
                    moveDirection.y = verticalVelocity;
                    controller.Move(moveDirection * Time.deltaTime);
                }
                else
                {
                    // Sin CharacterController, mover directamente
                    transform.position += direction * moveSpeed * Time.deltaTime;
                    
                    // Intentar mantener en el suelo
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, groundCheckDistance, groundLayer))
                    {
                        transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                    }
                }
            }
            else
            {
                // Atacar
                Attack();
            }
        }
    }
    
    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            
            // Sonido de ataque
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
            
            // Aplicar daño
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        
        // Puntuación
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.ZombieKilled();
        }
        
        // Sonido
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Desactivar collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Desactivar controller
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Destruir después de un tiempo
        Destroy(gameObject, 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
