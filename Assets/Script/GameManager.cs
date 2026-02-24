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
    
    [Header("Sistema de Dificultad")]
    public float zombieSpeedIncrease = 0.5f; // Aumento de velocidad por oleada
    public float zombieDamageIncrease = 2f; // Aumento de daño por oleada
    public float zombieHealthIncrease = 5f; // Aumento de salud por oleada
    public int zombiesPerWaveIncrease = 2; // Zombies adicionales por oleada
    
    [Header("UI")]
    public Text scoreText;
    public Text waveText;
    public Text zombiesKilledText;
    public GameObject pauseMenu;
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public WavePanel wavePanel; // Nuevo panel de rondas
    
    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip waveStartSound;
    public AudioClip gameOverSound;
    
    private AudioSource audioSource;
    private bool isPaused = false;
    private bool isGameOver = false;
    private ZombieSpawner spawner;
    private int zombiesInCurrentWave = 0;
    private int zombiesKilledInCurrentWave = 0;
    
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
        
        // NOTA: La pausa con ESC la gestiona AutoPauseMenuTool.cs
        // No duplicar aquí para evitar conflictos
    }
    
    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }
    
    public void ZombieKilled()
    {
        zombiesKilled++;
        zombiesKilledInCurrentWave++;
        
        // Notificar al spawner
        if (spawner != null)
        {
            spawner.ZombieDied();
        }
        
        // Verificar si se completó la oleada
        CheckWaveCompletion();
        
        UpdateUI();
    }
    
    /// <summary>
    /// Verifica si se ha completado la oleada actual
    /// </summary>
    void CheckWaveCompletion()
    {
        if (spawner != null)
        {
            int zombiesAlive = spawner.GetZombiesAlive();
            
            // Si no quedan zombies y no hay más por spawnear, la oleada está completa
            if (zombiesAlive <= 0 && !spawner.IsSpawning())
            {
                // Incrementar ronda
                currentWave++;
                
                // Mostrar mensaje de oleada completada
                if (wavePanel != null)
                {
                    wavePanel.ShowCustomPanel($"¡OLEADA {currentWave - 1} COMPLETADA!", 2f);
                }
                
                // Iniciar nueva oleada después de un breve retraso
                StartCoroutine(StartNextWave());
            }
        }
    }
    
    System.Collections.IEnumerator StartNextWave()
    {
        yield return new WaitForSeconds(3f); // Esperar 3 segundos
        
        // Iniciar nueva oleada
        NewWave(currentWave);
        
        // Notificar al spawner que inicie la siguiente oleada
        if (spawner != null)
        {
            spawner.StartNextWave();
        }
    }
    
    public void NewWave(int wave)
    {
        currentWave = wave;
        
        // Reiniciar contador de zombies de esta oleada
        zombiesKilledInCurrentWave = 0;
        zombiesInCurrentWave = spawner != null ? spawner.GetZombiesAlive() : 0;
        
        // Mostrar panel de nueva ronda
        if (wavePanel != null)
        {
            wavePanel.ShowWavePanel(currentWave);
        }
        
        // Aplicar aumento de dificultad
        ApplyDifficultyScaling();
        
        if (waveStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(waveStartSound);
        }
        
        UpdateUI();
        
        Debug.Log("¡Oleada " + wave + " comenzando! Dificultad aumentada.");
    }
    
    /// <summary>
    /// Aplica el aumento de dificultad a todos los zombies en la escena
    /// </summary>
    void ApplyDifficultyScaling()
    {
        ZombieAI[] allZombies = FindObjectsOfType<ZombieAI>();
        ZombieHealth[] allZombieHealth = FindObjectsOfType<ZombieHealth>();
        
        foreach (ZombieAI zombie in allZombies)
        {
            // Usar el nuevo método para aumentar estadísticas
            zombie.IncreaseStats(zombieSpeedIncrease, zombieDamageIncrease);
        }
        
        foreach (ZombieHealth health in allZombieHealth)
        {
            // Aumentar salud máxima
            health.maxHealth += zombieHealthIncrease;
            health.currentHealth = health.maxHealth; // Restaurar salud completa
        }
        
        // Aumentar cantidad de zombies para próximas oleadas
        if (spawner != null)
        {
            spawner.IncreaseWaveDifficulty(zombiesPerWaveIncrease);
        }
        
        Debug.Log($"Dificultad aumentada en oleada {currentWave}: Velocidad +{zombieSpeedIncrease}, Daño +{zombieDamageIncrease}, Salud +{zombieHealthIncrease}, Zombies +{zombiesPerWaveIncrease}");
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
