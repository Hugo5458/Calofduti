using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// HUD simple usando UI estándar de Unity
/// </summary>
public class SimpleGameUI : MonoBehaviour
{
    [Header("Elementos del HUD")]
    public Text healthText;
    public Text ammoText;
    public Text scoreText;
    public Text waveText;
    
    [Header("Barras")]
    public Slider healthBar;
    public Slider shieldBar;
    
    // Referencias
    private GunScript playerGun;
    private SimplePauseMenu pauseMenu;
    
    // Variables de estado
    private int currentScore = 0;
    private int currentWave = 1;
    private float maxHealth = 100f;
    private float currentHealth = 100f;
    private float maxShield = 50f;
    private float currentShield = 0f;
    
    void Start()
    {
        InitializeComponents();
        SetupHUD();
    }
    
    void Update()
    {
        if (pauseMenu != null && pauseMenu.IsPaused())
            return;
            
        UpdateAmmoDisplay();
        UpdateHealthDisplay();
    }
    
    void InitializeComponents()
    {
        // Buscar componentes del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerGun = player.GetComponent<GunScript>();
        }
        
        // Buscar sistema de pausa
        pauseMenu = FindObjectOfType<SimplePauseMenu>();
        
        // Buscar componentes si no están asignados
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.GetComponent<Slider>();
        if (shieldBar == null)
            shieldBar = transform.Find("ShieldBar")?.GetComponent<Slider>();
    }
    
    void SetupHUD()
    {
        UpdateHealthBar();
        UpdateShieldBar();
        UpdateScore(0);
        UpdateWave(1);
    }
    
    void UpdateAmmoDisplay()
    {
        if (playerGun != null && ammoText != null)
        {
            ammoText.text = $"{Mathf.FloorToInt(playerGun.bulletsInTheGun)} / {Mathf.FloorToInt(playerGun.bulletsIHave)}";
        }
    }
    
    void UpdateHealthDisplay()
    {
        if (healthText != null)
            healthText.text = $"{Mathf.FloorToInt(currentHealth)} / {Mathf.FloorToInt(maxHealth)}";
            
        UpdateHealthBar();
        UpdateShieldBar();
    }
    
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
    
    void UpdateShieldBar()
    {
        if (shieldBar != null)
        {
            shieldBar.maxValue = maxShield;
            shieldBar.value = currentShield;
        }
    }
    
    #region Métodos Públicos
    
    public void UpdateHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        
        if (currentHealth <= 0f)
        {
            Debug.Log("Game Over");
        }
    }
    
    public void UpdateShield(float newShield)
    {
        currentShield = Mathf.Clamp(newShield, 0f, maxShield);
    }
    
    public void UpdateScore(int points)
    {
        currentScore += points;
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }
    
    public void UpdateWave(int wave)
    {
        currentWave = wave;
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";
    }
    
    public void AddScore(int points)
    {
        UpdateScore(currentScore + points);
    }
    
    public void OnEnemyKilled()
    {
        AddScore(100);
    }
    
    #endregion
    
    #region Métodos de Utilidad
    
    public bool IsPlayerAlive()
    {
        return currentHealth > 0f;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    #endregion
}
