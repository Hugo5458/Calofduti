using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de HUD (Heads-Up Display) para el juego.
/// Muestra información vital del jugador como salud, munición, puntuación, etc.
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Elementos del HUD")]
    public Text healthText;
    public Text ammoText;
    public Text scoreText;
    public Text waveText;
    public Text timeText;
    public Text enemiesText;
    
    [Header("Barras de Progreso")]
    public Slider healthBar;
    public Slider shieldBar;
    public Slider experienceBar;
    
    [Header("Iconos y Estados")]
    public Image crosshair;
    public Image reloadIndicator;
    public Image lowHealthWarning;
    public Image criticalHealthWarning;
    
    [Header("Contadores de Munición")]
    public Text currentAmmoText;
    public Text totalAmmoText;
    public Text weaponNameText;
    
    [Header("Sistema de Notificaciones")]
    public GameObject notificationPanel;
    public Text notificationText;
    public float notificationDuration = 3f;
    
    [Header("Elementos de Minimapa")]
    public RawImage minimap;
    public GameObject minimapPlayer;
    public GameObject minimapEnemyContainer;
    
    // Referencias a componentes del juego
    private GunScript playerGun;
    private Weapon playerWeapon;
    private PlayerMovementScript playerMovement;
    private AutoPauseMenuTool pauseMenu;
    
    // Variables de estado del juego
    private int currentScore = 0;
    private int currentWave = 1;
    private int enemiesKilled = 0;
    private float gameTime = 0f;
    private float maxHealth = 100f;
    private float currentHealth = 100f;
    private float maxShield = 50f;
    private float currentShield = 0f;
    
    // Variables de notificación
    private float notificationTimer = 0f;
    private bool isShowingNotification = false;
    
    void Start()
    {
        InitializeComponents();
        SetupHUD();
        HideAllWarnings();
    }
    
    void Update()
    {
        if (pauseMenu != null && pauseMenu.IsPaused())
            return;
        
        // Reintentar buscar arma si no la tenemos
        if (playerGun == null && playerWeapon == null)
        {
            FindWeaponComponents();
        }
            
        UpdateGameTime();
        UpdateAmmoDisplay();
        UpdateHealthDisplay();
        UpdateWarnings();
        UpdateNotifications();
    }
    
    void InitializeComponents()
    {
        // Buscar componentes del jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovementScript>();
        }
        
        // Buscar arma
        FindWeaponComponents();
        
        // Buscar sistema de pausa
        pauseMenu = FindObjectOfType<AutoPauseMenuTool>();
        
        // Si no hay referencias asignadas, buscarlas automáticamente
        if (healthBar == null)
            healthBar = transform.Find("HealthBar")?.GetComponent<Slider>();
        if (shieldBar == null)
            shieldBar = transform.Find("ShieldBar")?.GetComponent<Slider>();
        if (crosshair == null)
            crosshair = transform.Find("Crosshair")?.GetComponent<Image>();
    }
    
    void FindWeaponComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Buscar GunScript en el jugador y sus hijos
            playerGun = player.GetComponent<GunScript>();
            if (playerGun == null)
            {
                playerGun = player.GetComponentInChildren<GunScript>();
            }
            
            // Buscar Weapon en el jugador y sus hijos
            playerWeapon = player.GetComponent<Weapon>();
            if (playerWeapon == null)
            {
                playerWeapon = player.GetComponentInChildren<Weapon>();
            }
        }
        
        // Fallback: buscar en toda la escena
        if (playerGun == null)
        {
            playerGun = FindObjectOfType<GunScript>();
        }
        if (playerWeapon == null)
        {
            playerWeapon = FindObjectOfType<Weapon>();
        }
    }
    
    void SetupHUD()
    {
        // Inicializar valores del HUD
        UpdateHealthBar();
        UpdateShieldBar();
        UpdateScore(0);
        UpdateWave(1);
        UpdateEnemiesKilled(0);
        UpdateWeaponDisplay();
    }
    
    #region Actualización de HUD
    
    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    void UpdateAmmoDisplay()
    {
        if (playerGun != null)
        {
            // Usar GunScript (Easy FPS)
            if (ammoText != null)
            {
                ammoText.text = $"{Mathf.FloorToInt(playerGun.bulletsInTheGun)} / {Mathf.FloorToInt(playerGun.bulletsIHave)}";
            }
            
            if (currentAmmoText != null)
                currentAmmoText.text = Mathf.FloorToInt(playerGun.bulletsInTheGun).ToString();
            if (totalAmmoText != null)
                totalAmmoText.text = Mathf.FloorToInt(playerGun.bulletsIHave).ToString();
            
            if (reloadIndicator != null)
            {
                reloadIndicator.gameObject.SetActive(playerGun.reloading);
            }
            
            UpdateWeaponDisplay();
        }
        else if (playerWeapon != null)
        {
            // Usar Weapon.cs (script propio)
            if (ammoText != null)
            {
                ammoText.text = $"{playerWeapon.currentAmmo} / {playerWeapon.reserveAmmo}";
            }
            
            if (currentAmmoText != null)
                currentAmmoText.text = playerWeapon.currentAmmo.ToString();
            if (totalAmmoText != null)
                totalAmmoText.text = playerWeapon.reserveAmmo.ToString();
            
            if (weaponNameText != null)
                weaponNameText.text = playerWeapon.weaponName;
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
    
    void UpdateWarnings()
    {
        float healthPercentage = currentHealth / maxHealth;
        
        // Advertencia de salud baja
        if (lowHealthWarning != null)
        {
            lowHealthWarning.gameObject.SetActive(healthPercentage <= 0.3f && healthPercentage > 0.15f);
        }
        
        // Advertencia de salud crítica
        if (criticalHealthWarning != null)
        {
            criticalHealthWarning.gameObject.SetActive(healthPercentage <= 0.15f);
        }
    }
    
    void UpdateWeaponDisplay()
    {
        if (weaponNameText != null && playerGun != null)
        {
            // Aquí podrías mostrar el nombre del arma actual
            // Por ahora, mostramos un texto genérico
            weaponNameText.text = "ARMA ACTUAL";
        }
    }
    
    void UpdateNotifications()
    {
        if (isShowingNotification)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
            {
                HideNotification();
            }
        }
    }
    
    #endregion
    
    #region Métodos Públicos de Actualización
    
    public void UpdateHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        
        if (currentHealth <= 0f)
        {
            ShowNotification("¡Has muerto!");
            // Aquí podrías activar una pantalla de game over
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
            scoreText.text = $"Puntuación: {currentScore}";
    }
    
    public void UpdateWave(int wave)
    {
        currentWave = wave;
        if (waveText != null)
            waveText.text = $"Oleada: {currentWave}";
    }
    
    public void UpdateEnemiesKilled(int count)
    {
        enemiesKilled = count;
        if (enemiesText != null)
            enemiesText.text = $"Enemigos: {enemiesKilled}";
    }
    
    public void AddScore(int points)
    {
        UpdateScore(currentScore + points);
        ShowNotification($"+{points} puntos");
    }
    
    public void OnEnemyKilled()
    {
        UpdateEnemiesKilled(enemiesKilled + 1);
        AddScore(100); // 100 puntos por enemigo
    }
    
    #endregion
    
    #region Sistema de Notificaciones
    
    public void ShowNotification(string message)
    {
        if (notificationText != null && notificationPanel != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
            isShowingNotification = true;
            notificationTimer = notificationDuration;
        }
    }
    
    public void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
            isShowingNotification = false;
            notificationTimer = 0f;
        }
    }
    
    #endregion
    
    #region Control de Visibilidad
    
    public void ShowCrosshair()
    {
        if (crosshair != null)
            crosshair.gameObject.SetActive(true);
    }
    
    public void HideCrosshair()
    {
        if (crosshair != null)
            crosshair.gameObject.SetActive(false);
    }
    
    public void HideAllWarnings()
    {
        if (lowHealthWarning != null)
            lowHealthWarning.gameObject.SetActive(false);
        if (criticalHealthWarning != null)
            criticalHealthWarning.gameObject.SetActive(false);
        if (reloadIndicator != null)
            reloadIndicator.gameObject.SetActive(false);
    }
    
    #endregion
    
    #region Métodos de Configuración
    
    public void SetMaxHealth(float max)
    {
        maxHealth = max;
        UpdateHealthBar();
    }
    
    public void SetMaxShield(float max)
    {
        maxShield = max;
        UpdateShieldBar();
    }
    
    public void ResetGameTime()
    {
        gameTime = 0f;
    }
    
    public void ResetStats()
    {
        currentScore = 0;
        currentWave = 1;
        enemiesKilled = 0;
        gameTime = 0f;
        currentHealth = maxHealth;
        currentShield = 0f;
        
        SetupHUD();
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
    
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Limpiar referencias
        playerGun = null;
        playerMovement = null;
        pauseMenu = null;
    }
}
