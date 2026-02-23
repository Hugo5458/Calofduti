using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de pausa simple y robusto sin dependencias externas
/// </summary>
public class SimplePauseMenu : MonoBehaviour
{
    public static SimplePauseMenu Instance;
    
    [Header("Referencias UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Configuración")]
    public KeyCode pauseKey = KeyCode.Escape;
    
    private bool isPaused = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Ocultar panel inicialmente
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        // Configurar botones
        SetupButtons();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }
    
    void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => Debug.Log("Configuración - Implementar"));
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Mostrar panel
        if (pausePanel != null)
            pausePanel.SetActive(true);
            
        Debug.Log("Juego pausado");
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        // Ocultar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Ocultar panel
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        Debug.Log("Juego reanudado");
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Inicio");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
}
