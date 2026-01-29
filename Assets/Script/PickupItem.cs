using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Ammo,
        Weapon
    }
    
    [Header("Configuración")]
    public PickupType pickupType = PickupType.Ammo;
    public float amount = 25f;
    public int ammoAmount = 30;
    
    [Header("Efectos")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.2f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Rotación
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        
        // Movimiento de balanceo
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bool pickedUp = false;
            
            switch (pickupType)
            {
                case PickupType.Health:
                    PlayerHealth health = other.GetComponent<PlayerHealth>();
                    if (health != null && health.currentHealth < health.maxHealth)
                    {
                        health.Heal(amount);
                        pickedUp = true;
                    }
                    break;
                    
                case PickupType.Ammo:
                    Weapon weapon = other.GetComponentInChildren<Weapon>();
                    if (weapon != null)
                    {
                        weapon.AddAmmo(ammoAmount);
                        pickedUp = true;
                    }
                    break;
                    
                case PickupType.Weapon:
                    // Implementar sistema de armas múltiples si es necesario
                    pickedUp = true;
                    break;
            }
            
            if (pickedUp)
            {
                // Reproducir sonido
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }
                
                // Efecto de recogida
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                Destroy(gameObject);
            }
        }
    }
}
