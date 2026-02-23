using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sistema completo de menú de pausa para el juego.
/// Maneja la pausa del juego, navegación de menús y configuración.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject confirmationPanel;
    
    [Header("Botones del Menú Pausa")]
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Botones de Configuración")]
    public Button backButton;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;
    
    [Header("Botones de Confirmación")]
    public Button confirmYesButton;
    public Button confirmNoButton;
    public TextMeshProUGUI confirmationText;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip pauseOpenSound;
    public AudioClip pauseCloseSound;
    
    private AudioSource audioSource;
    private bool isPaused = false;
    private string pendingAction = "";
    
    // Constantes para PlayerPrefs
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUTE_KEY = "MuteAll";
    
    void Start()
    {
        InitializeComponents();
        SetupButtons();
        LoadSettings();
        HideAllPanels();
    }
    
    void Update()
    {
        // Detectar tecla ESC para pausar/reanudar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    void InitializeComponents()
    {
        // Configurar AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        
        // Si no hay referencias asignadas, buscarlas automáticamente
        if (pausePanel == null)
            pausePanel = transform.Find("PausePanel")?.gameObject;
        if (settingsPanel == null)
            settingsPanel = transform.Find("SettingsPanel")?.gameObject;
        if (confirmationPanel == null)
            confirmationPanel = transform.Find("ConfirmationPanel")?.gameObject;
    }
    
    void SetupButtons()
    {
        // Botones del menú principal de pausa
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => PlayButtonSoundAndAction(ResumeGame));
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => PlayButtonSoundAndAction(OpenSettings));
            
        if (restartButton != null)
            restartButton.onClick.AddListener(() => PlayButtonSoundAndAction(() => ShowConfirmation("reiniciar el nivel", RestartLevel)));
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => PlayButtonSoundAndAction(() => ShowConfirmation("ir al menú principal", GoToMainMenu)));
            
        if (quitButton != null)
            quitButton.onClick.AddListener(() => PlayButtonSoundAndAction(() => ShowConfirmation("salir del juego", QuitGame)));
        
        // Botones de configuración
        if (backButton != null)
            backButton.onClick.AddListener(() => PlayButtonSoundAndAction(CloseSettings));
            
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        
        // Botones de confirmación
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(() => PlayButtonSoundAndAction(ExecutePendingAction));
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(() => PlayButtonSoundAndAction(HideConfirmation));
    }
    
    #region Control de Pausa
    
    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f; // Pausar el tiempo del juego
            
            // Mostrar cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Pausar audio
            if (AudioListener.pause != true)
                AudioListener.pause = true;
            
            // Reproducir sonido de pausa
            PlaySound(pauseOpenSound);
            
            // Mostrar panel de pausa
            ShowPausePanel();
            
            Debug.Log("Juego pausado");
        }
    }
    
    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f; // Reanudar el tiempo del juego
            
            // Ocultar cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Reanudar audio
            if (AudioListener.pause == true)
                AudioListener.pause = false;
            
            // Reproducir sonido de reanudar
            PlaySound(pauseCloseSound);
            
            // Ocultar todos los paneles
            HideAllPanels();
            
            Debug.Log("Juego reanudado");
        }
    }
    
    #endregion
    
    #region Navegación de Menús
    
    void ShowPausePanel()
    {
        HideAllPanels();
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    void ShowConfirmation(string action, System.Action callback)
    {
        pendingAction = action;
        
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
            
        if (confirmationText != null)
            confirmationText.text = $"¿Estás seguro de que quieres {action}?";
            
        // Guardar la acción pendiente para ejecutarla si el usuario confirma
        if (confirmYesButton != null)
        {
            // Eliminar listeners anteriores para evitar duplicados
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(() => PlayButtonSoundAndAction(callback));
        }
    }
    
    void HideConfirmation()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
        pendingAction = "";
    }
    
    void HideAllPanels()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    #endregion
    
    #region Acciones del Menú
    
    void RestartLevel()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void GoToMainMenu()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        SceneManager.LoadScene("Inicio"); // Usar el nombre de la escena del menú principal
    }
    
    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void ExecutePendingAction()
    {
        if (pendingAction.Contains("reiniciar"))
            RestartLevel();
        else if (pendingAction.Contains("menú principal"))
            GoToMainMenu();
        else if (pendingAction.Contains("salir"))
            QuitGame();
            
        HideConfirmation();
    }
    
    #endregion
    
    #region Configuración de Audio
    
    void LoadSettings()
    {
        // Cargar volúmenes guardados
        float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        bool isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        
        // Aplicar valores a los sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVol;
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVol;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVol;
        if (muteToggle != null)
            muteToggle.isOn = isMuted;
        
        // Aplicar volúmenes inmediatamente
        ApplyVolumes();
    }
    
    void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        ApplyVolumes();
    }
    
    void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        ApplyVolumes();
    }
    
    void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        ApplyVolumes();
    }
    
    void OnMuteToggled(bool isMuted)
    {
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
        ApplyVolumes();
    }
    
    void ApplyVolumes()
    {
        float masterVol = masterVolumeSlider != null ? masterVolumeSlider.value : 1f;
        float musicVol = musicVolumeSlider != null ? musicVolumeSlider.value : 0.7f;
        float sfxVol = sfxVolumeSlider != null ? sfxVolumeSlider.value : 1f;
        bool isMuted = muteToggle != null ? muteToggle.isOn : false;
        
        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            // El volumen master afecta a todo
            AudioListener.volume = masterVol;
            
            // Aquí podrías aplicar volúmenes separados si tuvieras múltiples AudioSources
            // Por ahora, usamos el volumen master como control principal
        }
        
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Utilidades de Audio
    
    void PlayButtonSoundAndAction(System.Action action)
    {
        PlaySound(buttonClickSound);
        action?.Invoke();
    }
    
    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    #endregion
    
    #region Métodos Públicos para Acceso Externo
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Limpiar listeners para evitar memory leaks
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (restartButton != null) restartButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        if (quitButton != null) quitButton.onClick.RemoveAllListeners();
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        if (confirmYesButton != null) confirmYesButton.onClick.RemoveAllListeners();
        if (confirmNoButton != null) confirmNoButton.onClick.RemoveAllListeners();
    }
}
