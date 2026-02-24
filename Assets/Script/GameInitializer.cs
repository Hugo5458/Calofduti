using UnityEngine;

/// <summary>
/// Inicializa configuraciones críticas del juego al iniciar
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Configuración")]
    public bool autoSetupPlayer = true;
    public bool autoSetupZombies = true;
    
    void Start()
    {
        SetupPlayer();
        SetupZombies();
        
        Debug.Log("[GameInitializer] Game initialized successfully");
    }
    
    void SetupPlayer()
    {
        if (!autoSetupPlayer) return;
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            // Buscar por nombre si no hay tag
            player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.Find("PlayerCapsule");
                if (player == null)
                {
                    // Buscar el objeto principal de la escena
                    GameObject[] allObjects = FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains("Player") || obj.name.Contains("player"))
                        {
                            player = obj;
                            break;
                        }
                    }
                }
            }
        }
        
        if (player != null)
        {
            // Asegurar tag correcto
            if (player.tag != "Player")
            {
                player.tag = "Player";
                Debug.Log("[GameInitializer] Player tag set to 'Player'");
            }
            
            // Asegurar PlayerHealth
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = player.AddComponent<PlayerHealth>();
                playerHealth.maxHealth = 100f;
                playerHealth.currentHealth = 100f;
                Debug.Log("[GameInitializer] PlayerHealth added to player");
            }
            
            // Asegurar AudioSource
            AudioSource audioSource = player.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = player.AddComponent<AudioSource>();
                Debug.Log("[GameInitializer] AudioSource added to player");
            }
            
            Debug.Log($"[GameInitializer] Player setup complete: {player.name}");
        }
        else
        {
            Debug.LogError("[GameInitializer] Player not found in scene!");
        }
    }
    
    void SetupZombies()
    {
        if (!autoSetupZombies) return;
        
        ZombieAI[] allZombies = FindObjectsOfType<ZombieAI>();
        
        foreach (ZombieAI zombie in allZombies)
        {
            // Asegurar que tienen el tag correcto si es necesario
            if (zombie.gameObject.tag == "Untagged")
            {
                zombie.gameObject.tag = "Enemy";
            }
            
            // Verificar que tienen ZombieHealth
            ZombieHealth health = zombie.GetComponent<ZombieHealth>();
            if (health == null)
            {
                health = zombie.gameObject.AddComponent<ZombieHealth>();
                health.maxHealth = zombie.maxHealth;
                health.currentHealth = zombie.maxHealth;
                Debug.Log($"[GameInitializer] ZombieHealth added to {zombie.gameObject.name}");
            }
        }
        
        Debug.Log($"[GameInitializer] {allZombies.Length} zombies setup complete");
    }
}
