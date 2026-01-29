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
    private Animator animator;
    
    void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
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
        
        // Recargar con R o automáticamente si no hay balas
        if (Input.GetKeyDown(KeyCode.R) || (currentAmmo <= 0 && reserveAmmo > 0))
        {
            StartCoroutine(Reload());
            return;
        }
        
        // Disparar
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
        
        // Animación de disparo
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        // Raycast para detectar impacto
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Impacto en: " + hit.transform.name);
            
            // Aplicar daño al zombie
            ZombieHealth zombie = hit.transform.GetComponent<ZombieHealth>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
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
        
        // Animación de recarga
        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }
        
        yield return new WaitForSeconds(reloadTime);
        
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
