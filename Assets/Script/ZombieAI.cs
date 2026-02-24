using UnityEngine;

/// <summary>
/// Zombie simple - persigue al jugador en línea recta.
/// No requiere NavMesh. Usa el nuevo ZombieController con parámetros.
/// </summary>
public class ZombieAI : MonoBehaviour
{
    [Header("Estadísticas")]
    public float damage = 10f;
    public float attackRate = 1f;
    public float detectionRange = 50f;
    public float attackRange = 1.2f;
    public float moveSpeed = 3f;
    
    [Header("Movimiento")]
    public float rotationSpeed = 5f;
    public float groundCheckDistance = 2f;
    [Tooltip("Distancia mínima entre zombies para evitar solapamiento")]
    public float separationDistance = 1.2f;
    [Tooltip("Fuerza de separación entre zombies")]
    public float separationForce = 2f;
    
    [Header("Referencias")]
    public Transform player;
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    
    private Animator animator;
    private AudioSource audioSource;
    private ZombieHealth health;
    
    private float nextAttackTime = 0f;
    private bool initialized = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<ZombieHealth>();
        
        // Configurar Animator
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
        
        // Desactivar NavMeshAgent si existe (no lo usamos)
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Desactivar CharacterController si existe (conflicto con movimiento directo)
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }
        
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
            else
            {
                Debug.LogError($"[ZombieAI] {gameObject.name}: ¡NO se encontró un objeto con Tag 'Player'!");
            }
        }

        // Verificar que el jugador tiene PlayerHealth
        if (player != null)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph == null)
            {
                ph = player.GetComponentInChildren<PlayerHealth>();
            }
            if (ph == null)
            {
                Debug.LogWarning($"[ZombieAI] {gameObject.name}: El Player NO tiene 'PlayerHealth'.");
            }
        }

        initialized = true;
    }
    
    void Update()
    {
        if (!initialized) return;
        if (health != null && health.IsDead()) return;
        
        // Si no hay player, intentar buscarlo de nuevo
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return;
            }
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Calcular dirección hacia el jugador (solo en XZ, sin Y)
        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0;
        Vector3 direction = dirToPlayer.normalized;
        
        // Rotar hacia el jugador
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Perseguir o atacar
        if (distanceToPlayer <= attackRange)
        {
            // Atacar
            SetSpeed(0f);
            Attack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // Perseguir
            SetSpeed(1f);
            
            // Movimiento directo hacia el jugador (solo XZ, mantener Y)
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            
            // Añadir separación de otros zombies
            Vector3 separation = GetSeparationVector();
            movement += separation * Time.deltaTime;
            
            // Solo mover en XZ, mantener la Y actual (sin gravedad)
            transform.position += new Vector3(movement.x, 0f, movement.z);
        }
        else
        {
            SetSpeed(0f);
        }
        
        // Mantener en el suelo (siempre activo)
        UpdateGroundPosition();
    }
    
    /// <summary>
    /// Mantiene al zombie pegado al suelo usando raycast
    /// </summary>
    void UpdateGroundPosition()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1f;
        
        if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance + 1f))
        {
            // Solo ajustar si está demasiado lejos del suelo
            float distanceToGround = Vector3.Distance(origin, hit.point);
            if (distanceToGround > 1.5f)
            {
                // Colocar suavemente en el suelo
                Vector3 newPos = transform.position;
                newPos.y = hit.point.y;
                transform.position = newPos;
            }
        }
    }
    
    void SetSpeed(float speed)
    {
        if (animator != null && animator.enabled)
        {
            animator.SetFloat("Speed", speed);
        }
    }
    
    /// <summary>
    /// Calcula un vector de separación para evitar solaparse con otros zombies cercanos.
    /// </summary>
    Vector3 GetSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationDistance);
        
        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            
            // Solo separarse de otros zombies
            if (col.GetComponent<ZombieAI>() != null || col.GetComponent<SimpleZombie>() != null)
            {
                Vector3 away = transform.position - col.transform.position;
                away.y = 0;
                if (away.sqrMagnitude > 0.01f)
                {
                    separation += away.normalized * separationForce;
                }
                else
                {
                    separation += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * separationForce;
                }
            }
        }
        
        return separation;
    }
    
    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            
            // Animación de ataque
            if (animator != null && animator.enabled)
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
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
            }
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    /// <summary>
    /// Aumenta las estadísticas del zombie para la dificultad progresiva
    /// </summary>
    public void IncreaseStats(float speedIncrease, float damageIncrease)
    {
        moveSpeed += speedIncrease;
        damage += damageIncrease;
        
        Debug.Log($"Zombie stats increased: Speed={moveSpeed}, Damage={damage}");
    }
    
    /// <summary>
    /// Activa la animación de muerte.
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (animator != null && animator.enabled)
        {
            animator.SetBool("Dead", true);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }
}
