using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Sistema de transiciones suaves para menús y UI.
/// Proporciona animaciones fluidas entre diferentes estados de la interfaz.
/// </summary>
public class MenuTransitionManager : MonoBehaviour
{
    public static MenuTransitionManager Instance;
    
    [Header("Configuración de Transiciones")]
    public float transitionDuration = 0.3f;
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 0.2f;
    
    [Header("Efectos de Transición")]
    public bool useScaleTransition = true;
    public bool useFadeTransition = true;
    public bool useSlideTransition = false;
    public Vector2 slideDirection = Vector2.up;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #region Transiciones de Panel
    
    /// <summary>
    /// Muestra un panel con transición suave
    /// </summary>
    public void ShowPanel(GameObject panel, System.Action onComplete = null)
    {
        if (panel == null) return;
        
        panel.SetActive(true);
        ResetPanelState(panel);
        
        // Iniciar animación
        StartCoroutine(ShowPanelCoroutine(panel, onComplete));
    }
    
    IEnumerator ShowPanelCoroutine(GameObject panel, System.Action onComplete)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(panel);
        
        // Estado inicial
        if (useFadeTransition)
            canvasGroup.alpha = 0f;
            
        if (useScaleTransition)
            rect.localScale = Vector3.zero;
            
        if (useSlideTransition)
            rect.anchoredPosition -= slideDirection * 100f;
        
        float elapsed = 0f;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;
            
            // Animación suave (ease-out)
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            if (useFadeTransition && canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothProgress);
                
            if (useScaleTransition)
                rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smoothProgress);
                
            if (useSlideTransition)
            {
                Vector2 targetPos = Vector2.zero;
                Vector2 startPos = targetPos - slideDirection * 100f;
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothProgress);
            }
            
            yield return null;
        }
        
        // Asegurar estado final
        if (useFadeTransition && canvasGroup != null)
            canvasGroup.alpha = 1f;
        if (useScaleTransition)
            rect.localScale = Vector3.one;
        if (useSlideTransition)
            rect.anchoredPosition = Vector2.zero;
        
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Oculta un panel con transición suave
    /// </summary>
    public void HidePanel(GameObject panel, System.Action onComplete = null)
    {
        if (panel == null) return;
        
        StartCoroutine(HidePanelCoroutine(panel, onComplete));
    }
    
    IEnumerator HidePanelCoroutine(GameObject panel, System.Action onComplete)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(panel);
        
        float elapsed = 0f;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;
            
            // Animación suave (ease-in)
            float smoothProgress = Mathf.Pow(progress, 3f);
            
            if (useFadeTransition && canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, smoothProgress);
                
            if (useScaleTransition)
                rect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, smoothProgress);
                
            if (useSlideTransition)
            {
                Vector2 currentPos = rect.anchoredPosition;
                Vector2 endPos = currentPos - slideDirection * 100f;
                rect.anchoredPosition = Vector2.Lerp(currentPos, endPos, smoothProgress);
            }
            
            yield return null;
        }
        
        panel.SetActive(false);
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Cambia de un panel a otro con transición suave
    /// </summary>
    public void TransitionPanels(GameObject fromPanel, GameObject toPanel, System.Action onComplete = null)
    {
        StartCoroutine(TransitionPanelsCoroutine(fromPanel, toPanel, onComplete));
    }
    
    IEnumerator TransitionPanelsCoroutine(GameObject fromPanel, GameObject toPanel, System.Action onComplete)
    {
        // Ocultar panel actual
        if (fromPanel != null)
        {
            RectTransform fromRect = fromPanel.GetComponent<RectTransform>();
            CanvasGroup fromCanvasGroup = GetOrAddCanvasGroup(fromPanel);
            
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeOutDuration;
                float smoothProgress = Mathf.Pow(progress, 3f);
                
                if (useFadeTransition && fromCanvasGroup != null)
                    fromCanvasGroup.alpha = Mathf.Lerp(1f, 0f, smoothProgress);
                    
                if (useScaleTransition)
                    fromRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, smoothProgress);
                
                yield return null;
            }
        }
        
        // Pequeña pausa
        yield return new WaitForSeconds(0.05f);
        
        // Mostrar nuevo panel
        if (toPanel != null)
        {
            toPanel.SetActive(true);
            ResetPanelState(toPanel);
            
            RectTransform toRect = toPanel.GetComponent<RectTransform>();
            CanvasGroup toCanvasGroup = GetOrAddCanvasGroup(toPanel);
            
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeInDuration;
                float smoothProgress = 1f - Mathf.Pow(1f - progress, 3f);
                
                if (useFadeTransition && toCanvasGroup != null)
                    toCanvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothProgress);
                    
                if (useScaleTransition)
                    toRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smoothProgress);
                
                yield return null;
            }
        }
        
        onComplete?.Invoke();
    }
    
    #endregion
    
    #region Transiciones de Botones
    
    /// <summary>
    /// Aplica efecto hover a un botón
    /// </summary>
    public void OnButtonHover(Button button, bool isEntering)
    {
        if (button == null) return;
        
        RectTransform rect = button.GetComponent<RectTransform>();
        Image image = button.GetComponent<Image>();
        
        if (isEntering)
        {
            // Efecto de entrada
            StartCoroutine(ButtonHoverCoroutine(rect, image, Vector3.one * 1.05f, Color.white));
        }
        else
        {
            // Efecto de salida
            Color originalColor = button.colors.normalColor;
            StartCoroutine(ButtonHoverCoroutine(rect, image, Vector3.one, originalColor));
        }
    }
    
    IEnumerator ButtonHoverCoroutine(RectTransform rect, Image image, Vector3 targetScale, Color targetColor)
    {
        Vector3 startScale = rect.localScale;
        Color startColor = image != null ? image.color : Color.white;
        
        float elapsed = 0f;
        float duration = 0.1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float smoothProgress = 1f - Mathf.Pow(1f - progress, 2f);
            
            rect.localScale = Vector3.Lerp(startScale, targetScale, smoothProgress);
            
            if (image != null)
                image.color = Color.Lerp(startColor, targetColor, smoothProgress);
            
            yield return null;
        }
        
        rect.localScale = targetScale;
        if (image != null)
            image.color = targetColor;
    }
    
    /// <summary>
    /// Aplica efecto de presión a un botón
    /// </summary>
    public void OnButtonPress(Button button)
    {
        if (button == null) return;
        
        StartCoroutine(ButtonPressCoroutine(button.GetComponent<RectTransform>()));
    }
    
    IEnumerator ButtonPressCoroutine(RectTransform rect)
    {
        Vector3 originalScale = rect.localScale;
        
        // Presión
        float elapsed = 0f;
        while (elapsed < 0.05f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / 0.05f;
            rect.localScale = Vector3.Lerp(originalScale, originalScale * 0.95f, progress);
            yield return null;
        }
        
        // Rebote
        elapsed = 0f;
        while (elapsed < 0.05f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / 0.05f;
            rect.localScale = Vector3.Lerp(originalScale * 0.95f, originalScale * 1.05f, progress);
            yield return null;
        }
        
        // Normal
        elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / 0.1f;
            rect.localScale = Vector3.Lerp(originalScale * 1.05f, originalScale, progress);
            yield return null;
        }
        
        rect.localScale = originalScale;
    }
    
    #endregion
    
    #region Utilidades
    
    CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }
    
    void ResetPanelState(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        
        // Resetear transform
        rect.localScale = Vector3.one;
        rect.anchoredPosition = Vector2.zero;
        
        // Resetear canvas group
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    
    #endregion
}
