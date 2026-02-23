using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador global de brillo que persiste entre escenas.
/// Se crea automáticamente si no existe. Usa un overlay de pantalla completa
/// para simular cambios de brillo.
/// </summary>
public class BrightnessController : MonoBehaviour
{
    public static BrightnessController Instance;

    private Canvas brightnessCanvas;
    private Image overlayImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CrearOverlay();
        AplicarBrilloGuardado();
    }

    void CrearOverlay()
    {
        // Canvas propio para el overlay de brillo
        GameObject canvasObj = new GameObject("BrightnessCanvas");
        canvasObj.transform.SetParent(transform);

        brightnessCanvas = canvasObj.AddComponent<Canvas>();
        brightnessCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        brightnessCanvas.sortingOrder = 999; // Por encima de todo

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // NO añadir GraphicRaycaster para que no bloquee clics

        // Overlay negro
        GameObject overlayObj = new GameObject("DarknessOverlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);

        overlayImage = overlayObj.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0);
        overlayImage.raycastTarget = false;

        RectTransform rt = overlayObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void AplicarBrilloGuardado()
    {
        float brillo = PlayerPrefs.GetFloat("Brightness", 1f);
        SetBrightness(brillo);
    }

    /// <summary>
    /// Establece el brillo. 1 = máximo brillo, 0 = totalmente oscuro.
    /// </summary>
    public void SetBrightness(float valor)
    {
        if (overlayImage != null)
        {
            float oscuridad = 1f - Mathf.Clamp01(valor);
            overlayImage.color = new Color(0, 0, 0, oscuridad * 0.85f);
        }
        PlayerPrefs.SetFloat("Brightness", valor);
    }

    /// <summary>
    /// Devuelve el brillo actual (0-1).
    /// </summary>
    public float GetBrightness()
    {
        return PlayerPrefs.GetFloat("Brightness", 1f);
    }
}
