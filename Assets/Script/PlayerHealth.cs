using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("UI")]
    public Slider healthSlider;
    public Image damageImage;
    public Text healthText;
    
    [Header("Efectos de Daño")]
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Header("Audio")]
    public AudioClip hurtSound;
    public AudioClip deathSound;
    
    private AudioSource audioSource;
    private bool isDead = false;
    private bool damaged = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        UpdateUI();
    }
    
    void Update()
    {
        // Efecto visual de daño
        if (damageImage != null)
        {
            if (damaged)
            {
                damageImage.color = flashColor;
            }
            else
            {
                damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }
            damaged = false;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        damaged = true;
        currentHealth -= damage;
        
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        UpdateUI();
        
        if (currentHealth <= 0)
        {
            Death();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = "Vida: " + Mathf.RoundToInt(currentHealth).ToString();
        }
    }
    
    void Death()
    {
        isDead = true;
        
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Desactivar controles del jugador
        var controller = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Reiniciar escena después de un delay
        Invoke("RestartGame", 3f);
    }
    
    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}
