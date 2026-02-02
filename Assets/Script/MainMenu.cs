using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;
    
    [Header("Opciones")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public Toggle fullscreenToggle;
    
    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip backgroundMusic;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Música de fondo
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Cargar configuración guardada
        LoadSettings();
        
        // Mostrar panel principal
        ShowMainPanel();
    }
    
    void PlayClickSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    public void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene("Zombies");
    }
    
    public void ShowOptions()
    {
        PlayClickSound();
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
    
    public void ShowCredits()
    {
        PlayClickSound();
        mainPanel.SetActive(false);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }
    
    public void ShowMainPanel()
    {
        PlayClickSound();
        mainPanel.SetActive(true);
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
    
    public void QuitGame()
    {
        PlayClickSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }
    
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    void LoadSettings()
    {
        // Volumen
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        AudioListener.volume = volume;
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        
        // Sensibilidad
        float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sensitivity;
        }
        
        // Pantalla completa
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = fullscreen;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = fullscreen;
        }
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }
}
