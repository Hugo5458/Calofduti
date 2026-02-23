using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tool automática que crea el menú de pausa más profesional al presionar ESC
/// </summary>
public class AutoPauseMenuTool : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Color backgroundColor = new Color(0, 0, 0, 0.85f);
    public Color buttonColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color buttonHoverColor = new Color(0.2f, 0.4f, 0.8f, 1f);
    public Color textColor = Color.white;
    public Color titleColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    
    [Header("Animación")]
    public bool enableAnimations = true;
    public float animationSpeed = 0.3f;
    
    // Canvas y paneles
    private Canvas pauseCanvas;
    private GameObject mainPanel;
    private GameObject settingsPanel;
    private GameObject confirmPanel;
    
    // Botones principales
    private Button resumeButton;
    private Button settingsButton;
    private Button restartButton;
    private Button mainMenuButton;
    private Button quitButton;
    
    // Botones de configuración
    private Button backButton;
    private Slider volumeSlider;
    private Slider sensitivitySlider;
    private Toggle fullscreenToggle;
    
    // Botones de confirmación
    private Button confirmYesButton;
    private Button confirmNoButton;
    private TextMeshProUGUI confirmText;
    
    // Estado
    private bool isPaused = false;
    private Vector3 originalButtonScale;
    private string pendingAction = "";
    
    void Start()
    {
        CreatePauseMenu();
        HideAllPanels();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    void CreatePauseMenu()
    {
        Debug.Log("🎮 Creando menú de pausa profesional...");
        
        // Crear Canvas principal
        pauseCanvas = CreateCanvas();
        
        // Crear paneles
        mainPanel = CreateMainPanel();
        settingsPanel = CreateSettingsPanel();
        confirmPanel = CreateConfirmPanel();
        
        // Ocultar inicialmente
        HideAllPanels();
        
        Debug.Log("✅ Menú de pausa creado exitosamente");
    }
    
    Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("AutoPauseCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return canvas;
    }
    
    GameObject CreateMainPanel()
    {
        GameObject panel = CreatePanel("MainPanel", pauseCanvas.transform);
        
        // Contenedor principal
        GameObject container = CreateVerticalContainer("MainContainer", panel.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(400, 500);
        
        // Título espectacular
        CreateTitle("⏸️ JUEGO PAUSADO", container.transform, 48, titleColor);
        CreateSpacer(container.transform, 30);
        
        // Línea separadora
        CreateSeparator(container.transform);
        CreateSpacer(container.transform, 20);
        
        // Botones principales con diseño increíble
        resumeButton = CreateAwesomeButton("ResumeButton", container.transform, "▶️ REANUDAR", 60);
        settingsButton = CreateAwesomeButton("SettingsButton", container.transform, "⚙️ CONFIGURACIÓN", 55);
        restartButton = CreateAwesomeButton("RestartButton", container.transform, "🔄 REINICIAR", 50);
        mainMenuButton = CreateAwesomeButton("MainMenuButton", container.transform, "🏠 MENÚ PRINCIPAL", 45);
        quitButton = CreateAwesomeButton("QuitButton", container.transform, "❌ SALIR", 40);
        
        // Espaciado final
        CreateSpacer(container.transform, 20);
        
        // Texto inferior
        CreateText("FooterText", container.transform, "Presiona ESC para reanudar", 16, new Color(0.6f, 0.6f, 0.6f, 1f));
        
        SetupMainButtonEvents();
        
        return panel;
    }
    
    GameObject CreateSettingsPanel()
    {
        GameObject panel = CreatePanel("SettingsPanel", pauseCanvas.transform);
        
        GameObject container = CreateVerticalContainer("SettingsContainer", panel.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(450, 400);
        
        // Título
        CreateTitle("⚙️ CONFIGURACIÓN", container.transform, 36, titleColor);
        CreateSpacer(container.transform, 25);
        
        // Volumen
        CreateText("VolumeLabel", container.transform, "🔊 Volumen General", 18, textColor);
        volumeSlider = CreateSlider("VolumeSlider", container.transform, 0f, 1f, AudioListener.volume);
        CreateSpacer(container.transform, 20);
        
        // Sensibilidad
        CreateText("SensitivityLabel", container.transform, "🎯 Sensibilidad del Ratón", 18, textColor);
        float currentSens = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        sensitivitySlider = CreateSlider("SensitivitySlider", container.transform, 0.5f, 10f, currentSens);
        CreateSpacer(container.transform, 20);
        
        // Pantalla completa
        fullscreenToggle = CreateToggle("FullscreenToggle", container.transform, "🖥️ Pantalla Completa", Screen.fullScreen);
        CreateSpacer(container.transform, 25);
        
        // Botón volver
        backButton = CreateAwesomeButton("BackButton", container.transform, "⬅️ VOLVER", 50);
        
        SetupSettingsEvents();
        
        return panel;
    }
    
    GameObject CreateConfirmPanel()
    {
        GameObject panel = CreatePanel("ConfirmPanel", pauseCanvas.transform);
        
        GameObject container = CreateVerticalContainer("ConfirmContainer", panel.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(350, 200);
        
        // Fondo más oscuro para confirmación
        Image panelBg = panel.GetComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.95f);
        
        // Texto de confirmación
        confirmText = CreateText("ConfirmText", container.transform, "¿Estás seguro?", 24, textColor);
        CreateSpacer(container.transform, 30);
        
        // Contenedor de botones
        GameObject buttonContainer = CreateHorizontalContainer("ConfirmButtonContainer", container.transform);
        
        confirmYesButton = CreateAwesomeButton("ConfirmYes", buttonContainer.transform, "✅ SÍ", 45);
        confirmNoButton = CreateAwesomeButton("ConfirmNo", buttonContainer.transform, "❌ NO", 45);
        
        return panel;
    }
    
    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = backgroundColor;
        
        return panel;
    }
    
    GameObject CreateVerticalContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        
        ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        return container;
    }
    
    GameObject CreateHorizontalContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        
        HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.padding = new RectOffset(20, 20, 0, 0);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        
        return container;
    }
    
    void CreateTitle(string text, Transform parent, int fontSize, Color color)
    {
        GameObject titleObj = new GameObject(text);
        titleObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = text;
        titleText.fontSize = fontSize;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = color;
        titleText.alignment = TextAlignmentOptions.Center;
        
        // Efecto de sombra
        titleText.enableAutoSizing = false;
        
        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 20);
        
        return titleObj;
    }
    
    TextMeshProUGUI CreateText(string name, Transform parent, string content, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 10);
        
        return text;
    }
    
    Button CreateAwesomeButton(string name, Transform parent, string text, int height)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, height);
        
        // Imagen de fondo con bordes redondeados
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Configurar colores espectaculares
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = new Color(buttonHoverColor.r * 0.8f, buttonHoverColor.g * 0.8f, buttonHoverColor.b * 0.8f, 1f);
        colors.selectedColor = buttonHoverColor;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        
        // Texto del botón
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = (int)(height * 0.35f);
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = textColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 5);
        textRect.offsetMax = new Vector2(-15, -5);
        
        // Layout element
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
        
        // Guardar escala original para animaciones
        originalButtonScale = Vector3.one;
        
        return button;
    }
    
    Slider CreateSlider(string name, Transform parent, float minValue, float maxValue, float defaultValue)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);
        
        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 25);
        
        LayoutElement layout = sliderObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 25;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = defaultValue;
        slider.wholeNumbers = false;
        
        // Crear visual del slider
        CreateSliderVisuals(slider);
        
        return slider;
    }
    
    void CreateSliderVisuals(Slider slider)
    {
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(slider.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.4f);
        bgRect.anchorMax = new Vector2(1, 0.6f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(slider.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.4f);
        fillAreaRect.anchorMax = new Vector2(1, 0.6f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = buttonHoverColor;
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slider.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(25, 25);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        
        // Asignar referencias
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
    }
    
    Toggle CreateToggle(string name, Transform parent, string label, bool defaultValue)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        
        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 30);
        
        LayoutElement layout = toggleObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 30;
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = defaultValue;
        
        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(30, 30);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = buttonColor;
        
        // Checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform, false);
        RectTransform checkRect = checkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.offsetMin = new Vector2(5, 5);
        checkRect.offsetMax = new Vector2(-5, -5);
        Image checkImg = checkObj.AddComponent<Image>();
        checkImg.color = buttonHoverColor;
        
        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 18;
        labelText.color = textColor;
        labelText.alignment = TextAlignmentOptions.Left;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(40, 0);
        labelRect.offsetMax = Vector2.zero;
        
        // Configurar toggle
        toggle.targetGraphic = bgImg;
        toggle.graphic = bgImg;
        
        return toggle;
    }
    
    void CreateSeparator(Transform parent)
    {
        GameObject separator = new GameObject("Separator");
        separator.transform.SetParent(parent, false);
        
        RectTransform rect = separator.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 2);
        
        Image image = separator.AddComponent<Image>();
        image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        
        LayoutElement layout = separator.AddComponent<LayoutElement>();
        layout.preferredHeight = 2;
    }
    
    void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        
        RectTransform rect = spacer.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, height);
        
        LayoutElement layout = spacer.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
    }
    
    void SetupMainButtonEvents()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => { PlayClickSound(); ResumeGame(); });
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => { PlayClickSound(); ShowSettings(); });
            
        if (restartButton != null)
            restartButton.onClick.AddListener(() => { PlayClickSound(); ShowConfirmation("reiniciar el nivel", RestartLevel); });
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => { PlayClickSound(); ShowConfirmation("ir al menú principal", GoToMainMenu); });
            
        if (quitButton != null)
            quitButton.onClick.AddListener(() => { PlayClickSound(); ShowConfirmation("salir del juego", QuitGame); });
    }
    
    void SetupSettingsEvents()
    {
        if (backButton != null)
            backButton.onClick.AddListener(() => { PlayClickSound(); ShowMainPanel(); });
            
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(() => { PlayClickSound(); ExecutePendingAction(); });
            
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(() => { PlayClickSound(); HideConfirmation(); });
    }
    
    void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void PauseGame()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            ShowMainPanel();
            
            if (enableAnimations)
                AnimatePanelEntry(mainPanel);
                
            Debug.Log("⏸️ Juego pausado - Menú profesional activado");
        }
    }
    
    public void ResumeGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (enableAnimations)
                AnimatePanelExit(mainPanel, () => HideAllPanels());
            else
                HideAllPanels();
                
            Debug.Log("▶️ Juego reanudado");
        }
    }
    
    void ShowMainPanel()
    {
        HideAllPanels();
        mainPanel.SetActive(true);
    }
    
    void ShowSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
        
        if (enableAnimations)
            AnimatePanelEntry(settingsPanel);
    }
    
    void ShowConfirmation(string action, System.Action callback)
    {
        pendingAction = action;
        
        HideAllPanels();
        confirmPanel.SetActive(true);
        
        if (confirmText != null)
            confirmText.text = $"¿Estás seguro de que quieres {action}?";
            
        if (enableAnimations)
            AnimatePanelEntry(confirmPanel);
            
        // Configurar botón de confirmación
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(() => { PlayClickSound(); callback?.Invoke(); });
        }
    }
    
    void HideConfirmation()
    {
        if (enableAnimations)
            AnimatePanelExit(confirmPanel, () => {
                confirmPanel.SetActive(false);
                ShowMainPanel();
            });
        else
        {
            confirmPanel.SetActive(false);
            ShowMainPanel();
        }
    }
    
    void HideAllPanels()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        confirmPanel.SetActive(false);
    }
    
    void AnimatePanelEntry(GameObject panel)
    {
        if (panel == null) return;
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();
            
        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        LeanTween.scale(rect, Vector3.one, animationSpeed).setEaseOutBack();
        LeanTween.alpha(canvasGroup, 1f, animationSpeed).setEaseOutQuad();
    }
    
    void AnimatePanelExit(GameObject panel, System.Action onComplete)
    {
        if (panel == null) return;
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();
            
        LeanTween.scale(rect, Vector3.zero, animationSpeed).setEaseInBack();
        LeanTween.alpha(canvasGroup, 0f, animationSpeed).setEaseInQuad().setOnComplete(onComplete);
    }
    
    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
    
    void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        
        // Aplicar a MouseLookScript si existe
        MouseLookScript mls = FindObjectOfType<MouseLookScript>();
        if (mls != null)
        {
            mls.mouseSensitvity_notAiming = value * 150f;
            mls.mouseSensitvity_aiming = value * 75f;
        }
    }
    
    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    void PlayClickSound()
    {
        // Sonido simple de clic
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Click"), Camera.main.transform.position, 0.5f);
    }
    
    void RestartLevel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Inicio");
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
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    void OnDestroy()
    {
        // Limpiar listeners
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
