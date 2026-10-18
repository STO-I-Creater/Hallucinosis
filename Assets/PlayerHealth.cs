using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; 
using UnityEngine.UI; 

/// <summary>
/// Manages the player's health, damage, and death.
/// Persists health data through the GameManager across scenes.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;

    [Tooltip("The current health of the player.")]
    public int currentHealth;

    [Header("UI & Respawn")]
    [Tooltip("Drag your 'DeathPanel' GameObject here from the Hierarchy.")]
    public GameObject deathPanel; 

    [Header("Low HP Warning")]
    [Tooltip("The health percentage (0.0 to 1.0) when the warning starts.")]
    [Range(0.0f, 0.5f)]
    public float lowHPThreshold = 0.25f; 
    [Tooltip("The UI Image used for the red screen vignette.")]
    public Image lowHPVignette; 
    [Tooltip("The sound clip for the heart beat.")]
    public AudioClip heartbeatSound; 
    [Tooltip("The volume of the heartbeat sound (0.0 to 1.0).")]
    [Range(0f, 1f)]
    public float heartbeatVolume = 0.6f; 
    [Tooltip("Time it takes to fade the red screen in/out.")]
    public float vignetteFadeTime = 0.5f;
    
    [Header("Death & Respawn Audio")]
    public AudioClip deathSound;
    [Range(0f, 1f)]
    public float deathVolume = 1.0f;
    public AudioClip respawnSound;
    [Range(0f, 1f)]
    public float respawnVolume = 0.8f;

    [Header("Damage Feedback")]
    public float flashDuration = 0.15f; 
    public Color flashColor = Color.white; 

    [Header("Events")]
    public UnityEvent<int> OnMaxHealthSet;
    public UnityEvent<int> OnHealthChanged;

    // --- Private References ---
    private PlayerMovement movementScript;
    private PlayerWallSlide wallSlideScript;
    private PlayerEffects playerEffects;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr; 
    private Coroutine vignetteCoroutine; 
    private Coroutine heartbeatLoopCoroutine; 
    private Coroutine damageFlashCoroutine; 
    private AudioSource heartbeatSource; 
    private Color originalSpriteColor; 

    void Awake()
    {
        // --- Persistence Logic ---
        if (GameManager.Instance != null && GameManager.Instance.playerStoredHealth != -1)
        {
            // Load health from GameManager if it has been saved before
            currentHealth = GameManager.Instance.playerStoredHealth;
        }
        else
        {
            // First time loading or no GameManager found
            currentHealth = maxHealth;
            if (GameManager.Instance != null) GameManager.Instance.playerStoredHealth = maxHealth;
        }

        // Get references
        movementScript = GetComponent<PlayerMovement>();
        wallSlideScript = GetComponent<PlayerWallSlide>();
        playerEffects = GetComponent<PlayerEffects>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>(); 
        
        if (sr != null) originalSpriteColor = sr.color;
        
        // Audio Setup
        heartbeatSource = gameObject.AddComponent<AudioSource>();
        heartbeatSource.playOnAwake = false;
        heartbeatSource.spatialBlend = 0f; 
        
        if(deathPanel != null) deathPanel.SetActive(false);
        
        if (lowHPVignette != null)
        {
            lowHPVignette.gameObject.SetActive(false);
            Color c = lowHPVignette.color;
            lowHPVignette.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void Start()
    {
        // Update UI at start
        OnMaxHealthSet?.Invoke(maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        
        // Check if we spawned into a new scene with low health
        CheckLowHealth();
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damageAmount;
        SaveHealthToManager(); // Keep GameManager updated

        OnHealthChanged?.Invoke(currentHealth);
        
        if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(DamageFlashSequence());
        
        CheckLowHealth();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            SaveHealthToManager();
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        SaveHealthToManager(); // Keep GameManager updated
        OnHealthChanged?.Invoke(currentHealth);
        CheckLowHealth();
    }

    private void SaveHealthToManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerStoredHealth = currentHealth;
        }
    }

    private IEnumerator DamageFlashSequence()
    {
        if (sr != null)
        {
            sr.color = flashColor; 
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalSpriteColor; 
        }
        damageFlashCoroutine = null;
    }

    private void CheckLowHealth()
    {
        float healthRatio = (float)currentHealth / maxHealth;
        bool isLow = healthRatio <= lowHPThreshold && currentHealth > 0;

        if (isLow)
        {
            if (heartbeatLoopCoroutine == null)
                heartbeatLoopCoroutine = StartCoroutine(HeartbeatLoop());

            if (lowHPVignette != null)
            {
                if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
                vignetteCoroutine = StartCoroutine(FadeVignette(0.5f));
            }
        }
        else
        {
            if (heartbeatLoopCoroutine != null)
            {
                StopCoroutine(heartbeatLoopCoroutine);
                heartbeatLoopCoroutine = null;
                if (heartbeatSource != null) heartbeatSource.Stop();
            }
            
            if (lowHPVignette != null)
            {
                if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
                vignetteCoroutine = StartCoroutine(FadeVignette(0f));
            }
        }
    }

    private IEnumerator HeartbeatLoop()
    {
        while (true)
        {
            if (heartbeatSound != null)
            {
                heartbeatSource.PlayOneShot(heartbeatSound, heartbeatVolume);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator FadeVignette(float targetAlpha)
    {
        if (lowHPVignette == null) yield break;

        if (targetAlpha > 0f) lowHPVignette.gameObject.SetActive(true);

        Color startColor = lowHPVignette.color;
        float startAlpha = startColor.a;
        float elapsed = 0f;

        while (elapsed < vignetteFadeTime)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / vignetteFadeTime);
            lowHPVignette.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }

        lowHPVignette.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        if (targetAlpha == 0f) lowHPVignette.gameObject.SetActive(false);
        vignetteCoroutine = null;
    }

    private void Die()
    {
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);
        
        if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
        if (sr != null) sr.color = originalSpriteColor;

        if (heartbeatLoopCoroutine != null) StopCoroutine(heartbeatLoopCoroutine);
        if (heartbeatSource != null) heartbeatSource.Stop();

        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.isKinematic = true; }
        if (movementScript != null) movementScript.enabled = false;
        if (wallSlideScript != null) wallSlideScript.enabled = false;
        if (playerEffects != null) playerEffects.enabled = false;
        if (col != null) col.enabled = false;
        
        if (deathPanel != null) deathPanel.SetActive(true);
    }

    public void Respawn()
    {
        if (respawnSound != null) AudioSource.PlayClipAtPoint(respawnSound, transform.position, respawnVolume);
        
        currentHealth = maxHealth;
        SaveHealthToManager();
        OnHealthChanged?.Invoke(currentHealth);

        // Position Logic
        if (GameManager.Instance != null && GameManager.Instance.savedCheckpoint != Vector2.zero)
        {
            transform.position = GameManager.Instance.savedCheckpoint;
        }

        // Reset State
        if (movementScript != null) movementScript.enabled = true;
        if (wallSlideScript != null) wallSlideScript.enabled = true;
        if (playerEffects != null) playerEffects.enabled = true;
        if (col != null) col.enabled = true;
        if (rb != null) { rb.isKinematic = false; rb.linearVelocity = Vector2.zero; }
        
        if (lowHPVignette != null) lowHPVignette.gameObject.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        
        CheckLowHealth(); // Ensure effects stop
    }
}