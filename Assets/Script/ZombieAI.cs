using UnityEngine;

/// <summary>
/// Zombie con colisiones - persigue al jugador esquivando obstáculos.
/// Usa CharacterController para respetar colliders de casas/edificios.
/// 
/// IMPORTANTE: Las animaciones se controlan EXCLUSIVAMENTE a través de
/// GhoulAnimationController. Este script NO toca el Animator directamente.
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
    
    private GhoulAnimationController animController;
    private AudioSource audioSource;
    private ZombieHealth health;
    private CharacterController characterController;
    
    private float nextAttackTime = 0f;
    private bool initialized = false;
    private float verticalVelocity = 0f;
    private bool isAttacking = false;
    private float attackAnimationDuration = 1.0f;
    private PlayerHealth cachedPlayerHealth;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<ZombieHealth>();
        
        // Buscar animController - lazy init también disponible en Update
        animController = GetComponent<GhoulAnimationController>();
        
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
            characterController.center = new Vector3(0f, 2f, 0f);
            characterController.height = 4f;
            characterController.radius = 1.0f;
        }
        else
        {
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
    
    /// <summary>
    /// Obtiene el GhoulAnimationController si aún no lo tenemos.
    /// </summary>
    GhoulAnimationController GetAnimController()
    {
        if (animController == null)
        {
            animController = GetComponent<GhoulAnimationController>();
            if (animController != null)
            {
                animController.Initialize(); // Asegurar que está inicializado
            }
        }
        return animController;
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
        
        // Si estamos en medio de un ataque, no hacer nada más (solo gravedad)
        if (isAttacking)
        {
            ApplyGravityOnly();
            return;
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
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        // == DECIDIR ESTADO ==
        if (distanceToPlayer <= attackRange)
        {
            // ATACAR
            GhoulAnimationController anim = GetAnimController();
            if (anim != null && !anim.IsAttacking())
            {
                anim.SetIdle();
            }
            Attack();
            ApplyGravityOnly();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // PERSEGUIR
            GhoulAnimationController anim = GetAnimController();
            if (anim != null) anim.SetWalking();
            
            Vector3 moveDir = direction * moveSpeed;
            
            // Evitar obstáculos
            Vector3 avoidance = GetObstacleAvoidance(direction);
            moveDir += avoidance;
            
            // Separación de otros zombies
            Vector3 separation = GetSeparationVector();
            moveDir += separation;
            
            // Movimiento final con gravedad
            Vector3 finalMove = new Vector3(moveDir.x, verticalVelocity, moveDir.z) * Time.deltaTime;
            characterController.Move(finalMove);
        }
        else
        {
            // IDLE
            GhoulAnimationController anim = GetAnimController();
            if (anim != null) anim.SetIdle();
            ApplyGravityOnly();
        }
    }
    
    void ApplyGravityOnly()
    {
        if (characterController != null && characterController.enabled)
        {
            if (characterController.isGrounded)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
            
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
        
        RaycastHit hit;
        if (Physics.Raycast(origin, moveDirection, out hit, obstacleDetectionRange))
        {
            if (hit.collider is TerrainCollider) return avoidance;
            if (hit.collider.GetComponent<ZombieAI>() != null) return avoidance;
            if (hit.collider.GetComponent<ZombieHealth>() != null) return avoidance;
            if (hit.collider.CompareTag("Player")) return avoidance;
            
            float obstacleProximity = 1f - (hit.distance / obstacleDetectionRange);
            
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
                avoidance = Quaternion.Euler(0, 180, 0) * moveDirection * obstacleAvoidanceForce * obstacleProximity;
            }
        }
        
        avoidance.y = 0f;
        return avoidance;
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
        if (Time.time < nextAttackTime) return;
        if (isAttacking) return;
        
        nextAttackTime = Time.time + attackRate;
        isAttacking = true;
        
        Debug.Log($"[ZombieAI] {gameObject.name} ATTACKING! Distance to player: {Vector3.Distance(transform.position, player.position):F1}");
        
        // Pedir la animación de ataque al controlador de animaciones
        GhoulAnimationController anim = GetAnimController();
        if (anim != null)
        {
            anim.PlayAttackAnimation();
        }
        else
        {
            // Fallback directo: intentar activar trigger en el Animator
            Animator directAnimator = GetComponent<Animator>();
            if (directAnimator == null) directAnimator = GetComponentInChildren<Animator>();
            if (directAnimator != null)
            {
                try
                {
                    directAnimator.ResetTrigger("Attack");
                    directAnimator.SetTrigger("Attack");
                    directAnimator.SetFloat("Speed", 0f);
                    Debug.Log($"[ZombieAI] {gameObject.name}: Attack via direct Animator (no GhoulAnimationController)");
                }
                catch { }
            }
        }
        
        // Reproducir sonido de ataque
        if (audioSource != null && attackSounds != null && attackSounds.Length > 0)
        {
            int randomSound = Random.Range(0, attackSounds.Length);
            audioSource.PlayOneShot(attackSounds[randomSound]);
        }
        
        // Secuencia de ataque completa
        StartCoroutine(CompleteAttackSequence());
    }
    
    System.Collections.IEnumerator CompleteAttackSequence()
    {
        // Esperar al momento del impacto (mitad de la animación)
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Aplicar daño en el momento del impacto
        if (player != null)
        {
            float hitRange = attackRange * 1.2f; // Un poco más de rango para que el golpe conecte
            float realDistance = Vector3.Distance(transform.position, player.position);
            
            if (realDistance <= hitRange)
            {
                // Verificar línea de visión
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                RaycastHit losHit;
                float checkDist = realDistance;
                
                bool canHit = true;
                if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out losHit, checkDist + 0.5f))
                {
                    if (!losHit.transform.CompareTag("Player") && 
                        losHit.transform.GetComponent<PlayerHealth>() == null &&
                        losHit.transform.GetComponentInParent<PlayerHealth>() == null)
                    {
                        canHit = false;
                    }
                }
                
                if (canHit)
                {
                    if (cachedPlayerHealth == null)
                    {
                        cachedPlayerHealth = player.GetComponent<PlayerHealth>();
                        if (cachedPlayerHealth == null)
                            cachedPlayerHealth = player.GetComponentInChildren<PlayerHealth>();
                    }
                    
                    if (cachedPlayerHealth != null && !cachedPlayerHealth.IsDead())
                    {
                        cachedPlayerHealth.TakeDamage(damage);
                        Debug.Log($"[ZombieAI] {gameObject.name} applied {damage} damage to player!");
                    }
                }
            }
        }
        
        // Esperar el resto de la animación
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);
        
        // Resetear estado de ataque
        isAttacking = false;
        
        // Notificar al controlador de animaciones que el ataque terminó
        GhoulAnimationController anim = GetAnimController();
        if (anim != null)
        {
            anim.OnAttackFinished();
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
    /// Activa la animación de muerte (delegado a GhoulAnimationController).
    /// </summary>
    public void PlayDeathAnimation()
    {
        GhoulAnimationController anim = GetAnimController();
        if (anim != null)
        {
            anim.PlayDeathAnimation();
        }
        else
        {
            // Fallback directo
            Animator directAnimator = GetComponent<Animator>();
            if (directAnimator == null) directAnimator = GetComponentInChildren<Animator>();
            if (directAnimator != null)
            {
                try { directAnimator.SetBool("Dead", true); } catch { }
            }
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
