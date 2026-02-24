using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// ═══════════════════════════════════════════════════════════
///  MENÚ DE PAUSA AUTO-GENERADO — TOOL PROFESIONAL
/// ═══════════════════════════════════════════════════════════
/// 
/// INSTRUCCIONES:
///   1. Crea un GameObject vacío en tu escena de juego
///   2. Añade SOLO este script al GameObject
///   3. ¡Listo! Pulsa ESC para pausar
/// 
/// IMPORTANTE: Este script maneja ESC internamente.
/// Si otros scripts también usan ESC para pausar,
/// desactiva su lógica de pausa para evitar conflictos.
/// 
/// Funcionalidades:
///   - Reanudar partida
///   - Control de Volumen (slider)
///   - Control de Brillo (slider)
///   - Reiniciar nivel
///   - Salir al Menú Principal
///   - Animaciones suaves de entrada/salida
///   - Diseño oscuro profesional con acentos rojos
/// 
/// Métodos Públicos:
///   - PauseGame()   → pausar desde otros scripts
///   - ResumeGame()  → reanudar desde otros scripts
///   - IsPaused()    → consultar estado de pausa
/// ═══════════════════════════════════════════════════════════
/// </summary>
public class AutoPauseMenuTool : MonoBehaviour
{
    // ════════════════════════════════════════
    //  CONFIGURACIÓN (Inspector)
    // ════════════════════════════════════════

    [Header("Escena del Menú Principal")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "Inicio";

    [Header("Colores del Menú")]
    public Color fondoColor = new Color(0.02f, 0.02f, 0.05f, 0.92f);
    public Color panelColor = new Color(0.08f, 0.08f, 0.10f, 0.97f);
    public Color botonColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);
    public Color botonHoverColor = new Color(0.85f, 0.25f, 0.25f, 1f);
    public Color textoColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    public Color accentColor = new Color(0.85f, 0.25f, 0.25f, 1f);

    // ════════════════════════════════════════
    //  ESTADO INTERNO
    // ════════════════════════════════════════

    private bool isPaused = false;
    private bool menuCreated = false;

    // Referencias UI
    private Canvas pauseCanvas;
    private GameObject panelFondo;
    private GameObject panelMenu;
    private GameObject panelOpciones;
    private Slider sliderVolumen;
    private Slider sliderBrillo;
    private Text textoVolumenValor;
    private Text textoBrilloValor;

    // Fuente
    private Font fuente;

    // ════════════════════════════════════════
    //  INICIALIZACIÓN
    // ════════════════════════════════════════

    void Start()
    {
        // Intentar cargar fuente (LegacyRuntime primero, luego Arial)
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null)
            fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    // ════════════════════════════════════════
    //  UPDATE — Detectar ESC
    // ════════════════════════════════════════

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Crear el menú la primera vez que se pulsa ESC
            if (!menuCreated)
            {
                CrearMenuCompleto();
                menuCreated = true;
            }

            TogglePause();
        }
    }

    // ════════════════════════════════════════
    //  CONTROL DE PAUSA
    // ════════════════════════════════════════

    void TogglePause()
    {
        if (isPaused)
            Reanudar();
        else
            Pausar();
    }

    void Pausar()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Mostrar menú principal, ocultar opciones
        if (panelFondo != null) panelFondo.SetActive(true);
        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        // Animación de entrada
        StartCoroutine(AnimarEntrada());
    }

    void Reanudar()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Ocultar cursor y bloquear
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ocultar todo
        if (panelFondo != null) panelFondo.SetActive(false);
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    // ════════════════════════════════════════
    //  MÉTODOS PÚBLICOS (para otros scripts)
    // ════════════════════════════════════════

    /// <summary>
    /// Consulta si el juego está pausado.
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }

    /// <summary>
    /// Pausa el juego desde otro script (GameController, etc.)
    /// </summary>
    public void PauseGame()
    {
        if (!menuCreated)
        {
            CrearMenuCompleto();
            menuCreated = true;
        }
        if (!isPaused)
        {
            Pausar();
        }
    }

    /// <summary>
    /// Reanuda el juego desde otro script (GameController, etc.)
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused)
        {
            Reanudar();
        }
    }

    // ════════════════════════════════════════
    //  ANIMACIONES
    // ════════════════════════════════════════

    IEnumerator AnimarEntrada()
    {
        if (panelMenu == null) yield break;

        CanvasGroup cg = panelMenu.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelMenu.AddComponent<CanvasGroup>();

        RectTransform rt = panelMenu.GetComponent<RectTransform>();

        cg.alpha = 0f;
        rt.localScale = Vector3.one * 0.85f;

        float duracion = 0.2f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float ease = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic

            cg.alpha = Mathf.Lerp(0f, 1f, ease);
            rt.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, ease);

            yield return null;
        }

        cg.alpha = 1f;
        rt.localScale = Vector3.one;
    }

    IEnumerator AnimarEntradaPanel(GameObject panel)
    {
        if (panel == null) yield break;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        RectTransform rt = panel.GetComponent<RectTransform>();
        cg.alpha = 0f;
        rt.localScale = Vector3.one * 0.9f;

        float duracion = 0.15f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            cg.alpha = ease;
            rt.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, ease);
            yield return null;
        }

        cg.alpha = 1f;
        rt.localScale = Vector3.one;
    }

    // ════════════════════════════════════════
    //  CREACIÓN DEL MENÚ COMPLETO
    // ════════════════════════════════════════

    void CrearMenuCompleto()
    {
        // Asegurar que existe un EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem_Pausa");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // ── CANVAS ──
        GameObject canvasObj = new GameObject("PauseMenuCanvas_Auto");
        pauseCanvas = canvasObj.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 900;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ── FONDO OSCURO (pantalla completa) ──
        panelFondo = CrearPanelFullscreen("Fondo", pauseCanvas.transform, fondoColor);

        // ── PANEL MENÚ PRINCIPAL ──
        panelMenu = CrearPanelCentral("MenuPrincipal", pauseCanvas.transform, 420, 530);
        CrearContenidoMenuPrincipal();

        // ── PANEL OPCIONES ──
        panelOpciones = CrearPanelCentral("PanelOpciones", pauseCanvas.transform, 460, 420);
        CrearContenidoOpciones();

        // Ocultar todo inicialmente
        panelFondo.SetActive(false);
        panelMenu.SetActive(false);
        panelOpciones.SetActive(false);

        Debug.Log("[AutoPauseMenuTool] Menú de pausa creado correctamente.");
    }

    // ════════════════════════════════════════
    //  CONTENIDO: MENÚ PRINCIPAL
    // ════════════════════════════════════════

    void CrearContenidoMenuPrincipal()
    {
        // Layout vertical
        VerticalLayoutGroup vlg = panelMenu.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(40, 40, 35, 35);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ── TÍTULO ──
        CrearTexto(panelMenu.transform, "PAUSA", 42, FontStyle.Bold, accentColor, 55);

        // Línea decorativa
        CrearLinea(panelMenu.transform, accentColor, 2f);
        CrearEspaciador(panelMenu.transform, 10);

        // ── BOTÓN: REANUDAR ──
        CrearBoton(panelMenu.transform, "▶  REANUDAR", 58, () =>
        {
            Reanudar();
        });

        CrearEspaciador(panelMenu.transform, 4);

        // ── BOTÓN: OPCIONES ──
        CrearBoton(panelMenu.transform, "⚙  OPCIONES", 55, () =>
        {
            panelMenu.SetActive(false);
            panelOpciones.SetActive(true);
            StartCoroutine(AnimarEntradaPanel(panelOpciones));
        });

        CrearEspaciador(panelMenu.transform, 4);

        // ── BOTÓN: REINICIAR ──
        CrearBoton(panelMenu.transform, "↻  REINICIAR", 52, () =>
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });

        CrearEspaciador(panelMenu.transform, 4);

        // ── BOTÓN: SALIR AL MENÚ ──
        CrearBoton(panelMenu.transform, "✕  SALIR AL MENÚ", 52, () =>
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneManager.LoadScene(mainMenuSceneName);
        });

        CrearEspaciador(panelMenu.transform, 15);

        // Línea decorativa inferior
        CrearLinea(panelMenu.transform, new Color(1f, 1f, 1f, 0.15f), 1f);

        // Texto inferior
        CrearTexto(panelMenu.transform, "Pulsa ESC para reanudar", 15, FontStyle.Italic,
            new Color(0.6f, 0.6f, 0.6f, 0.8f), 22);
    }

    // ════════════════════════════════════════
    //  CONTENIDO: PANEL OPCIONES
    // ════════════════════════════════════════

    void CrearContenidoOpciones()
    {
        VerticalLayoutGroup vlg = panelOpciones.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(40, 40, 30, 30);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ── TÍTULO ──
        CrearTexto(panelOpciones.transform, "OPCIONES", 36, FontStyle.Bold, accentColor, 48);
        CrearLinea(panelOpciones.transform, accentColor, 2f);
        CrearEspaciador(panelOpciones.transform, 12);

        // ═══ VOLUMEN ═══
        CrearTexto(panelOpciones.transform, "♫  VOLUMEN", 20, FontStyle.Bold, textoColor, 28);

        GameObject volContainer = CrearContainerHorizontal("VolumenContainer", panelOpciones.transform, 35);
        float volActual = PlayerPrefs.GetFloat("Volume", 1f);
        sliderVolumen = CrearSliderEnContainer(volContainer.transform, 0f, 1f, volActual);
        textoVolumenValor = CrearTextoEnContainer(volContainer.transform,
            Mathf.RoundToInt(volActual * 100) + "%", 50);

        sliderVolumen.onValueChanged.AddListener((valor) =>
        {
            AudioListener.volume = valor;
            PlayerPrefs.SetFloat("Volume", valor);
            PlayerPrefs.Save();
            if (textoVolumenValor != null)
                textoVolumenValor.text = Mathf.RoundToInt(valor * 100) + "%";
        });

        CrearEspaciador(panelOpciones.transform, 15);

        // ═══ BRILLO ═══
        CrearTexto(panelOpciones.transform, "☀  BRILLO", 20, FontStyle.Bold, textoColor, 28);

        GameObject briContainer = CrearContainerHorizontal("BrilloContainer", panelOpciones.transform, 35);
        float briActual = PlayerPrefs.GetFloat("Brightness", 1f);
        sliderBrillo = CrearSliderEnContainer(briContainer.transform, 0.1f, 1f, briActual);
        textoBrilloValor = CrearTextoEnContainer(briContainer.transform,
            Mathf.RoundToInt(briActual * 100) + "%", 50);

        sliderBrillo.onValueChanged.AddListener((valor) =>
        {
            // Aplicar brillo si BrightnessController existe
            if (BrightnessController.Instance != null)
                BrightnessController.Instance.SetBrightness(valor);

            PlayerPrefs.SetFloat("Brightness", valor);
            PlayerPrefs.Save();
            if (textoBrilloValor != null)
                textoBrilloValor.text = Mathf.RoundToInt(valor * 100) + "%";
        });

        CrearEspaciador(panelOpciones.transform, 20);

        // ── BOTÓN: VOLVER ──
        CrearBoton(panelOpciones.transform, "←  VOLVER", 52, () =>
        {
            panelOpciones.SetActive(false);
            panelMenu.SetActive(true);
            StartCoroutine(AnimarEntradaPanel(panelMenu));
        });
    }

    // ════════════════════════════════════════
    //  CONSTRUCTORES DE UI
    // ════════════════════════════════════════

    GameObject CrearPanelFullscreen(string nombre, Transform padre, Color color)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(padre, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        return panel;
    }

    GameObject CrearPanelCentral(string nombre, Transform padre, float ancho, float alto)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(padre, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(ancho, alto);

        Image img = panel.AddComponent<Image>();
        img.color = panelColor;

        // Borde sutil con Outline
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
        outline.effectDistance = new Vector2(2, -2);

        return panel;
    }

    Text CrearTexto(Transform padre, string contenido, int tamano, FontStyle estilo,
        Color color, float altura)
    {
        GameObject obj = new GameObject("Txt_" + contenido);
        obj.transform.SetParent(padre, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);

        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = altura;

        Text txt = obj.AddComponent<Text>();
        txt.text = contenido;
        txt.font = fuente;
        txt.fontSize = tamano;
        txt.fontStyle = estilo;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;

        return txt;
    }

    Button CrearBoton(Transform padre, string texto, float altura, UnityEngine.Events.UnityAction accion)
    {
        GameObject btnObj = new GameObject("Btn_" + texto);
        btnObj.transform.SetParent(padre, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = altura;

        Image img = btnObj.AddComponent<Image>();
        img.color = botonColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        // Configurar colores del botón
        ColorBlock colores = btn.colors;
        colores.normalColor = botonColor;
        colores.highlightedColor = botonHoverColor;
        colores.pressedColor = new Color(
            botonHoverColor.r * 0.7f,
            botonHoverColor.g * 0.7f,
            botonHoverColor.b * 0.7f, 1f);
        colores.selectedColor = botonColor;
        colores.fadeDuration = 0.1f;
        btn.colors = colores;

        btn.onClick.AddListener(accion);

        // Texto del botón
        GameObject txtObj = new GameObject("Texto");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRT = txtObj.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(20, 5);
        txtRT.offsetMax = new Vector2(-20, -5);

        Text txt = txtObj.AddComponent<Text>();
        txt.text = texto;
        txt.font = fuente;
        txt.fontSize = (int)(altura * 0.38f);
        txt.fontStyle = FontStyle.Bold;
        txt.color = textoColor;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;

        // Sombra del texto
        Shadow shadow = txtObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(1, -1);

        return btn;
    }

    void CrearLinea(Transform padre, Color color, float grosor)
    {
        GameObject lineObj = new GameObject("Linea");
        lineObj.transform.SetParent(padre, false);

        RectTransform rt = lineObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, grosor);

        LayoutElement le = lineObj.AddComponent<LayoutElement>();
        le.preferredHeight = grosor;

        Image img = lineObj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    void CrearEspaciador(Transform padre, float altura)
    {
        GameObject espObj = new GameObject("Espaciador");
        espObj.transform.SetParent(padre, false);

        RectTransform rt = espObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);

        LayoutElement le = espObj.AddComponent<LayoutElement>();
        le.preferredHeight = altura;
    }

    GameObject CrearContainerHorizontal(string nombre, Transform padre, float altura)
    {
        GameObject container = new GameObject(nombre);
        container.transform.SetParent(padre, false);

        RectTransform rt = container.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);

        LayoutElement le = container.AddComponent<LayoutElement>();
        le.preferredHeight = altura;

        HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        return container;
    }

    Slider CrearSliderEnContainer(Transform padre, float min, float max, float valorInicial)
    {
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(padre, false);

        RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.sizeDelta = new Vector2(0, 28);

        LayoutElement sliderLE = sliderObj.AddComponent<LayoutElement>();
        sliderLE.flexibleWidth = 1f;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;

        // Background del slider
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRT = bgObj.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.35f);
        bgRT.anchorMax = new Vector2(1, 0.65f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.35f);
        fillAreaRT.anchorMax = new Vector2(1, 0.65f);
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // Fill (la barra de color)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = accentColor;

        // Handle Slide Area
        GameObject handleArea = new GameObject("HandleSlideArea");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(10, 0);
        handleAreaRT.offsetMax = new Vector2(-10, 0);

        // Handle (el botón deslizable)
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(22, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        // Asignar referencias al Slider
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;

        // Valor inicial
        slider.value = valorInicial;

        return slider;
    }

    Text CrearTextoEnContainer(Transform padre, string contenido, float anchoFijo)
    {
        GameObject txtObj = new GameObject("ValorTexto");
        txtObj.transform.SetParent(padre, false);

        RectTransform rt = txtObj.AddComponent<RectTransform>();

        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.preferredWidth = anchoFijo;
        le.flexibleWidth = 0;

        Text txt = txtObj.AddComponent<Text>();
        txt.text = contenido;
        txt.font = fuente;
        txt.fontSize = 18;
        txt.fontStyle = FontStyle.Bold;
        txt.color = textoColor;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;

        return txt;
    }

    // ════════════════════════════════════════
    //  LIMPIEZA
    // ════════════════════════════════════════

    void OnDestroy()
    {
        // Restaurar tiempo por si se destruye mientras está en pausa
        Time.timeScale = 1f;
    }
}
