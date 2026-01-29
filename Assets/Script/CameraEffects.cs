using UnityEngine;

/// <summary>
/// Efecto de cámara para cuando el jugador recibe daño.
/// Añadir a la cámara principal del jugador.
/// </summary>
public class CameraEffects : MonoBehaviour
{
    [Header("Screen Shake")]
    public float shakeDuration = 0.2f;
    public float shakeIntensity = 0.1f;
    
    [Header("Blood Effect")]
    public GameObject bloodOverlay;
    public float bloodFadeSpeed = 2f;
    
    private Vector3 originalPosition;
    private float currentShakeDuration = 0f;
    private bool isShaking = false;
    
    void Start()
    {
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        if (isShaking)
        {
            if (currentShakeDuration > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
                currentShakeDuration -= Time.deltaTime;
            }
            else
            {
                isShaking = false;
                transform.localPosition = originalPosition;
            }
        }
    }
    
    public void ShakeCamera()
    {
        currentShakeDuration = shakeDuration;
        isShaking = true;
    }
    
    public void ShakeCamera(float duration, float intensity)
    {
        currentShakeDuration = duration;
        shakeIntensity = intensity;
        isShaking = true;
    }
}
