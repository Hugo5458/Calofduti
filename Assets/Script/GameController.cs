using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del juego que integra todos los sistemas.
/// Maneja el flujo del juego, pausa, y comunicación entre sistemas.
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController Instance;
    
    [Header("Referencias a Sistemas")]
    public AutoPauseMenuTool pauseMenu;
    public GameUI gameUI;
    public AudioManager audioManager;
    
    [Header("Configuración del Juego")]
    public bool startPaused = false;
    public bool allowPauseInMenu = false;
    
    // Estado del juego
    private bool isGameActive = false;
    private bool isPaused = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Inicializar sistemas
        SetupSystems();
        
        // Configurar estado inicial
        if (startPaused && !IsInMenuScene())
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    
    void Update()
    {
        HandleInput();
        UpdateGameState();
    }
    
    void InitializeGame()
    {
        Debug.Log("GameController inicializado");
        
        // Asegurar que el tiempo del juego esté normal
        Time.timeScale = 1f;
        
        // Configurar cursor inicial
        if (!IsInMenuScene())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void SetupSystems()
    {
        // Buscar sistemas si no están asignados
        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<AutoPauseMenuTool>();
            
        if (gameUI == null)
            gameUI = FindObjectOfType<GameUI>();
            
        if (audioManager == null)
            audioManager = AudioManager.Instance;
        
        // Configurar sistemas
        SetupPauseMenu();
        SetupGameUI();
        SetupAudioManager();
    }
    
    void SetupPauseMenu()
    {
        if (pauseMenu != null)
        {
            Debug.Log("PauseMenu configurado");
        }
        else
        {
            Debug.LogWarning("PauseMenu no encontrado en la escena");
        }
    }
    
    void SetupGameUI()
    {
        if (gameUI != null)
        {
            Debug.Log("GameUI configurado");
            
            // Configurar HUD inicial
            if (!IsInMenuScene())
            {
                gameUI.ShowCrosshair();
                gameUI.ResetStats();
            }
        }
        else
        {
            Debug.LogWarning("GameUI no encontrado en la escena");
        }
    }
    
    void SetupAudioManager()
    {
        if (audioManager != null)
        {
            Debug.Log("AudioManager configurado");
        }
        else
        {
            Debug.LogWarning("AudioManager no encontrado");
        }
    }
    
    void HandleInput()
    {
        // Solo manejar input si estamos en una escena de juego
        if (IsInMenuScene() && !allowPauseInMenu)
            return;
            
        // NOTA: La pausa con ESC la gestiona AutoPauseMenuTool.cs
        // No duplicar aquí para evitar conflictos
        
        // Input de reinicio (para testing)
        if (Input.GetKeyDown(KeyCode.F5))
        {
            RestartLevel();
        }
        
        // Input de pantalla completa
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }
    }
    
    void UpdateGameState()
    {
        // Actualizar estado del juego basado en el sistema de pausa
        if (pauseMenu != null)
        {
            isPaused = pauseMenu.IsPaused();
        }
        
        // Sincronizar estado con otros sistemas
        if (gameUI != null)
        {
            // El GameUI ya maneja su propia lógica de pausa
        }
    }
    
    #region Control del Juego
    
    public void PauseGame()
    {
        if (!isPaused && !IsInMenuScene())
        {
            isPaused = true;
            
            if (pauseMenu != null)
                pauseMenu.PauseGame();
                
            if (gameUI != null)
                gameUI.HideCrosshair();
                
            Debug.Log("Juego pausado por GameController");
        }
    }
    
    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            
            if (pauseMenu != null)
                pauseMenu.ResumeGame();
                
            if (gameUI != null && !IsInMenuScene())
                gameUI.ShowCrosshair();
                
            Debug.Log("Juego reanudado por GameController");
        }
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void RestartLevel()
    {
        Debug.Log("Reiniciando nivel...");
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        
        // Notificar a los sistemas antes de reiniciar
        if (gameUI != null)
            gameUI.ResetStats();
            
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    public void GoToMainMenu()
    {
        Debug.Log("Yendo al menú principal...");
        Time.timeScale = 1f;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("Inicio");
    }
    
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Configuración de Visual
    
    public void ToggleFullscreen()
    {
        bool isFullscreen = Screen.fullScreen;
        Screen.fullScreen = !isFullscreen;
        
        if (audioManager != null)
            audioManager.PlayButtonClick();
            
        Debug.Log($"Pantalla completa: {!isFullscreen}");
    }
    
    #endregion
    
    #region Métodos de Utilidad
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public bool IsGameActive()
    {
        return isGameActive && !IsInMenuScene();
    }
    
    public bool IsInMenuScene()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return currentScene == "Inicio" || currentScene == "MainMenu";
    }
    
    public void SetGameActive(bool active)
    {
        isGameActive = active;
        
        if (active)
        {
            Debug.Log("Juego activado");
            if (gameUI != null)
                gameUI.ShowCrosshair();
        }
        else
        {
            Debug.Log("Juego desactivado");
            if (gameUI != null)
                gameUI.HideCrosshair();
        }
    }
    
    #endregion
    
    #region Eventos del Juego
    
    public void OnEnemyKilled()
    {
        if (gameUI != null)
            gameUI.OnEnemyKilled();
            
        if (audioManager != null)
            audioManager.PlayRandomHit();
    }
    
    public void OnPlayerDamaged(float damage)
    {
        if (gameUI != null)
        {
            float currentHealth = gameUI.GetHealthPercentage() * 100f;
            gameUI.UpdateHealth(currentHealth - damage);
        }
        
        if (audioManager != null)
            audioManager.PlayRandomHit();
    }
    
    public void OnWeaponFired()
    {
        if (audioManager != null)
            audioManager.PlayRandomGunshot();
    }
    
    public void OnWeaponReloaded()
    {
        if (audioManager != null)
            audioManager.PlayRandomReload();
    }
    
    public void OnPlayerFootstep()
    {
        if (audioManager != null)
            audioManager.PlayRandomFootstep();
    }
    
    public void OnWaveCompleted(int waveNumber)
    {
        if (gameUI != null)
        {
            gameUI.UpdateWave(waveNumber + 1);
            gameUI.ShowNotification($"¡Oleada {waveNumber} completada!");
        }
        
        // Reproducir sonido de victoria o notificación
        if (audioManager != null)
            audioManager.PlayNotificationSound();
    }
    
    public void OnScoreChanged(int newScore)
    {
        if (gameUI != null)
            gameUI.UpdateScore(newScore);
    }
    
    #endregion
    
    #region Integración con Sistemas Existentes
    
    /// <summary>
    /// Método para integrar con el GunScript existente
    /// </summary>
    public void RegisterGunEvents(GunScript gun)
    {
        if (gun != null)
        {
            Debug.Log("Registrando eventos del arma");
            // Los eventos se manejarán a través de los métodos públicos
        }
    }
    
    /// <summary>
    /// Método para integrar con el PlayerMovementScript existente
    /// </summary>
    public void RegisterPlayerEvents(PlayerMovementScript player)
    {
        if (player != null)
        {
            Debug.Log("Registrando eventos del jugador");
            // Los eventos se manejarán a través de los métodos públicos
        }
    }
    
    #endregion
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Nivel cargado: {scene.buildIndex}");
        
        // Reconfigurar sistemas cuando se carga un nuevo nivel
        SetupSystems();
        
        // Determinar si estamos en menú o en juego
        if (IsInMenuScene())
        {
            SetGameActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            SetGameActive(true);
            if (!isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    void OnDestroy()
    {
        // Limpiar referencias
        pauseMenu = null;
        gameUI = null;
        audioManager = null;
        
        Debug.Log("GameController destruido");
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !IsInMenuScene())
        {
            PauseGame();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !IsInMenuScene())
        {
            PauseGame();
        }
    }
}
