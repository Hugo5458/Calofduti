using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Estadísticas del Juego")]
    public int score = 0;
    public int zombiesKilled = 0;
    public int currentWave = 0;
    
    [Header("UI")]
    public Text scoreText;
    public Text waveText;
    public Text zombiesKilledText;
    public GameObject pauseMenu;
    public GameObject gameOverPanel;
    public Text finalScoreText;
    
    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip waveStartSound;
    public AudioClip gameOverSound;
    
    private AudioSource audioSource;
    private bool isPaused = false;
    private bool isGameOver = false;
    private ZombieSpawner spawner;
    
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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        spawner = FindObjectOfType<ZombieSpawner>();
        
        // Iniciar música de fondo
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Ocultar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Inicializar UI
        UpdateUI();
        
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        Time.timeScale = 1f;
    }
    
    void Update()
    {
        if (isGameOver) return;
        
        // Pausar con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }
    
    public void ZombieKilled()
    {
        zombiesKilled++;
        
        // Notificar al spawner
        if (spawner != null)
        {
            spawner.ZombieDied();
        }
        
        UpdateUI();
    }
    
    public void NewWave(int wave)
    {
        currentWave = wave;
        
        if (waveStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(waveStartSound);
        }
        
        UpdateUI();
        
        // Mostrar notificación de oleada (opcional)
        Debug.Log("¡Oleada " + wave + " comenzando!");
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Puntuación: " + score;
        }
        
        if (waveText != null)
        {
            waveText.text = "Oleada: " + currentWave;
        }
        
        if (zombiesKilledText != null)
        {
            zombiesKilledText.text = "Zombies: " + zombiesKilled;
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (pauseMenu != null)
                pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (pauseMenu != null)
                pauseMenu.SetActive(false);
        }
    }
    
    public void GameOver()
    {
        isGameOver = true;
        
        if (gameOverSound != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(gameOverSound);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = "Puntuación Final: " + score + "\nZombies Eliminados: " + zombiesKilled + "\nOleadas Superadas: " + currentWave;
        }
        
        Time.timeScale = 0f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
