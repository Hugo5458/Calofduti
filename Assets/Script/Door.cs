using UnityEngine;

/// <summary>
/// Script para puerta interactiva.
/// Puede usarse con los prefabs de puertas de country house01.
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Configuración")]
    public bool isOpen = false;
    public bool isLocked = false;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool requiresKey = false;
    public string keyName = "Llave";
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    
    [Header("Interacción")]
    public float interactionDistance = 2f;
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Presiona E para abrir";
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;
    private AudioSource audioSource;
    private bool playerInRange = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        targetRotation = closedRotation;
    }
    
    void Update()
    {
        // Rotar suavemente
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime);
        
        // Detectar input del jugador
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            TryToggleDoor();
        }
    }
    
    void TryToggleDoor()
    {
        if (isLocked)
        {
            if (requiresKey)
            {
                // Aquí podrías verificar si el jugador tiene la llave
                // Por ahora solo reproduce el sonido de bloqueado
                if (lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                Debug.Log("La puerta está cerrada. Necesitas: " + keyName);
            }
            return;
        }
        
        ToggleDoor();
    }
    
    public void ToggleDoor()
    {
        isOpen = !isOpen;
        targetRotation = isOpen ? openRotation : closedRotation;
        
        // Reproducir sonido
        AudioClip sound = isOpen ? openSound : closeSound;
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
    }
    
    public void Lock()
    {
        isLocked = true;
        if (isOpen)
        {
            ToggleDoor(); // Cerrar si está abierta
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    void OnGUI()
    {
        if (playerInRange)
        {
            string text = isLocked ? "Puerta bloqueada" : interactionText;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30), text);
        }
    }
}
