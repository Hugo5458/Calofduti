using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponBox : MonoBehaviour
{
    [Header("Interacción")]
    public KeyCode interactionKey = KeyCode.E;
    public string interactionText = "Presiona E para abrir la caja";
    public float openTime = 2.0f; // Tiempo simulado para abrir la caja

    [Header("Armas (Prefabs)")]
    // Arrastra aquí los prefabs de las armas (o Pickups) que quieres que salgan
    public List<GameObject> weaponPrefabs; 
    public Transform spawnPoint; // Asigna un objeto vacío hijo donde aparecerá el arma

    [Header("Referencias Opcionales")]
    public Animator boxAnimator; // Si tienes animación de abrir
    public AudioClip openSound;
    public AudioClip spawnSound;

    private bool playerInRange = false;
    private bool isOpening = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Detectar pulsación de tecla solo si el jugador está cerca y no se está abriendo ya
        if (playerInRange && !isOpening)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                StartCoroutine(OpenBoxRoutine());
            }
        }
    }

    IEnumerator OpenBoxRoutine()
    {
        isOpening = true;

        // Reproducir sonido de apertura
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Activar trigger de animación 'Open'
        if (boxAnimator != null)
        {
            boxAnimator.SetTrigger("Open");
        }

        // Esperar el tiempo configurado (simulando que se abre o busca el arma)
        yield return new WaitForSeconds(openTime);

        SpawnRandomWeapon();

        // Esperar un momento antes de volver a cerrar (o permitir interactuar)
        yield return new WaitForSeconds(1f);

        // Volver a estado idle/cerrado
        if (boxAnimator != null)
        {
            boxAnimator.SetTrigger("Close");
        }

        isOpening = false;
    }

    void SpawnRandomWeapon()
    {
        if (weaponPrefabs == null || weaponPrefabs.Count == 0)
        {
            Debug.LogWarning("WeaponBox: No hay armas asignadas en la lista 'Weapon Prefabs'.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("WeaponBox: No hay 'Spawn Point' asignado. Usando la posición de la caja.");
            spawnPoint = transform;
        }

        // Elegir índice aleatorio
        int randomIndex = Random.Range(0, weaponPrefabs.Count);
        GameObject selectedWeapon = weaponPrefabs[randomIndex];

        // Instanciar el arma en el punto de spawn
        if (selectedWeapon != null)
        {
            GameObject newWeapon = Instantiate(selectedWeapon, spawnPoint.position, spawnPoint.rotation);
            
            // Opcional: dar un pequeño impulso físico si tiene Rigidbody
            Rigidbody rb = newWeapon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
            }

            // Sonido de aparición de arma
            if (spawnSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(spawnSound);
            }

            Debug.Log("Salió el arma: " + selectedWeapon.name);
        }
    }

    // Detectar cuando el jugador entra en el área
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // Detectar cuando el jugador sale del área
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // Dibujar la interfaz (texto) en pantalla
    void OnGUI()
    {
        if (playerInRange && !isOpening)
        {
            // Estilo básico de etiqueta en el centro-abajo de la pantalla
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            float width = 300f;
            float height = 50f;
            float x = (Screen.width - width) / 2;
            float y = (Screen.height - height) / 2 + 100; // Un poco más abajo del centro

            GUI.Label(new Rect(x, y, width, height), interactionText, style);
        }
    }
}
