using UnityEngine;

/// <summary>
/// Zombie con colisiones - persigue al jugador esquivando obstáculos.
/// Usa CharacterController para respetar colliders de casas/edificios.
/// </summary>
public class ZombieAI : MonoBehaviour
{
    [Header("Estadísticas")]
    public float damage = 10f;
    public float attackRate = 1f;
    public float maxHealth = 100f;
    public float detectionRange = 50f;
    public float attackRange = 2.5f;
    public float moveSpeed = 3f;
    
    [Header("Movimiento")]
    public float rotationSpeed = 5f;
    public float gravity = -15f;
    [Tooltip("Distancia mínima entre zombies para evitar solapamiento")]
    public float separationDistance = 1.2f;
    [Tooltip("Fuerza de separación entre zombies")]
    public float separationForce = 2f;
    
    [Header("Evitación de Obstáculos")]
    [Tooltip("Distancia del raycast para detectar obstáculos delante")]
    public float obstacleDetectionRange = 2.5f;
    [Tooltip("Fuerza para esquivar obstáculos")]
    public float obstacleAvoidanceForce = 5f;
    
    [Header("Referencias")]
    public Transform player;
    [Tooltip("¿Es este zombie un Ghoul? (Afecta el tamaño de la hitbox)")]
    public bool isGhoul = false;
    
    [Header("Audio")]
    public AudioClip[] attackSounds;
    
    private Animator animator;
    private AudioSource audioSource;
    private ZombieHealth health;
    private CharacterController characterController;
    
    private float nextAttackTime = 0f;
    private bool initialized = false;
    private float verticalVelocity = 0f;
    
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

        // Configurar CharacterController para colisiones con casas/edificios
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
        
        // Ajustar tamaño según tipo de zombie
        if (isGhoul)
        {
            // Ghoul: tamaño 20 (radio 1.0, altura 4.0)
            characterController.center = new Vector3(0f, 2f, 0f);
            characterController.height = 4f;
            characterController.radius = 1.0f;
        }
        else
        {
            // Zombie normal: tamaño 10 (radio 0.5, altura 2.0)
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.height = 2f;
            characterController.radius = 0.5f;
        }
        
        characterController.slopeLimit = 45f;
        characterController.stepOffset = 0.5f;
        characterController.skinWidth = 0.08f;
        characterController.enabled = true;
        
        // Sincronizar ZombieHealth con maxHealth
        if (health != null)
        {
            health.maxHealth = maxHealth;
            health.currentHealth = maxHealth;
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
        if (characterController == null || !characterController.enabled) return;
        
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
        
        // Gravedad
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f; // Mantiene al zombie pegado al suelo
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        // Perseguir o atacar
        if (distanceToPlayer <= attackRange)
        {
            // Atacar - solo aplicar gravedad
            SetSpeed(0f);
            Attack();
            
            Vector3 gravityMove = new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
            characterController.Move(gravityMove);
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // Perseguir
            SetSpeed(1f);
            
            // Dirección base hacia el jugador
            Vector3 moveDir = direction * moveSpeed;
            
            // Evitar obstáculos (casas, edificios, etc.)
            Vector3 avoidance = GetObstacleAvoidance(direction);
            moveDir += avoidance;
            
            // Añadir separación de otros zombies
            Vector3 separation = GetSeparationVector();
            moveDir += separation;
            
            // Movimiento final con gravedad usando CharacterController
            Vector3 finalMove = new Vector3(moveDir.x, verticalVelocity, moveDir.z) * Time.deltaTime;
            characterController.Move(finalMove);
        }
        else
        {
            // Idle - solo aplicar gravedad
            SetSpeed(0f);
            Vector3 gravityMove = new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
            characterController.Move(gravityMove);
        }
    }
    
    /// <summary>
    /// Usa raycasts para detectar obstáculos (casas, edificios) y calcular
    /// una dirección de evasión para rodearlos.
    /// </summary>
    Vector3 GetObstacleAvoidance(Vector3 moveDirection)
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        
        // Raycast frontal
        RaycastHit hit;
        if (Physics.Raycast(origin, moveDirection, out hit, obstacleDetectionRange))
        {
            // Ignorar si es otro zombie, el jugador, o es terreno
            if (hit.collider is TerrainCollider) return avoidance;
            if (hit.collider.GetComponent<ZombieAI>() != null) return avoidance;
            if (hit.collider.GetComponent<ZombieHealth>() != null) return avoidance;
            if (hit.collider.CompareTag("Player")) return avoidance;
            
            // Hay un obstáculo (casa/edificio) - calcular evasión
            float obstacleProximity = 1f - (hit.distance / obstacleDetectionRange);
            
            // Probar izquierda y derecha para elegir la mejor dirección
            Vector3 rightDir = Quaternion.Euler(0, 45, 0) * moveDirection;
            Vector3 leftDir = Quaternion.Euler(0, -45, 0) * moveDirection;
            
            bool rightBlocked = Physics.Raycast(origin, rightDir, obstacleDetectionRange * 0.7f);
            bool leftBlocked = Physics.Raycast(origin, leftDir, obstacleDetectionRange * 0.7f);
            
            if (!rightBlocked)
            {
                avoidance = Quaternion.Euler(0, 90, 0) * moveDirection * obstacleAvoidanceForce * obstacleProximity;
            }
            else if (!leftBlocked)
            {
                avoidance = Quaternion.Euler(0, -90, 0) * moveDirection * obstacleAvoidanceForce * obstacleProximity;
            }
            else
            {
                // Ambos lados bloqueados - girar completamente
                avoidance = Quaternion.Euler(0, 180, 0) * moveDirection * obstacleAvoidanceForce * obstacleProximity;
            }
        }
        
        avoidance.y = 0f;
        return avoidance;
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
    
    private PlayerHealth cachedPlayerHealth;
    private bool isAttacking = false;
    
    void Attack()
    {
        if (Time.time < nextAttackTime) return;
        if (isAttacking) return;
        
        nextAttackTime = Time.time + attackRate;
        
        // Reproducir animación de ataque
        GhoulAnimationController ghoulAnimator = GetComponent<GhoulAnimationController>();
        if (ghoulAnimator != null)
        {
            ghoulAnimator.PlayAttackAnimation();
        }
        else if (animator != null && animator.enabled)
        {
            animator.SetTrigger("Attack");
        }
        
        // Reproducir sonido de ataque
        if (audioSource != null && attackSounds.Length > 0)
        {
            int randomSound = Random.Range(0, attackSounds.Length);
            audioSource.PlayOneShot(attackSounds[randomSound]);
        }
        
        // Aplicar daño con DELAY para que coincida con la animación del golpe
        isAttacking = true;
        StartCoroutine(DelayedDamage());
    }
    
    System.Collections.IEnumerator DelayedDamage()
    {
        // Esperar a que la animación de ataque llegue al momento del impacto
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
        
        if (player == null) yield break;
        
        // Verificar distancia REAL en el momento del golpe (no cuando empezó la animación)
        float hitRange = attackRange * 0.85f; // Rango de golpe real es menor que rango de inicio de ataque
        float realDistance = Vector3.Distance(transform.position, player.position);
        
        if (realDistance > hitRange)
        {
            // El jugador se alejó — el golpe falla
            yield break;
        }
        
        // Verificar línea de visión — que no haya otro zombie u obstáculo bloqueando
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        RaycastHit losHit;
        float checkDist = realDistance;
        
        if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out losHit, checkDist + 0.5f))
        {
            // Si el raycast golpea algo que NO es el jugador, el golpe es bloqueado
            if (!losHit.transform.CompareTag("Player") && 
                losHit.transform.GetComponent<PlayerHealth>() == null &&
                losHit.transform.GetComponentInParent<PlayerHealth>() == null)
            {
                yield break; // Bloqueado por obstáculo u otro zombie
            }
        }
        
        // Buscar PlayerHealth (usar caché para rendimiento)
        if (cachedPlayerHealth == null)
        {
            cachedPlayerHealth = player.GetComponent<PlayerHealth>();
            if (cachedPlayerHealth == null)
                cachedPlayerHealth = player.GetComponentInChildren<PlayerHealth>();
        }
        
        if (cachedPlayerHealth != null && !cachedPlayerHealth.IsDead())
        {
            cachedPlayerHealth.TakeDamage(damage);
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
    
    /// <summary>
    /// Verifica si el zombie está persiguiendo al jugador
    /// </summary>
    public bool IsChasing()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange && distanceToPlayer > attackRange;
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
