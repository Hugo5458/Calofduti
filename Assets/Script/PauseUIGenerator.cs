using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Generador automático de UI para el sistema de pausa.
/// Crea todos los elementos visuales del menú de pausa programáticamente.
/// </summary>
public class PauseUIGenerator : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Color panelColor = new Color(0, 0, 0, 0.8f);
    public Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color buttonHoverColor = new Color(0.3f, 0.6f, 0.9f, 1f);
    public Color textColor = Color.white;
    public Font buttonFont;
    
    [Header("Referencias (se asignan automáticamente)")]
    public Canvas pauseCanvas;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject confirmationPanel;
    
    // Componentes del menú de pausa
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    // Componentes del menú de configuración
    public Button backButton;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteToggle;
    
    // Componentes del panel de confirmación
    public Button confirmYesButton;
    public Button confirmNoButton;
    public TextMeshProUGUI confirmationText;
    
    void Start()
    {
        if (pauseCanvas == null)
        {
            CreatePauseUI();
        }
        else
        {
            FindExistingUI();
        }
    }
    
    void CreatePauseUI()
    {
        Debug.Log("Creando UI de pausa automáticamente");
        
        // Crear Canvas principal
        pauseCanvas = CreateCanvas("PauseCanvas");
        
        // Crear paneles
        pausePanel = CreatePanel("PausePanel", pauseCanvas.transform);
        settingsPanel = CreatePanel("SettingsPanel", pauseCanvas.transform);
        confirmationPanel = CreatePanel("ConfirmationPanel", pauseCanvas.transform);
        
        // Crear contenido de los paneles
        CreatePauseMenuContent();
        CreateSettingsContent();
        CreateConfirmationContent();
        
        // Ocultar todos los paneles inicialmente
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        confirmationPanel.SetActive(false);
    }
    
    void FindExistingUI()
    {
        Debug.Log("Buscando UI existente");
        
        // Buscar canvas existente
        pauseCanvas = FindObjectOfType<Canvas>();
        
        if (pauseCanvas != null)
        {
            // Buscar paneles existentes
            pausePanel = pauseCanvas.transform.Find("PausePanel")?.gameObject;
            settingsPanel = pauseCanvas.transform.Find("SettingsPanel")?.gameObject;
            confirmationPanel = pauseCanvas.transform.Find("ConfirmationPanel")?.gameObject;
            
            // Buscar componentes existentes
            FindExistingComponents();
        }
    }
    
    void FindExistingComponents()
    {
        // Buscar botones del menú de pausa
        resumeButton = pausePanel?.transform.Find("ResumeButton")?.GetComponent<Button>();
        settingsButton = pausePanel?.transform.Find("SettingsButton")?.GetComponent<Button>();
        restartButton = pausePanel?.transform.Find("RestartButton")?.GetComponent<Button>();
        mainMenuButton = pausePanel?.transform.Find("MainMenuButton")?.GetComponent<Button>();
        quitButton = pausePanel?.transform.Find("QuitButton")?.GetComponent<Button>();
        
        // Buscar componentes de configuración
        backButton = settingsPanel?.transform.Find("BackButton")?.GetComponent<Button>();
        masterVolumeSlider = settingsPanel?.transform.Find("MasterVolumeSlider")?.GetComponent<Slider>();
        musicVolumeSlider = settingsPanel?.transform.Find("MusicVolumeSlider")?.GetComponent<Slider>();
        sfxVolumeSlider = settingsPanel?.transform.Find("SFXVolumeSlider")?.GetComponent<Slider>();
        muteToggle = settingsPanel?.transform.Find("MuteToggle")?.GetComponent<Toggle>();
        
        // Buscar componentes de confirmación
        confirmYesButton = confirmationPanel?.transform.Find("ConfirmYesButton")?.GetComponent<Button>();
        confirmNoButton = confirmationPanel?.transform.Find("ConfirmNoButton")?.GetComponent<Button>();
        confirmationText = confirmationPanel?.transform.Find("ConfirmationText")?.GetComponent<TextMeshProUGUI>();
    }
    
    #region Creación de Canvas y Paneles
    
    Canvas CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Muy alto para estar encima de todo
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return canvas;
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
        image.color = panelColor;
        
        return panel;
    }
    
    #endregion
    
    #region Creación de Contenido del Menú Pausa
    
    void CreatePauseMenuContent()
    {
        // Contenedor vertical para los botones
        GameObject buttonContainer = CreateVerticalLayoutGroup("ButtonContainer", pausePanel.transform);
        RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(300, 400);
        
        // Título
        CreateText("PauseTitle", buttonContainer.transform, "JUEGO PAUSADO", 36, FontStyle.Bold);
        
        // Espaciador
        CreateSpacer(buttonContainer.transform, 30);
        
        // Botones
        resumeButton = CreateButton("ResumeButton", buttonContainer.transform, "REANUDAR", 60);
        settingsButton = CreateButton("SettingsButton", buttonContainer.transform, "CONFIGURACIÓN", 55);
        restartButton = CreateButton("RestartButton", buttonContainer.transform, "REINICIAR", 50);
        mainMenuButton = CreateButton("MainMenuButton", buttonContainer.transform, "MENÚ PRINCIPAL", 45);
        quitButton = CreateButton("QuitButton", buttonContainer.transform, "SALIR", 40);
    }
    
    #endregion
    
    #region Creación de Contenido de Configuración
    
    void CreateSettingsContent()
    {
        // Contenedor vertical para la configuración
        GameObject settingsContainer = CreateVerticalLayoutGroup("SettingsContainer", settingsPanel.transform);
        RectTransform containerRect = settingsContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(400, 500);
        
        // Título
        CreateText("SettingsTitle", settingsContainer.transform, "CONFIGURACIÓN", 32, FontStyle.Bold);
        
        // Espaciador
        CreateSpacer(settingsContainer.transform, 20);
        
        // Volumen Master
        CreateText("MasterVolumeLabel", settingsContainer.transform, "Volumen Master", 18, FontStyle.Normal);
        masterVolumeSlider = CreateSlider("MasterVolumeSlider", settingsContainer.transform, 0f, 1f, 0.8f);
        
        // Espaciador
        CreateSpacer(settingsContainer.transform, 10);
        
        // Volumen Música
        CreateText("MusicVolumeLabel", settingsContainer.transform, "Volumen Música", 18, FontStyle.Normal);
        musicVolumeSlider = CreateSlider("MusicVolumeSlider", settingsContainer.transform, 0f, 1f, 0.7f);
        
        // Espaciador
        CreateSpacer(settingsContainer.transform, 10);
        
        // Volumen Efectos
        CreateText("SFXVolumeLabel", settingsContainer.transform, "Volumen Efectos", 18, FontStyle.Normal);
        sfxVolumeSlider = CreateSlider("SFXVolumeSlider", settingsContainer.transform, 0f, 1f, 0.8f);
        
        // Espaciador
        CreateSpacer(settingsContainer.transform, 15);
        
        // Mute Toggle
        muteToggle = CreateToggle("MuteToggle", settingsContainer.transform, "Silenciar Todo");
        
        // Espaciador
        CreateSpacer(settingsContainer.transform, 20);
        
        // Botón de volver
        backButton = CreateButton("BackButton", settingsContainer.transform, "VOLVER", 50);
    }
    
    #endregion
    
    #region Creación de Contenido de Confirmación
    
    void CreateConfirmationContent()
    {
        // Contenedor vertical para la confirmación
        GameObject confirmContainer = CreateVerticalLayoutGroup("ConfirmContainer", confirmationPanel.transform);
        RectTransform containerRect = confirmContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(350, 200);
        
        // Panel de fondo más oscuro
        Image confirmBg = confirmationPanel.GetComponent<Image>();
        confirmBg.color = new Color(0, 0, 0, 0.9f);
        
        // Texto de confirmación
        confirmationText = CreateText("ConfirmationText", confirmContainer.transform, "¿Estás seguro?", 20, FontStyle.Normal);
        
        // Espaciador
        CreateSpacer(confirmContainer.transform, 20);
        
        // Contenedor horizontal para botones
        GameObject buttonContainer = CreateHorizontalLayoutGroup("ConfirmButtonContainer", confirmContainer.transform);
        
        // Botones
        confirmYesButton = CreateButton("ConfirmYesButton", buttonContainer.transform, "SÍ", 45);
        confirmNoButton = CreateButton("ConfirmNoButton", buttonContainer.transform, "NO", 45);
    }
    
    #endregion
    
    #region Creación de Componentes UI
    
    GameObject CreateVerticalLayoutGroup(string name, Transform parent)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        
        ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        return container;
    }
    
    GameObject CreateHorizontalLayoutGroup(string name, Transform parent)
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
    
    TextMeshProUGUI CreateText(string name, Transform parent, string content, int fontSize, FontStyle fontStyle)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        
        if (buttonFont != null)
            text.font = buttonFont;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, fontSize + 10);
        
        return text;
    }
    
    Button CreateButton(string name, Transform parent, string text, int height)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, height);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Configurar colores del botón
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
        buttonText.fontSize = (int)(height * 0.4f);
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = textColor;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        if (buttonFont != null)
            buttonText.font = buttonFont;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        // Añadir LayoutElement
        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
        
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
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(slider.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
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
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        
        // Asignar referencias
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;
    }
    
    Toggle CreateToggle(string name, Transform parent, string label)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        
        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 30);
        
        LayoutElement layout = toggleObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 30;
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        
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
        toggle.isOn = false;
        
        return toggle;
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
    
    #endregion
    
    #region Métodos Públicos
    
    public void ShowPausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    public void ShowSettingsPanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    public void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }
    
    public void HideAllPanels()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    #endregion
}
