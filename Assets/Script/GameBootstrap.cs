using UnityEngine;

/// <summary>
/// Script inicializador que asegura que todos los componentes necesarios existen.
/// Colócalo en un GameObject vacío llamado "GameBootstrap" en la escena.
/// También se puede poner directamente en el Player.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Estado")]
    public bool setupComplete = false;
    
    void Awake()
    {
        SetupPlayer();
        SetupHUD();
        SetupGameManager();
        setupComplete = true;
        Debug.Log("[GameBootstrap] ✅ Todos los sistemas inicializados correctamente.");
    }
    
    void SetupPlayer()
    {
        // Buscar el jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("[GameBootstrap] ❌ No se encontró un objeto con tag 'Player'. Asegúrate de que tu jugador tiene el tag 'Player'.");
            return;
        }
        
        // Asegurar que tiene PlayerHealth
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = player.AddComponent<PlayerHealth>();
            health.maxHealth = 100f;
            health.currentHealth = 100f;
            Debug.Log("[GameBootstrap] ✅ PlayerHealth añadido al jugador automáticamente.");
        }
        else
        {
            Debug.Log("[GameBootstrap] ✅ PlayerHealth ya existe en el jugador.");
        }
        
        // Asegurar que tiene AudioSource para sonidos de daño
        AudioSource audioSource = player.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = player.AddComponent<AudioSource>();
            Debug.Log("[GameBootstrap] ✅ AudioSource añadido al jugador.");
        }
        
        // Verificar que el GunScript tiene referencias correctas
        GunScript gun = player.GetComponentInChildren<GunScript>();
        if (gun != null)
        {
            Debug.Log($"[GameBootstrap] ✅ GunScript encontrado. Bullet prefab: {(gun.bullet != null ? gun.bullet.name : "NULL")}");
            
            // Si bulletSpawnPlace es null, intentar encontrarlo
            if (gun.bulletSpawnPlace == null)
            {
                // Buscar por tag
                GameObject spawnObj = GameObject.FindGameObjectWithTag("BulletSpawn");
                if (spawnObj != null)
                {
                    gun.bulletSpawnPlace = spawnObj;
                    Debug.Log("[GameBootstrap] ✅ bulletSpawnPlace encontrado por tag 'BulletSpawn'.");
                }
                else
                {
                    // Buscar por nombre
                    Transform spawnTransform = gun.transform.Find("BulletSpawn");
                    if (spawnTransform == null) spawnTransform = gun.transform.Find("bulletSpawn");
                    if (spawnTransform == null) spawnTransform = gun.transform.Find("Muzzle");
                    if (spawnTransform == null) spawnTransform = gun.transform.Find("muzzle");
                    
                    if (spawnTransform != null)
                    {
                        gun.bulletSpawnPlace = spawnTransform.gameObject;
                        Debug.Log($"[GameBootstrap] ✅ bulletSpawnPlace encontrado: {spawnTransform.name}");
                    }
                    else
                    {
                        Debug.LogWarning("[GameBootstrap] ⚠️ No se encontró bulletSpawnPlace. Las balas se dispararán pero sin efecto visual de proyectil. El raycast funciona igual.");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[GameBootstrap] ⚠️ No se encontró GunScript en el jugador o sus hijos.");
        }
    }
    
    void SetupHUD()
    {
        // Asegurar que existe el PlayerHUD
        PlayerHUD existingHUD = FindObjectOfType<PlayerHUD>();
        if (existingHUD == null)
        {
            GameObject hudObj = new GameObject("PlayerHUD_Manager");
            hudObj.AddComponent<PlayerHUD>();
            Debug.Log("[GameBootstrap] ✅ PlayerHUD creado automáticamente.");
        }
        else
        {
            Debug.Log("[GameBootstrap] ✅ PlayerHUD ya existe.");
        }
    }
    
    void SetupGameManager()
    {
        // Asegurar que existe el GameManager
        GameManager existingGM = FindObjectOfType<GameManager>();
        if (existingGM == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
            Debug.Log("[GameBootstrap] ✅ GameManager creado automáticamente.");
        }
        else
        {
            Debug.Log("[GameBootstrap] ✅ GameManager ya existe.");
        }
    }
}
