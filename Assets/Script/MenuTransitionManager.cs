using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Necesitarás instalar DoTween desde Asset Store o usar Unity's Animation

/// <summary>
/// Sistema de transiciones suaves para menús y UI.
/// Proporciona animaciones fluidas entre diferentes estados de la interfaz.
/// </summary>
public class MenuTransitionManager : MonoBehaviour
{
    public static MenuTransitionManager Instance;
    
    [Header("Configuración de Transiciones")]
    public float transitionDuration = 0.3f;
    public Ease transitionEase = Ease.OutQuad;
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
        
        // Resetear estado inicial
        ResetPanelState(panel);
        
        // Crear secuencia de animación
        Sequence sequence = DOTween.Sequence();
        
        if (useFadeTransition)
        {
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(panel);
            canvasGroup.alpha = 0f;
            sequence.Append(canvasGroup.DOFade(1f, fadeInDuration));
        }
        
        if (useScaleTransition)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.localScale = Vector3.zero;
            sequence.Insert(0f, rect.DOScale(1f, transitionDuration).SetEase(transitionEase));
        }
        
        if (useSlideTransition)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition - slideDirection * 100f;
            rect.anchoredPosition = startPos;
            sequence.Insert(0f, rect.DOAnchorPos(startPos + slideDirection * 100f, transitionDuration).SetEase(transitionEase));
        }
        
        if (onComplete != null)
            sequence.OnComplete(() => onComplete());
    }
    
    /// <summary>
    /// Oculta un panel con transición suave
    /// </summary>
    public void HidePanel(GameObject panel, System.Action onComplete = null)
    {
        if (panel == null) return;
        
        // Crear secuencia de animación
        Sequence sequence = DOTween.Sequence();
        
        if (useFadeTransition)
        {
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(panel);
            sequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));
        }
        
        if (useScaleTransition)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            sequence.Insert(0f, rect.DOScale(0f, transitionDuration).SetEase(transitionEase));
        }
        
        if (useSlideTransition)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            Vector2 endPos = rect.anchoredPosition - slideDirection * 100f;
            sequence.Insert(0f, rect.DOAnchorPos(endPos, transitionDuration).SetEase(transitionEase));
        }
        
        sequence.OnComplete(() => {
            panel.SetActive(false);
            onComplete?.Invoke();
        });
    }
    
    /// <summary>
    /// Cambia de un panel a otro con transición suave
    /// </summary>
    public void TransitionPanels(GameObject fromPanel, GameObject toPanel, System.Action onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();
        
        // Ocultar panel actual
        if (fromPanel != null)
        {
            if (useFadeTransition)
            {
                CanvasGroup fromCanvasGroup = GetOrAddCanvasGroup(fromPanel);
                sequence.Append(fromCanvasGroup.DOFade(0f, fadeOutDuration));
            }
            
            if (useScaleTransition)
            {
                RectTransform fromRect = fromPanel.GetComponent<RectTransform>();
                sequence.Insert(0f, fromRect.DOScale(0.8f, transitionDuration).SetEase(transitionEase));
            }
        }
        
        // Pequeña pausa entre transiciones
        sequence.AppendInterval(0.05f);
        
        // Mostrar nuevo panel
        if (toPanel != null)
        {
            sequence.AppendCallback(() => {
                toPanel.SetActive(true);
                ResetPanelState(toPanel);
            });
            
            if (useFadeTransition)
            {
                CanvasGroup toCanvasGroup = GetOrAddCanvasGroup(toPanel);
                toCanvasGroup.alpha = 0f;
                sequence.Append(toCanvasGroup.DOFade(1f, fadeInDuration));
            }
            
            if (useScaleTransition)
            {
                RectTransform toRect = toPanel.GetComponent<RectTransform>();
                toRect.localScale = Vector3.zero;
                sequence.Insert(sequence.Duration() - transitionDuration, toRect.DOScale(1f, transitionDuration).SetEase(transitionEase));
            }
        }
        
        if (onComplete != null)
            sequence.OnComplete(() => onComplete());
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
            rect.DOScale(1.05f, 0.1f).SetEase(Ease.OutQuad);
            
            if (image != null)
            {
                Color targetColor = Color.white;
                image.DOColor(targetColor, 0.2f).SetEase(Ease.OutQuad);
            }
        }
        else
        {
            // Efecto de salida
            rect.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
            
            if (image != null)
            {
                Color originalColor = button.colors.normalColor;
                image.DOColor(originalColor, 0.2f).SetEase(Ease.OutQuad);
            }
        }
    }
    
    /// <summary>
    /// Aplica efecto de presión a un botón
    /// </summary>
    public void OnButtonPress(Button button)
    {
        if (button == null) return;
        
        RectTransform rect = button.GetComponent<RectTransform>();
        
        // Efecto de presión
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rect.DOScale(0.95f, 0.05f).SetEase(Ease.OutQuad));
        sequence.Append(rect.DOScale(1.05f, 0.05f).SetEase(Ease.OutQuad));
        sequence.Append(rect.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
    }
    
    #endregion
    
    #region Transiciones de UI Elements
    
    /// <summary>
    /// Anima la aparición de texto
    /// </summary>
    public void ShowText(TextMeshProUGUI text, float delay = 0f, System.Action onComplete = null)
    {
        if (text == null) return;
        
        text.gameObject.SetActive(true);
        
        Sequence sequence = DOTween.Sequence();
        
        if (delay > 0)
            sequence.AppendInterval(delay);
        
        // Efecto de máquina de escribir
        text.alpha = 0f;
        sequence.Append(text.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        
        // Pequeño escalado
        RectTransform rect = text.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;
        rect.localScale = originalScale * 0.8f;
        sequence.Insert(0f, rect.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack));
        
        if (onComplete != null)
            sequence.OnComplete(() => onComplete());
    }
    
    /// <summary>
    /// Anima la desaparición de texto
    /// </summary>
    public void HideText(TextMeshProUGUI text, System.Action onComplete = null)
    {
        if (text == null) return;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(text.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
        
        RectTransform rect = text.GetComponent<RectTransform>();
        sequence.Insert(0f, rect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad));
        
        sequence.OnComplete(() => {
            text.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
    
    /// <summary>
    /// Efecto de pulsación para elementos importantes
    /// </summary>
    public void PulseElement(GameObject element, int pulses = 2, System.Action onComplete = null)
    {
        if (element == null) return;
        
        RectTransform rect = element.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;
        
        Sequence sequence = DOTween.Sequence();
        
        for (int i = 0; i < pulses; i++)
        {
            sequence.Append(rect.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutQuad));
            sequence.Append(rect.DOScale(originalScale, 0.2f).SetEase(Ease.InQuad));
        }
        
        if (onComplete != null)
            sequence.OnComplete(() => onComplete());
    }
    
    #endregion
    
    #region Transiciones de Notificaciones
    
    /// <summary>
    /// Muestra una notificación con animación
    /// </summary>
    public void ShowNotification(GameObject notificationPanel, float duration = 3f, System.Action onComplete = null)
    {
        if (notificationPanel == null) return;
        
        notificationPanel.SetActive(true);
        
        RectTransform rect = notificationPanel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = GetOrAddCanvasGroup(notificationPanel);
        
        // Posición inicial (arriba de la pantalla)
        rect.anchoredPosition = new Vector2(0, 100f);
        canvasGroup.alpha = 0f;
        
        Sequence sequence = DOTween.Sequence();
        
        // Animación de entrada
        sequence.Append(rect.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutBack));
        sequence.Insert(0f, canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad));
        
        // Esperar
        sequence.AppendInterval(duration);
        
        // Animación de salida
        sequence.Append(rect.DOAnchorPosY(100f, 0.3f).SetEase(Ease.InQuad));
        sequence.Insert(sequence.Duration() - 0.2f, canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
        
        sequence.OnComplete(() => {
            notificationPanel.SetActive(false);
            onComplete?.Invoke();
        });
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
    
    #region Métodos de Conveniencia
    
    /// <summary>
    /// Crea una transición de fundido a negro
    /// </summary>
    public void FadeToBlack(float duration, System.Action onComplete = null)
    {
        // Crear panel de fundido si no existe
        GameObject fadePanel = GameObject.Find("FadePanel");
        if (fadePanel == null)
        {
            fadePanel = CreateFadePanel();
        }
        
        fadePanel.SetActive(true);
        Image image = fadePanel.GetComponent<Image>();
        
        image.DOFade(1f, duration).SetEase(Ease.InQuad).OnComplete(() => {
            onComplete?.Invoke();
        });
    }
    
    /// <summary>
    /// Crea una transición de fundido desde negro
    /// </summary>
    public void FadeFromBlack(float duration, System.Action onComplete = null)
    {
        GameObject fadePanel = GameObject.Find("FadePanel");
        if (fadePanel == null)
        {
            fadePanel = CreateFadePanel();
        }
        
        fadePanel.SetActive(true);
        Image image = fadePanel.GetComponent<Image>();
        image.color = Color.black;
        
        image.DOFade(0f, duration).SetEase(Ease.OutQuad).OnComplete(() => {
            fadePanel.SetActive(false);
            onComplete?.Invoke();
        });
    }
    
    GameObject CreateFadePanel()
    {
        GameObject panel = new GameObject("FadePanel");
        
        // Canvas
        Canvas canvas = panel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        CanvasScaler scaler = panel.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        panel.AddComponent<GraphicRaycaster>();
        
        // Panel
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = Color.black;
        
        return panel;
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Limpiar todos los tweens al destruir
        DOTween.KillAll();
    }
}
