using UnityEngine;

/// <summary>
/// GameController simple que integra los sistemas básicos
/// </summary>
public class SimpleGameController : MonoBehaviour
{
    public static SimpleGameController Instance;
    
    [Header("Referencias a Sistemas")]
    public AutoPauseMenuTool pauseMenu;
    public SimpleGameUI gameUI;
    public SimpleAudioManager audioManager;
    
    // Estado del juego
    private bool isGameActive = false;
    private bool isPaused = false;
    
    void Awake()
    {
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
        SetupSystems();
        ConfigureInitialState();
    }
    
    void InitializeGame()
    {
        Debug.Log("SimpleGameController inicializado");
        Time.timeScale = 1f;
    }
    
    void SetupSystems()
    {
        // Buscar sistemas si no están asignados
        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<AutoPauseMenuTool>();
            
        if (gameUI == null)
            gameUI = FindObjectOfType<SimpleGameUI>();
            
        if (audioManager == null)
            audioManager = SimpleAudioManager.Instance;
        
        Debug.Log("Sistemas configurados");
    }
    
    void ConfigureInitialState()
    {
        if (!IsInMenuScene())
        {
            SetGameActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            SetGameActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void Update()
    {
        UpdateGameState();
        HandleInput();
    }
    
    void UpdateGameState()
    {
        if (pauseMenu != null)
        {
            isPaused = pauseMenu.IsPaused();
        }
    }
    
    void HandleInput()
    {
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
    
    #region Control del Juego
    
    public void PauseGame()
    {
        if (!isPaused && !IsInMenuScene())
        {
            isPaused = true;
            
            if (pauseMenu != null)
                pauseMenu.PauseGame();
                
            Debug.Log("Juego pausado por SimpleGameController");
        }
    }
    
    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            
            if (pauseMenu != null)
                pauseMenu.ResumeGame();
                
            Debug.Log("Juego reanudado por SimpleGameController");
        }
    }
    
    public void RestartLevel()
    {
        Debug.Log("Reiniciando nivel...");
        Time.timeScale = 1f;
        
        if (gameUI != null)
            gameUI.UpdateHealth(100f);
            
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
        }
        else
        {
            Debug.Log("Juego desactivado");
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
    
    #endregion
    
    void OnLevelWasLoaded(int level)
    {
        Debug.Log($"Nivel cargado: {level}");
        SetupSystems();
        ConfigureInitialState();
    }
    
    void OnDestroy()
    {
        pauseMenu = null;
        gameUI = null;
        audioManager = null;
        Debug.Log("SimpleGameController destruido");
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
