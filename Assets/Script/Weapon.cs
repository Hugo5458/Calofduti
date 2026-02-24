using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [Header("Configuración del Arma")]
    public string weaponName = "Pistola";
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.25f;
    public float impactForce = 30f;
    
    [Header("Munición")]
    public int maxAmmo = 30;
    public int currentAmmo;
    public int reserveAmmo = 90;
    public float reloadTime = 1.5f;
    
    [Header("Referencias")]
    public Camera fpsCam;
    public Transform firePoint;
    public GameObject muzzleFlash;
    public GameObject impactEffect;
    public GameObject bulletPrefab;
    
    [Header("UI")]
    public Text ammoText;
    
    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    
    private AudioSource audioSource;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private Animation anim;
    
    void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        
        // Buscar componente Animation (legacy) en este objeto o en los hijos
        anim = GetComponent<Animation>();
        if (anim == null)
        {
            anim = GetComponentInChildren<Animation>();
        }
        
        if (anim != null)
        {
            Debug.Log("[Weapon] Animation encontrado en: " + anim.gameObject.name);
            
            // Forzar que la animación Shoot NO se repita en loop
            if (anim.GetClip("Shoot") != null)
            {
                anim["Shoot"].wrapMode = WrapMode.Once;
            }
        }
        else
        {
            Debug.LogWarning("[Weapon] NO se encontró componente Animation en " + gameObject.name + " ni en sus hijos.");
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (fpsCam == null)
        {
            fpsCam = Camera.main;
        }
        
        UpdateAmmoUI();
    }
    
    void Update()
    {
        if (isReloading) return;
        
        // Recargar con R
        if (Input.GetKeyDown(KeyCode.R) && reserveAmmo > 0 && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }
        
        // Disparar manteniendo click pulsado
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
    }
    
    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            // Sonido de arma vacía
            if (emptySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(emptySound);
            }
            
            // Auto-recargar si quedan balas en reserva
            if (reserveAmmo > 0)
            {
                StartCoroutine(Reload());
            }
            return;
        }
        
        currentAmmo--;
        UpdateAmmoUI();
        
        // Efecto de fogonazo
        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }
        
        // Sonido de disparo
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // Animación de disparo (legacy Animation)
        if (anim != null && anim.GetClip("Shoot") != null)
        {
            anim.Rewind("Shoot");
            anim.Play("Shoot");
        }
        
        // Raycast para detectar impacto
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Impacto en: " + hit.transform.name);
            
            // Intentar obtener ZombieHitbox (partes del cuerpo)
            ZombieHitbox hitbox = hit.transform.GetComponent<ZombieHitbox>();
            if (hitbox != null)
            {
                hitbox.TakeDamage(damage);
            }
            else
            {
                // Si no tiene hitbox, buscar ZombieHealth directamente (en el objeto o sus padres)
                ZombieHealth zombie = hit.transform.GetComponentInParent<ZombieHealth>();
                if (zombie != null)
                {
                    zombie.TakeDamage(damage);
                }
            }
            
            // Aplicar fuerza física
            Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(-hit.normal * impactForce);
            }
            
            // Efecto de impacto
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 2f);
            }
        }
    }
    
    System.Collections.IEnumerator Reload()
    {
        if (reserveAmmo <= 0 || currentAmmo == maxAmmo) yield break;
        
        isReloading = true;
        
        // Sonido de recarga
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        // Animación de recarga (legacy Animation)
        float waitTime = reloadTime;
        if (anim != null && anim.GetClip("Reload") != null)
        {
            anim["Reload"].wrapMode = WrapMode.Once;
            anim.Rewind("Reload");
            anim.Play("Reload");
            // Esperar la duración real de la animación de recarga
            waitTime = anim["Reload"].length;
        }
        
        yield return new WaitForSeconds(waitTime);
        
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
        
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;
        
        isReloading = false;
        UpdateAmmoUI();
    }
    
    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + reserveAmmo;
        }
    }
    
    public void AddAmmo(int amount)
    {
        reserveAmmo += amount;
        UpdateAmmoUI();
    }
}
