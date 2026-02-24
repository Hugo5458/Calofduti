using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WavePanel : MonoBehaviour
{
    [Header("Configuración del Panel")]
    public GameObject wavePanel;
    public Text waveNumberText;
    public float displayDuration = 3f;
    
    [Header("Efectos")]
    public Animator panelAnimator;
    public AudioClip waveStartSound;
    
    private AudioSource audioSource;
    private bool isShowing = false;
    
    void Start()
    {
        // Ocultar panel al inicio
        if (wavePanel != null)
        {
            wavePanel.SetActive(false);
        }
        
        // Configurar AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Muestra el panel de nueva ronda
    /// </summary>
    public void ShowWavePanel(int waveNumber)
    {
        if (isShowing) return;
        
        StartCoroutine(ShowWaveCoroutine(waveNumber));
    }
    
    System.Collections.IEnumerator ShowWaveCoroutine(int waveNumber)
    {
        isShowing = true;
        
        // Mostrar panel
        if (wavePanel != null)
        {
            wavePanel.SetActive(true);
            
            // Actualizar texto
            if (waveNumberText != null)
            {
                waveNumberText.text = $"RONDA N°{waveNumber}";
            }
            
            // Reproducir animación si existe
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("Show");
            }
            
            // Reproducir sonido
            if (waveStartSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(waveStartSound);
            }
        }
        
        // Esperar el tiempo de visualización
        yield return new WaitForSeconds(displayDuration);
        
        // Ocultar panel
        if (wavePanel != null)
        {
            // Reproducir animación de salida si existe
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("Hide");
            }
            
            yield return new WaitForSeconds(0.5f); // Tiempo para animación de salida
            
            wavePanel.SetActive(false);
        }
        
        isShowing = false;
    }
    
    /// <summary>
    /// Muestra panel personalizado
    /// </summary>
    public void ShowCustomPanel(string message, float duration = 3f)
    {
        StartCoroutine(ShowCustomCoroutine(message, duration));
    }
    
    System.Collections.IEnumerator ShowCustomCoroutine(string message, float duration)
    {
        if (isShowing) yield break;
        
        isShowing = true;
        
        if (wavePanel != null)
        {
            wavePanel.SetActive(true);
            
            if (waveNumberText != null)
            {
                waveNumberText.text = message;
            }
            
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("Show");
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        if (wavePanel != null)
        {
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("Hide");
            }
            
            yield return new WaitForSeconds(0.5f);
            wavePanel.SetActive(false);
        }
        
        isShowing = false;
    }
}
