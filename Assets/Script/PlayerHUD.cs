using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD automático del jugador.
/// Crea automáticamente: Barra de vida (abajo izquierda), Crosshair (centro), Indicador de ronda.
/// Se adjunta al Player o a un objeto vacío en la escena.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Configuración")]
    public bool autoCreate = true;
    
    [Header("Crosshair")]
    public float crosshairSize = 20f;
    public float crosshairThickness = 2f;
    public float crosshairGap = 6f;
    public Color crosshairColor = Color.white;
    public bool showDot = true;
    
    [Header("Barra de Vida")]
    public Color healthBarColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color healthBarLowColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    public Color healthBarBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
    
    // Referencias internas
    private Canvas hudCanvas;
    private Image healthBarFill;
    private Image healthBarBackground;
    private Image healthBarBorder;
    private Text healthText;
    private Text waveIndicatorText;
    private Text zombieCountText;
    private Image damageOverlay;
    
    private PlayerHealth playerHealth;
    private GameManager gameManager;
    
    private bool hudCreated = false;
    private float lastHealth;
    private float damageFlashTimer = 0f;
    
    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        gameManager = GameManager.Instance;
        
        if (autoCreate && !hudCreated)
        {
            CreateHUD();
        }
    }
    
    void Update()
    {
        if (!hudCreated) return;
        
        UpdateHealthBar();
        UpdateWaveInfo();
        UpdateDamageOverlay();
    }
    
    void CreateHUD()
    {
        // Crear Canvas
        GameObject canvasObj = new GameObject("PlayerHUD_Canvas");
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // =========== CROSSHAIR ===========
        CreateCrosshair(canvasObj.transform);
        
        // =========== BARRA DE VIDA (abajo izquierda) ===========
        CreateHealthBar(canvasObj.transform);
        
        // =========== INDICADOR DE RONDA (arriba centro) ===========
        CreateWaveIndicator(canvasObj.transform);
        
        // =========== CONTADOR DE ZOMBIES (arriba derecha) ===========
        CreateZombieCounter(canvasObj.transform);
        
        // =========== DAMAGE OVERLAY (pantalla completa) ===========
        CreateDamageOverlay(canvasObj.transform);
        
        hudCreated = true;
        
        if (playerHealth != null)
            lastHealth = playerHealth.currentHealth;
    }
    
    // ==================== CROSSHAIR ====================
    void CreateCrosshair(Transform parent)
    {
        GameObject crosshairGroup = new GameObject("Crosshair");
        crosshairGroup.transform.SetParent(parent, false);
        RectTransform groupRect = crosshairGroup.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0.5f);
        groupRect.anchorMax = new Vector2(0.5f, 0.5f);
        groupRect.sizeDelta = new Vector2(100, 100);
        
        // Línea izquierda
        CreateCrosshairLine(crosshairGroup.transform, "Left", 
            new Vector2(-crosshairGap - crosshairSize/2, 0), 
            new Vector2(crosshairSize, crosshairThickness));
        
        // Línea derecha
        CreateCrosshairLine(crosshairGroup.transform, "Right", 
            new Vector2(crosshairGap + crosshairSize/2, 0), 
            new Vector2(crosshairSize, crosshairThickness));
        
        // Línea arriba
        CreateCrosshairLine(crosshairGroup.transform, "Top", 
            new Vector2(0, crosshairGap + crosshairSize/2), 
            new Vector2(crosshairThickness, crosshairSize));
        
        // Línea abajo
        CreateCrosshairLine(crosshairGroup.transform, "Bottom", 
            new Vector2(0, -crosshairGap - crosshairSize/2), 
            new Vector2(crosshairThickness, crosshairSize));
        
        // Punto central
        if (showDot)
        {
            CreateCrosshairLine(crosshairGroup.transform, "Dot", 
                Vector2.zero, 
                new Vector2(3f, 3f));
        }
    }
    
    void CreateCrosshairLine(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject("Crosshair_" + name);
        line.transform.SetParent(parent, false);
        
        RectTransform rect = line.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image img = line.AddComponent<Image>();
        img.color = crosshairColor;
        img.raycastTarget = false;
        
        // Sombra sutil
        Shadow shadow = line.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(1, -1);
    }
    
    // ==================== BARRA DE VIDA ====================
    void CreateHealthBar(Transform parent)
    {
        // Contenedor principal — abajo izquierda
        GameObject container = new GameObject("HealthBar_Container");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.pivot = new Vector2(0, 0);
        containerRect.anchoredPosition = new Vector2(30, 30);
        containerRect.sizeDelta = new Vector2(280, 45);
        
        // Icono de vida (cruz)
        GameObject icon = new GameObject("HealthIcon");
        icon.transform.SetParent(container.transform, false);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 0);
        iconRect.sizeDelta = new Vector2(30, 30);
        Text iconText = icon.AddComponent<Text>();
        iconText.text = "♥";
        iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        iconText.fontSize = 26;
        iconText.color = new Color(1f, 0.3f, 0.3f, 1f);
        iconText.alignment = TextAnchor.MiddleCenter;
        iconText.raycastTarget = false;
        
        // Background de la barra
        GameObject bgObj = new GameObject("HealthBar_BG");
        bgObj.transform.SetParent(container.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = new Vector2(35, 5);
        bgRect.offsetMax = new Vector2(-5, -5);
        healthBarBackground = bgObj.AddComponent<Image>();
        healthBarBackground.color = healthBarBackgroundColor;
        healthBarBackground.raycastTarget = false;
        
        // Borde
        GameObject borderObj = new GameObject("HealthBar_Border");
        borderObj.transform.SetParent(container.transform, false);
        RectTransform borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 0);
        borderRect.anchorMax = new Vector2(1, 1);
        borderRect.offsetMin = new Vector2(33, 3);
        borderRect.offsetMax = new Vector2(-3, -3);
        healthBarBorder = borderObj.AddComponent<Image>();
        healthBarBorder.color = new Color(0.9f, 0.9f, 0.9f, 0.6f);
        healthBarBorder.raycastTarget = false;
        // Hacer que solo se vea el borde usando fillAmount en fill mode
        Outline outline = borderObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.9f, 0.9f, 0.4f);
        outline.effectDistance = new Vector2(1.5f, 1.5f);
        healthBarBorder.color = Color.clear; // Solo el outline se ve
        
        // Fill de la barra
        GameObject fillObj = new GameObject("HealthBar_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = healthBarColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillAmount = 1f;
        healthBarFill.raycastTarget = false;
        
        // Texto de salud
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(container.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(35, 0);
        textRect.offsetMax = new Vector2(-5, 0);
        healthText = textObj.AddComponent<Text>();
        healthText.text = "100/100";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 16;
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.fontStyle = FontStyle.Bold;
        healthText.raycastTarget = false;
        
        // Sombra del texto
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0, 0, 0, 0.8f);
        textShadow.effectDistance = new Vector2(1, -1);
    }
    
    // ==================== INDICADOR DE RONDA ====================
    void CreateWaveIndicator(Transform parent)
    {
        GameObject waveObj = new GameObject("WaveIndicator");
        waveObj.transform.SetParent(parent, false);
        RectTransform waveRect = waveObj.AddComponent<RectTransform>();
        waveRect.anchorMin = new Vector2(0.5f, 1);
        waveRect.anchorMax = new Vector2(0.5f, 1);
        waveRect.pivot = new Vector2(0.5f, 1);
        waveRect.anchoredPosition = new Vector2(0, -15);
        waveRect.sizeDelta = new Vector2(300, 40);
        
        // Background semitransparente
        Image waveBg = waveObj.AddComponent<Image>();
        waveBg.color = new Color(0, 0, 0, 0.4f);
        waveBg.raycastTarget = false;
        
        waveIndicatorText = waveObj.AddComponent<Text>();
        // Sobreescribir — el text se pone encima
        Destroy(waveIndicatorText);
        
        // Texto
        GameObject waveTextObj = new GameObject("WaveText");
        waveTextObj.transform.SetParent(waveObj.transform, false);
        RectTransform wtRect = waveTextObj.AddComponent<RectTransform>();
        wtRect.anchorMin = Vector2.zero;
        wtRect.anchorMax = Vector2.one;
        wtRect.offsetMin = Vector2.zero;
        wtRect.offsetMax = Vector2.zero;
        
        waveIndicatorText = waveTextObj.AddComponent<Text>();
        waveIndicatorText.text = "RONDA 1";
        waveIndicatorText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        waveIndicatorText.fontSize = 22;
        waveIndicatorText.color = new Color(1f, 0.85f, 0.2f, 1f);
        waveIndicatorText.alignment = TextAnchor.MiddleCenter;
        waveIndicatorText.fontStyle = FontStyle.Bold;
        waveIndicatorText.raycastTarget = false;
        
        Shadow waveShadow = waveTextObj.AddComponent<Shadow>();
        waveShadow.effectColor = new Color(0, 0, 0, 0.9f);
        waveShadow.effectDistance = new Vector2(1.5f, -1.5f);
    }
    
    // ==================== CONTADOR DE ZOMBIES ====================
    void CreateZombieCounter(Transform parent)
    {
        GameObject counterObj = new GameObject("ZombieCounter");
        counterObj.transform.SetParent(parent, false);
        RectTransform counterRect = counterObj.AddComponent<RectTransform>();
        counterRect.anchorMin = new Vector2(1, 1);
        counterRect.anchorMax = new Vector2(1, 1);
        counterRect.pivot = new Vector2(1, 1);
        counterRect.anchoredPosition = new Vector2(-20, -15);
        counterRect.sizeDelta = new Vector2(200, 35);
        
        Image counterBg = counterObj.AddComponent<Image>();
        counterBg.color = new Color(0, 0, 0, 0.4f);
        counterBg.raycastTarget = false;
        
        GameObject counterTextObj = new GameObject("CounterText");
        counterTextObj.transform.SetParent(counterObj.transform, false);
        RectTransform ctRect = counterTextObj.AddComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.offsetMin = Vector2.zero;
        ctRect.offsetMax = Vector2.zero;
        
        zombieCountText = counterTextObj.AddComponent<Text>();
        zombieCountText.text = "☠ 0 Eliminados";
        zombieCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        zombieCountText.fontSize = 18;
        zombieCountText.color = new Color(0.9f, 0.3f, 0.3f, 1f);
        zombieCountText.alignment = TextAnchor.MiddleCenter;
        zombieCountText.fontStyle = FontStyle.Bold;
        zombieCountText.raycastTarget = false;
        
        Shadow counterShadow = counterTextObj.AddComponent<Shadow>();
        counterShadow.effectColor = new Color(0, 0, 0, 0.9f);
        counterShadow.effectDistance = new Vector2(1, -1);
    }
    
    // ==================== DAMAGE OVERLAY ====================
    void CreateDamageOverlay(Transform parent)
    {
        GameObject overlayObj = new GameObject("DamageOverlay");
        overlayObj.transform.SetParent(parent, false);
        RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        damageOverlay = overlayObj.AddComponent<Image>();
        damageOverlay.color = Color.clear;
        damageOverlay.raycastTarget = false;
    }
    
    // ==================== ACTUALIZACIONES ====================
    void UpdateHealthBar()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null) return;
        }
        
        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);
        
        // Animar la barra suavemente
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, healthPercent, Time.deltaTime * 5f);
            
            // Cambiar color según la vida
            healthBarFill.color = Color.Lerp(healthBarLowColor, healthBarColor, healthPercent);
        }
        
        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(playerHealth.currentHealth) + "/" + Mathf.RoundToInt(playerHealth.maxHealth);
        }
        
        // Detectar daño recibido para el flash
        if (playerHealth.currentHealth < lastHealth)
        {
            damageFlashTimer = 0.3f;
        }
        lastHealth = playerHealth.currentHealth;
    }
    
    void UpdateWaveInfo()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            if (gameManager == null) return;
        }
        
        if (waveIndicatorText != null)
        {
            waveIndicatorText.text = "⚔ RONDA " + gameManager.currentWave;
        }
        
        if (zombieCountText != null)
        {
            zombieCountText.text = "☠ " + gameManager.zombiesKilled + " Eliminados";
        }
    }
    
    void UpdateDamageOverlay()
    {
        if (damageOverlay == null) return;
        
        if (damageFlashTimer > 0)
        {
            damageFlashTimer -= Time.deltaTime;
            damageOverlay.color = new Color(0.8f, 0f, 0f, damageFlashTimer * 0.7f);
        }
        else
        {
            damageOverlay.color = Color.Lerp(damageOverlay.color, Color.clear, Time.deltaTime * 3f);
        }
    }
}
