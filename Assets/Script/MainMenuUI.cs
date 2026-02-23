using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

/// <summary>
/// Script que genera toda la UI del menú principal programáticamente.
/// Instrucciones: Crear un GameObject vacío en la escena "Inicio" y añadir este script.
/// Se encarga de crear el Canvas, los botones de inicio y el panel de ajustes (brillo/sonido).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Colores")]
    public Color botonColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color botonHoverColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color textoColor = Color.white;
    public Color panelColor = new Color(0f, 0f, 0f, 0.7f);
    public Color sliderFillColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    [Header("Fondo")]
    public Sprite imagenFondo;

    [Header("Audio")]
    public AudioClip musicaFondo;
    public AudioClip sonidoClick;

    // Referencias internas
    private Canvas canvas;
    private GameObject panelPrincipal;
    private GameObject panelAjustesRapidos;
    private GameObject panelOpciones;
    private AudioSource audioSource;
    private Slider sliderBrillo;
    private Slider sliderVolumen;

    // Fuente por defecto
    private Font fuenteDefault;
    
    // Constantes para configuración
    private const string NOMBRE_ESCENA_JUEGO = "SampleScene";
    private const float BRILLO_MINIMO = 0f;
    private const float BRILLO_MAXIMO = 1f;
    private const float VOLUMEN_MINIMO = 0f;
    private const float VOLUMEN_MAXIMO = 1f;
    private const float SENSIBILIDAD_MINIMA = 0.5f;
    private const float SENSIBILIDAD_MAXIMA = 10f;

    void Start()
    {
        CargarFuentePorDefecto();

        // Configurar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        if (musicaFondo != null)
        {
            audioSource.clip = musicaFondo;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Inicializar BrightnessController global
        if (BrightnessController.Instance == null)
        {
            GameObject bcObj = new GameObject("BrightnessController");
            bcObj.AddComponent<BrightnessController>();
        }

        // Crear EventSystem (NECESARIO para que funcionen los clics)
        CrearEventSystem();

        // Crear Canvas
        CrearCanvas();

        // Imagen de fondo
        CrearFondo();

        // Crear paneles
        CrearPanelPrincipal();
        CrearPanelAjustesRapidos();

        // Cargar ajustes guardados
        CargarAjustes();
    }
    
    void CargarFuentePorDefecto()
    {
        fuenteDefault = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuenteDefault == null)
            fuenteDefault = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
    
    void OnDestroy()
    {
        // Limpiar recursos para evitar fugas de memoria
        if (fuenteDefault != null)
        {
            fuenteDefault = null;
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    // ========== EVENT SYSTEM (sin esto no funciona NADA) ==========
    void CrearEventSystem()
    {
        // Si ya existe uno en la escena, no crear otro
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
    }

    // ========== CANVAS ==========
    void CrearCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas_MenuPrincipal");
        canvasObj.transform.SetParent(transform);
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    // ========== IMAGEN DE FONDO ==========
    void CrearFondo()
    {
        GameObject fondoObj = new GameObject("ImagenFondo");
        fondoObj.transform.SetParent(canvas.transform, false);
        fondoObj.transform.SetAsFirstSibling(); // Detrás de todo

        RectTransform rt = fondoObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = fondoObj.AddComponent<Image>();
        img.raycastTarget = false;

        if (imagenFondo != null)
        {
            img.sprite = imagenFondo;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
        }
        else
        {
            // Si no hay sprite asignado, usar un degradado oscuro
            img.color = new Color(0.05f, 0.05f, 0.1f, 1f);
        }
    }

    // ========== PANEL PRINCIPAL (Botones de Inicio) ==========
    void CrearPanelPrincipal()
    {
        panelPrincipal = CrearPanel("PanelPrincipal", canvas.transform);
        RectTransform rt = panelPrincipal.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Fondo semitransparente - NO bloquea raycasts para que los botones funcionen
        Image bg = panelPrincipal.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.3f);
        bg.raycastTarget = false;

        // Contenedor central para los botones
        GameObject contenedor = new GameObject("ContenedorBotones");
        contenedor.transform.SetParent(panelPrincipal.transform, false);
        RectTransform contRT = contenedor.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.5f, 0.5f);
        contRT.anchorMax = new Vector2(0.5f, 0.5f);
        contRT.pivot = new Vector2(0.5f, 0.5f);
        contRT.sizeDelta = new Vector2(400, 500);

        VerticalLayoutGroup vlg = contenedor.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Título
        CrearTexto(contenedor.transform, "CALL OF DUTI", 52, FontStyle.Bold, textoColor, 80);

        // Subtítulo
        CrearTexto(contenedor.transform, "ZOMBIES", 28, FontStyle.Normal, sliderFillColor, 40);

        // Espacio
        CrearEspaciador(contenedor.transform, 30);

        // Botón JUGAR
        CrearBoton(contenedor.transform, "JUGAR", 60, () => {
            ReproducirClick();
            SceneManager.LoadScene(NOMBRE_ESCENA_JUEGO);
        });

        // Botón OPCIONES
        CrearBoton(contenedor.transform, "OPCIONES", 55, () => {
            ReproducirClick();
            panelPrincipal.SetActive(false);
            if (panelOpciones == null) CrearPanelOpciones();
            panelOpciones.SetActive(true);
        });

        // Botón SALIR
        CrearBoton(contenedor.transform, "SALIR", 55, () => {
            ReproducirClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    // ========== PANEL AJUSTES RÁPIDOS (Arriba derecha con botón rueda) ==========
    void CrearPanelAjustesRapidos()
    {
        // Contenedor padre (arriba derecha)
        GameObject contenedorAjustes = new GameObject("ContenedorAjustes");
        contenedorAjustes.transform.SetParent(canvas.transform, false);
        RectTransform contRT = contenedorAjustes.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(1, 1);
        contRT.anchorMax = new Vector2(1, 1);
        contRT.pivot = new Vector2(1, 1);
        contRT.anchoredPosition = new Vector2(-15, -15);
        contRT.sizeDelta = new Vector2(310, 250);

        // --- BOTÓN RUEDA/ENGRANAJE ---
        GameObject botonRueda = new GameObject("BotonRueda");
        botonRueda.transform.SetParent(contenedorAjustes.transform, false);
        RectTransform btnRT = botonRueda.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 1);
        btnRT.anchorMax = new Vector2(1, 1);
        btnRT.pivot = new Vector2(1, 1);
        btnRT.anchoredPosition = Vector2.zero;
        btnRT.sizeDelta = new Vector2(50, 50);

        Image btnImg = botonRueda.AddComponent<Image>();
        btnImg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        Button btnComp = botonRueda.AddComponent<Button>();
        btnComp.targetGraphic = btnImg;
        ColorBlock cbtn = btnComp.colors;
        cbtn.normalColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        cbtn.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        cbtn.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        cbtn.fadeDuration = 0.1f;
        btnComp.colors = cbtn;

        // Texto del engranaje (⚙)
        GameObject txtRueda = new GameObject("TextoRueda");
        txtRueda.transform.SetParent(botonRueda.transform, false);
        RectTransform txtRuedaRT = txtRueda.AddComponent<RectTransform>();
        txtRuedaRT.anchorMin = Vector2.zero;
        txtRuedaRT.anchorMax = Vector2.one;
        txtRuedaRT.offsetMin = Vector2.zero;
        txtRuedaRT.offsetMax = Vector2.zero;
        Text txtR = txtRueda.AddComponent<Text>();
        txtR.text = "O";
        txtR.font = fuenteDefault;
        txtR.fontSize = 28;
        txtR.fontStyle = FontStyle.Bold;
        txtR.color = Color.white;
        txtR.alignment = TextAnchor.MiddleCenter;
        txtR.raycastTarget = false;

        // --- PANEL DESPLEGABLE DE AJUSTES ---
        panelAjustesRapidos = new GameObject("PanelAjustesRapidos");
        panelAjustesRapidos.transform.SetParent(contenedorAjustes.transform, false);

        RectTransform rt = panelAjustesRapidos.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(0, -55); // Debajo del botón
        rt.sizeDelta = new Vector2(280, 180);

        Image bg = panelAjustesRapidos.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        // Bordes redondeados simulados con outline
        Outline outline = panelAjustesRapidos.AddComponent<Outline>();
        outline.effectColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        outline.effectDistance = new Vector2(1, -1);

        VerticalLayoutGroup vlg = panelAjustesRapidos.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 12, 12);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Título
        CrearTexto(panelAjustesRapidos.transform, "AJUSTES", 16, FontStyle.Bold, textoColor, 22);

        // --- VOLUMEN ---
        CrearTexto(panelAjustesRapidos.transform, "\u266B Volumen", 13, FontStyle.Normal, textoColor, 18);
        sliderVolumen = CrearSlider(panelAjustesRapidos.transform, VOLUMEN_MINIMO, VOLUMEN_MAXIMO, 1f, (valor) => {
            valor = Mathf.Clamp(valor, VOLUMEN_MINIMO, VOLUMEN_MAXIMO);
            AudioListener.volume = valor;
            PlayerPrefs.SetFloat("Volume", valor);
            PlayerPrefs.Save();
        });

        // --- BRILLO ---
        CrearTexto(panelAjustesRapidos.transform, "\u2600 Brillo", 13, FontStyle.Normal, textoColor, 18);
        sliderBrillo = CrearSlider(panelAjustesRapidos.transform, BRILLO_MINIMO, BRILLO_MAXIMO, 1f, (valor) => {
            valor = Mathf.Clamp(valor, BRILLO_MINIMO, BRILLO_MAXIMO);
            if (BrightnessController.Instance != null)
                BrightnessController.Instance.SetBrightness(valor);
            PlayerPrefs.SetFloat("Brightness", valor);
            PlayerPrefs.Save();
        });

        // Oculto por defecto
        panelAjustesRapidos.SetActive(false);

        // Evento del botón: toggle mostrar/ocultar
        btnComp.onClick.AddListener(() => {
            ReproducirClick();
            panelAjustesRapidos.SetActive(!panelAjustesRapidos.activeSelf);
        });
    }

    // ========== PANEL OPCIONES COMPLETO ==========
    void CrearPanelOpciones()
    {
        panelOpciones = CrearPanel("PanelOpciones", canvas.transform);
        RectTransform rt = panelOpciones.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panelOpciones.GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);

        // Contenedor central
        GameObject contenedor = new GameObject("ContenedorOpciones");
        contenedor.transform.SetParent(panelOpciones.transform, false);
        RectTransform contRT = contenedor.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.5f, 0.5f);
        contRT.anchorMax = new Vector2(0.5f, 0.5f);
        contRT.pivot = new Vector2(0.5f, 0.5f);
        contRT.sizeDelta = new Vector2(500, 400);

        VerticalLayoutGroup vlg = contenedor.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Título
        CrearTexto(contenedor.transform, "OPCIONES", 36, FontStyle.Bold, textoColor, 50);
        CrearEspaciador(contenedor.transform, 10);

        // Volumen
        CrearTexto(contenedor.transform, "Volumen", 18, FontStyle.Normal, textoColor, 25);
        Slider volOpc = CrearSlider(contenedor.transform, VOLUMEN_MINIMO, VOLUMEN_MAXIMO, AudioListener.volume, (valor) => {
            valor = Mathf.Clamp(valor, VOLUMEN_MINIMO, VOLUMEN_MAXIMO);
            AudioListener.volume = valor;
            PlayerPrefs.SetFloat("Volume", valor);
            if (sliderVolumen != null) sliderVolumen.value = valor;
        });

        CrearEspaciador(contenedor.transform, 5);

        // Brillo
        CrearTexto(contenedor.transform, "Brillo", 18, FontStyle.Normal, textoColor, 25);
        float brilloActual = PlayerPrefs.GetFloat("Brightness", 1f);
        brilloActual = Mathf.Clamp(brilloActual, BRILLO_MINIMO, BRILLO_MAXIMO);
        Slider briOpc = CrearSlider(contenedor.transform, BRILLO_MINIMO, BRILLO_MAXIMO, brilloActual, (valor) => {
            valor = Mathf.Clamp(valor, BRILLO_MINIMO, BRILLO_MAXIMO);
            if (BrightnessController.Instance != null)
                BrightnessController.Instance.SetBrightness(valor);
            if (sliderBrillo != null) sliderBrillo.value = valor;
        });

        CrearEspaciador(contenedor.transform, 5);

        // Sensibilidad del ratón
        CrearTexto(contenedor.transform, "Sensibilidad Ratón", 18, FontStyle.Normal, textoColor, 25);
        float sensActual = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
        sensActual = Mathf.Clamp(sensActual, SENSIBILIDAD_MINIMA, SENSIBILIDAD_MAXIMA);
        CrearSlider(contenedor.transform, SENSIBILIDAD_MINIMA, SENSIBILIDAD_MAXIMA, sensActual, (valor) => {
            valor = Mathf.Clamp(valor, SENSIBILIDAD_MINIMA, SENSIBILIDAD_MAXIMA);
            PlayerPrefs.SetFloat("MouseSensitivity", valor);
        });

        CrearEspaciador(contenedor.transform, 15);

        // Botón Volver
        CrearBoton(contenedor.transform, "VOLVER", 50, () => {
            ReproducirClick();
            PlayerPrefs.Save();
            panelOpciones.SetActive(false);
            panelPrincipal.SetActive(true);
        });
    }

    // ========== MÉTODOS AUXILIARES ==========

    void CargarAjustes()
    {
        // Volumen
        float vol = PlayerPrefs.GetFloat("Volume", 1f);
        vol = Mathf.Clamp(vol, VOLUMEN_MINIMO, VOLUMEN_MAXIMO);
        AudioListener.volume = vol;
        if (sliderVolumen != null) sliderVolumen.value = vol;

        // Brillo
        float brillo = PlayerPrefs.GetFloat("Brightness", 1f);
        brillo = Mathf.Clamp(brillo, BRILLO_MINIMO, BRILLO_MAXIMO);
        if (sliderBrillo != null) sliderBrillo.value = brillo;
        if (BrightnessController.Instance != null)
            BrightnessController.Instance.SetBrightness(brillo);
    }

    void ReproducirClick()
    {
        if (sonidoClick != null && audioSource != null)
        {
            try
            {
                audioSource.PlayOneShot(sonidoClick);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error al reproducir sonido de clic: {ex.Message}");
            }
        }
    }

    // --- Crear Panel base ---
    GameObject CrearPanel(string nombre, Transform padre)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(padre, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<Image>();
        return panel;
    }

    // --- Crear Texto ---
    Text CrearTexto(Transform padre, string contenido, int tamano, FontStyle estilo, Color color, float altura)
    {
        GameObject txtObj = new GameObject("Texto_" + contenido);
        txtObj.transform.SetParent(padre, false);

        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);

        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.preferredHeight = altura;

        Text txt = txtObj.AddComponent<Text>();
        txt.text = contenido;
        txt.font = fuenteDefault;
        txt.fontSize = tamano;
        txt.fontStyle = estilo;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        // Sombra para mejor legibilidad
        Shadow shadow = txtObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        return txt;
    }

    // --- Crear Espaciador ---
    void CrearEspaciador(Transform padre, float altura)
    {
        GameObject espacio = new GameObject("Espacio");
        espacio.transform.SetParent(padre, false);
        RectTransform rt = espacio.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);
        LayoutElement le = espacio.AddComponent<LayoutElement>();
        le.preferredHeight = altura;
    }

    // --- Crear Botón ---
    Button CrearBoton(Transform padre, string texto, float altura, UnityEngine.Events.UnityAction accion)
    {
        GameObject btnObj = new GameObject("Boton_" + texto);
        btnObj.transform.SetParent(padre, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, altura);

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredHeight = altura;

        Image img = btnObj.AddComponent<Image>();
        img.color = botonColor;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        // Colores del botón
        ColorBlock colores = btn.colors;
        colores.normalColor = botonColor;
        colores.highlightedColor = botonHoverColor;
        colores.pressedColor = new Color(botonHoverColor.r * 0.8f, botonHoverColor.g * 0.8f, botonHoverColor.b * 0.8f, 1f);
        colores.selectedColor = botonHoverColor;
        colores.fadeDuration = 0.1f;
        btn.colors = colores;

        btn.onClick.AddListener(accion);

        // Texto del botón
        GameObject txtObj = new GameObject("Texto");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRT = txtObj.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(10, 5);
        txtRT.offsetMax = new Vector2(-10, -5);

        Text txt = txtObj.AddComponent<Text>();
        txt.text = texto;
        txt.font = fuenteDefault;
        txt.fontSize = (int)(altura * 0.45f);
        txt.fontStyle = FontStyle.Bold;
        txt.color = textoColor;
        txt.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    // --- Crear Slider ---
    Slider CrearSlider(Transform padre, float min, float max, float valorInicial, UnityEngine.Events.UnityAction<float> onChange)
    {
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(padre, false);

        RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.sizeDelta = new Vector2(0, 25);

        LayoutElement le = sliderObj.AddComponent<LayoutElement>();
        le.preferredHeight = 25;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRT = bgObj.AddComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.25f);
        bgRT.anchorMax = new Vector2(1, 0.75f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = sliderFillColor;

        // Handle Slide Area
        GameObject handleArea = new GameObject("HandleSlideArea");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(10, 0);
        handleAreaRT.offsetMax = new Vector2(-10, 0);

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        // Asignar referencias del slider
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;

        // Valor inicial
        slider.value = valorInicial;

        // Evento
        slider.onValueChanged.AddListener(onChange);

        return slider;
    }
}
