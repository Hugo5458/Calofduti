using UnityEngine;

/// <summary>
/// Asegura que el jugador tenga todos los componentes necesarios
/// </summary>
public class PlayerSetup : MonoBehaviour
{
    [ContextMenu("Configurar Jugador")]
    public void SetupPlayer()
    {
        // Asegurar que tiene PlayerHealth
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = gameObject.AddComponent<PlayerHealth>();
            playerHealth.maxHealth = 100f;
            playerHealth.currentHealth = 100f;
            Debug.Log("[PlayerSetup] PlayerHealth añadido al jugador");
        }
        
        // Asegurar que tiene el tag "Player"
        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
            Debug.Log("[PlayerSetup] Tag del jugador cambiado a 'Player'");
        }
        
        // Asegurar que tiene AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[PlayerSetup] AudioSource añadido al jugador");
        }
        
        // Buscar y asignar componentes UI si existen
        if (playerHealth.healthSlider == null)
        {
            GameObject healthSliderObj = GameObject.Find("HealthSlider");
            if (healthSliderObj != null)
            {
                playerHealth.healthSlider = healthSliderObj.GetComponent<UnityEngine.UI.Slider>();
            }
        }
        
        if (playerHealth.healthText == null)
        {
            GameObject healthTextObj = GameObject.Find("HealthText");
            if (healthTextObj != null)
            {
                playerHealth.healthText = healthTextObj.GetComponent<UnityEngine.UI.Text>();
            }
        }
        
        Debug.Log("[PlayerSetup] Jugador configurado correctamente");
    }
    
    void Start()
    {
        SetupPlayer();
    }
}
