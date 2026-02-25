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
    private GhoulAnimationController animController;
    private ZombieAI zombieAI;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        animController = GetComponent<GhoulAnimationController>();
        zombieAI = GetComponent<ZombieAI>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public void TakeDamage(float damage)
    {
        Debug.Log($"[ZombieHealth] {gameObject.name} recibiendo {damage} daño. Vida actual: {currentHealth}/{maxHealth}, isDead: {isDead}");
        
        if (isDead) 
        {
            Debug.Log($"[ZombieHealth] {gameObject.name} ya está muerto, ignorando daño");
            return;
        }
        
        currentHealth -= damage;
        Debug.Log($"[ZombieHealth] {gameObject.name} vida después del daño: {currentHealth}/{maxHealth}");
        
        // Mostrar popup de daño flotante encima de la cabeza
        Vector3 headPos = transform.position + Vector3.up * 2.2f;
        try
        {
            DamagePopup.Spawn(headPos, damage, damage >= 50f);
            Debug.Log($"[ZombieHealth] {gameObject.name} DamagePopup creado");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ZombieHealth] Error en DamagePopup: {e.Message}");
        }
        
        // Sonido de daño
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
            Debug.Log($"[ZombieHealth] {gameObject.name} sonido de daño reproducido");
        }
        
        // Animación de daño (a través de GhoulAnimationController)
        if (animController != null)
        {
            animController.PlayHitAnimation();
            Debug.Log($"[ZombieHealth] {gameObject.name} animación de daño activada");
        }
        else
        {
            Debug.LogWarning($"[ZombieHealth] {gameObject.name} NO tiene GhoulAnimationController");
        }
        
        if (currentHealth <= 0)
        {
            Debug.Log($"[ZombieHealth] {gameObject.name} vida llegó a 0, llamando a Die()");
            Die();
        }
        else
        {
            Debug.Log($"[ZombieHealth] {gameObject.name} sigue vivo con {currentHealth} vida");
        }
    }
    
    void Die()
    {
        Debug.Log($"[ZombieHealth] {gameObject.name} Die() llamado. isDead: {isDead}, currentHealth: {currentHealth}/{maxHealth}");
        
        if (isDead) 
        {
            Debug.Log($"[ZombieHealth] {gameObject.name} ya está muerto, saliendo de Die()");
            return;
        }
        isDead = true;
        
        // Cambiar layer para no bloquear raycast de balas
        try
        {
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            ChangeLayerRecursively(gameObject, ignoreRaycastLayer);
            Debug.Log($"[ZombieHealth] {gameObject.name} cambiado a capa Ignore Raycast ({ignoreRaycastLayer})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ZombieHealth] Error cambiando capa: {e.Message}");
        }
        
        // Añadir puntuación
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
            gameManager.ZombieKilled();
            Debug.Log($"[ZombieHealth] {gameObject.name} puntuación añadida: {scoreValue}");
        }
        else
        {
            Debug.LogWarning("[ZombieHealth] GameManager no encontrado");
        }
        
        // Sonido de muerte
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
            Debug.Log($"[ZombieHealth] {gameObject.name} sonido de muerte reproducido");
        }
        
        // Animación de muerte (centralizada en GhoulAnimationController)
        if (animController != null)
        {
            animController.PlayDeathAnimation();
            Debug.Log($"[ZombieHealth] {gameObject.name} animación de muerte activada");
        }
        else
        {
            Debug.LogWarning($"[ZombieHealth] {gameObject.name} NO tiene GhoulAnimationController");
        }
        
        // Desactivar IA
        if (zombieAI != null)
        {
            zombieAI.enabled = false;
            Debug.Log($"[ZombieHealth] {gameObject.name} IA desactivada");
        }
        
        // Desactivar TODOS los colliders (incluidos hijos como Hitbox)
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        Debug.Log($"[ZombieHealth] {gameObject.name} encontrados {allColliders.Length} colliders para desactivar");
        foreach (Collider col in allColliders)
        {
            if (col is CharacterController)
            {
                ((CharacterController)col).enabled = false;
                Debug.Log($"[ZombieHealth] {gameObject.name} CharacterController desactivado");
            }
            else
            {
                col.enabled = false;
                Debug.Log($"[ZombieHealth] {gameObject.name} Collider desactivado: {col.GetType().Name}");
            }
        }
        
        // Desactivar NavMeshAgent si existe
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            Debug.Log($"[ZombieHealth] {gameObject.name} NavMeshAgent desactivado");
        }
        
        // Efecto de muerte
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            Debug.Log($"[ZombieHealth] {gameObject.name} efecto de muerte creado");
        }
        
        // Destruir después de un tiempo
        Debug.Log($"[ZombieHealth] {gameObject.name} programado para destruir en 3 segundos");
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
