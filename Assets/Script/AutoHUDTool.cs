using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// ═══════════════════════════════════════════════════════════
///  HUD AUTO-GENERADO — TOOL PROFESIONAL
/// ═══════════════════════════════════════════════════════════
/// Genera automáticamente la interfaz del jugador con un
/// diseño moderno y animado al estilo del AutoPauseMenuTool.
/// Muestra Vida, Rondas y Eliminaciones de zombies.
/// </summary>
public class AutoHUDTool : MonoBehaviour
{
    [Header("Colores del HUD")]
    public Color colorBarraFondo = new Color(0.1f, 0.1f, 0.12f, 0.85f);
    public Color colorVidaNormal = new Color(0.2f, 0.8f, 0.3f, 1f);
    public Color colorVidaBaja = new Color(0.9f, 0.2f, 0.2f, 1f);
    public Color colorTexto = new Color(0.95f, 0.95f, 0.95f, 1f);
    public Color colorRonda = new Color(0.9f, 0.7f, 0.1f, 1f); // Naranja/Dorado

    // Elementos de UI
    private Canvas hudCanvas;
    private Image fillVida;
    private Text txtVida;
    private Text txtRonda;
    private Text txtZombies;
    private Image damageFlasher;

    // Referencias
    private PlayerHealth playerHealth;
    private float smoothHealth;
    private int ultimaRonda = -1;
    private Font fuente;

    void Start()
    {
        // Limpiar cualquier HUD anterior automático para no duplicar
        GameObject oldHUD = GameObject.Find("AutoHUD_Canvas");
        if (oldHUD != null) Destroy(oldHUD);

        // Desactivar temporalmente si existe PlayerHUD clásico para no tener dos HUDs
        PlayerHUD ph = FindObjectOfType<PlayerHUD>();
        if (ph != null) ph.gameObject.SetActive(false);

        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GenerarUI();

        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            smoothHealth = playerHealth.currentHealth;
        }
    }

    void Update()
    {
        ActualizarHUD();
    }

    void GenerarUI()
    {
        // ── CANVAS PRINCIPAL ──
        GameObject canvasObj = new GameObject("AutoHUD_Canvas");
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // ── PANTALLA ROJA (DAÑO) ──
        GameObject damageObj = new GameObject("DamageFlash");
        damageObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rtDam = damageObj.AddComponent<RectTransform>();
        rtDam.anchorMin = Vector2.zero;
        rtDam.anchorMax = Vector2.one;
        rtDam.offsetMin = Vector2.zero;
        rtDam.offsetMax = Vector2.zero;
        damageFlasher = damageObj.AddComponent<Image>();
        damageFlasher.color = new Color(1f, 0f, 0f, 0f);
        damageFlasher.raycastTarget = false;

        // ── BARRA DE VIDA (Abajo Izquierda) ──
        GameObject panelVida = CrearPanelBase("PanelVida", canvasObj.transform, new Vector2(0f, 0f), new Vector2(40f, 40f), new Vector2(350f, 60f));
        
        // Icono de Corazon
        Text iconoCorazon = CrearTexto(panelVida.transform, "♥", 40, FontStyle.Normal, colorVidaBaja, new Vector2(0f, 0.5f), new Vector2(30, 0), new Vector2(50, 50));
        
        // Fondo de la barra
        GameObject barraBG = new GameObject("BarraFondo");
        barraBG.transform.SetParent(panelVida.transform, false);
        RectTransform rtBG = barraBG.AddComponent<RectTransform>();
        rtBG.anchorMin = new Vector2(0f, 0.5f);
        rtBG.anchorMax = new Vector2(1f, 0.5f);
        rtBG.offsetMin = new Vector2(80f, -15f);
        rtBG.offsetMax = new Vector2(-20f, 15f);
        Image imgBG = barraBG.AddComponent<Image>();
        imgBG.color = colorBarraFondo;

        // Llenado de la barra
        GameObject barraFill = new GameObject("BarraLlenado");
        barraFill.transform.SetParent(barraBG.transform, false);
        RectTransform rtFill = barraFill.AddComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.offsetMin = new Vector2(2f, 2f);
        rtFill.offsetMax = new Vector2(-2f, -2f);
        fillVida = barraFill.AddComponent<Image>();
        fillVida.color = colorVidaNormal;
        fillVida.type = Image.Type.Filled;
        fillVida.fillMethod = Image.FillMethod.Horizontal;
        fillVida.fillOrigin = 0;

        // Texto numérico de vida
        txtVida = CrearTexto(barraBG.transform, "100 / 100", 22, FontStyle.Bold, colorTexto, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 30));

        // ── RONDAS / OLEADAS (Arriba Centro) ──
        GameObject panelRonda = CrearPanelBase("PanelRonda", canvasObj.transform, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(300f, 50f));
        txtRonda = CrearTexto(panelRonda.transform, "RONDA 1", 30, FontStyle.Bold, colorRonda, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 50));

        // ── CROSSHAIR (Centro de la pantalla) ──
        CrearCrosshair(canvasObj.transform);

        // ── ELIMINACIONES DE ZOMBIES (Arriba Derecha) ──
        GameObject panelZombies = CrearPanelBase("PanelZombies", canvasObj.transform, new Vector2(1f, 1f), new Vector2(-40f, -40f), new Vector2(260f, 50f));
        // Icono de calavera simple (o texto)
        Text iconoSkull = CrearTexto(panelZombies.transform, "☠", 30, FontStyle.Normal, colorVidaBaja, new Vector2(0f, 0.5f), new Vector2(25, 3), new Vector2(40, 50));
        txtZombies = CrearTexto(panelZombies.transform, "ELIMINADOS: 0", 22, FontStyle.Bold, colorTexto, new Vector2(0.5f, 0.5f), new Vector2(20, 0), new Vector2(200, 50));
        
        Debug.Log("[AutoHUDTool] HUD de Vida, Rondas y Eliminaciones creado profesionalmente.");
    }

    void CrearCrosshair(Transform parent)
    {
        float size = 20f;
        float thickness = 2f;
        float gap = 6f;

        GameObject cg = new GameObject("CrosshairGroup");
        cg.transform.SetParent(parent, false);
        RectTransform rt = cg.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        CrearLineaCross(cg.transform, "L", new Vector2(-gap - size/2, 0), new Vector2(size, thickness));
        CrearLineaCross(cg.transform, "R", new Vector2(gap + size/2, 0), new Vector2(size, thickness));
        CrearLineaCross(cg.transform, "T", new Vector2(0, gap + size/2), new Vector2(thickness, size));
        CrearLineaCross(cg.transform, "B", new Vector2(0, -gap - size/2), new Vector2(thickness, size));
        CrearLineaCross(cg.transform, "Centro", Vector2.zero, new Vector2(3f, 3f));
    }

    void CrearLineaCross(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        GameObject line = new GameObject("Cross_" + name);
        line.transform.SetParent(parent, false);
        RectTransform rt = line.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = line.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.8f);
        img.raycastTarget = false;

        Outline outline = line.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(1, -1);
    }

    void ActualizarHUD()
    {
        // 1. VIDA DEL JUGADOR
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Efecto suave de llenado
            smoothHealth = Mathf.Lerp(smoothHealth, playerHealth.currentHealth, Time.deltaTime * 6f);
            float porcentaje = smoothHealth / playerHealth.maxHealth;
            
            fillVida.fillAmount = porcentaje;
            fillVida.color = Color.Lerp(colorVidaBaja, colorVidaNormal, porcentaje);
            txtVida.text = $"{Mathf.CeilToInt(playerHealth.currentHealth)} / {playerHealth.maxHealth}";

            // Pantalla roja al recibir daño (detectar si la vida bajó bruscamente real vs animada)
            float realToSmoothDif = Mathf.Clamp(smoothHealth - playerHealth.currentHealth, 0, 100);
            if (realToSmoothDif > 1f)
            {
                damageFlasher.color = new Color(0.8f, 0f, 0f, Mathf.Min(0.5f, realToSmoothDif / 20f));
            }
            else
            {
                damageFlasher.color = Color.Lerp(damageFlasher.color, Color.clear, Time.deltaTime * 3f);
            }
        }

        // 2. RONDAS Y ELIMINACIONES
        if (GameManager.Instance != null)
        {
            int rondaActual = GameManager.Instance.currentWave;
            int zombiesKilled = GameManager.Instance.zombiesKilled;

            txtRonda.text = $"RONDA {rondaActual}";
            txtZombies.text = $"BAJAS: {zombiesKilled}";

            // Animación sencilla cuando cambia la ronda
            if (ultimaRonda != -1 && ultimaRonda != rondaActual)
            {
                StartCoroutine(AnimarCambioRonda());
            }
            ultimaRonda = rondaActual;
        }
    }

    IEnumerator AnimarCambioRonda()
    {
        // Efecto de pulso / escalado
        float duration = 0.5f;
        float elapsed = 0f;
        RectTransform rt = txtRonda.rectTransform;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.PingPong(elapsed * 4f, 0.4f);
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private GameObject CrearPanelBase(string nombre, Transform padre, Vector2 anchor, Vector2 offsetCentro, Vector2 tamano)
    {
        GameObject panelObj = new GameObject(nombre);
        panelObj.transform.SetParent(padre, false);
        
        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor; // El pivot coincide con su punto de anclaje
        
        // Ajustar posicionamientos dependiendo del anclaje
        float posX = anchor.x == 0 ? offsetCentro.x : (anchor.x == 1 ? offsetCentro.x : 0);
        float posY = anchor.y == 0 ? offsetCentro.y : (anchor.y == 1 ? offsetCentro.y : 0);
        if (anchor.x == 0.5f) posX = offsetCentro.x;
        if (anchor.y == 0.5f) posY = offsetCentro.y;

        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta = tamano;

        Image img = panelObj.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.08f, 0.75f); // Fondo semitransparente
        img.raycastTarget = false;

        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return panelObj;
    }

    private Text CrearTexto(Transform padre, string contenido, int fontsize, FontStyle style, Color color, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject txtObj = new GameObject($"Txt_{contenido}");
        txtObj.transform.SetParent(padre, false);
        
        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text txt = txtObj.AddComponent<Text>();
        txt.text = contenido;
        txt.font = fuente;
        txt.fontSize = fontsize;
        txt.fontStyle = style;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        
        Shadow sh = txtObj.AddComponent<Shadow>();
        sh.effectColor = new Color(0,0,0, 0.8f);
        sh.effectDistance = new Vector2(1.5f, -1.5f);

        return txt;
    }
}
