using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionDelay = 0f;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private float upwardsModifier = 3f;
    [Header("Damage Settings")]
    [SerializeField] private int maxDamage = 100;
    [SerializeField] private bool dealDamage = true;
    [Header("Effects (Optional)")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioClip explosionSound;
    [Obsolete("Use activationSound for throw sound")]
    [SerializeField] private AudioClip startSound; // Backwards compatibility if needed, but preferred:
    [SerializeField] private AudioClip activationSound; // Sound played when grenade activates (thrown)
    private float timer; // Restored timer
    private bool hasExploded = false; // Restored hasExploded
    private float soundTimer;
    private bool soundPlayed = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Start countdowns
        timer = explosionDelay;
        soundTimer = explosionDelay;
        soundPlayed = false;
        
        // Play activation sound (monkey music/noise)
        if (activationSound != null)
        {
            float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            AudioSource.PlayClipAtPoint(activationSound, transform.position, vol);
        }
    }

    private void Update()
    {
        if (hasExploded) return;
        
        float dt = Time.deltaTime;
        timer -= dt;
        
        // Handle Early Sound Playback
        if (!soundPlayed)
        {
            soundTimer -= dt;
            if (soundTimer <= 0f)
            {
                PlayExplosionSound();
                soundPlayed = true;
            }
        }
        
        if (timer <= 0f)
        {
            Explode();
        }
    }
    
    // Separated Sound Playback
    private void PlayExplosionSound()
    {
        if (explosionSound != null)
        {
            float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, vol);
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // Ensure sound plays if it hasn't somehow (e.g. ExplodeNow called)
        if (!soundPlayed) 
        {
            PlayExplosionSound();
            soundPlayed = true;
        }

        Vector3 explosionPosition = transform.position;

        // Trova tutti i collider nel raggio dell'esplosione (molto efficiente)
        // ... (rest of Explode)
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        
        foreach (Collider hit in colliders)
        {
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            
            // Applica forza fisica solo ai Rigidbody
            if (hitRb != null)
            {
                // Usa AddExplosionForce - metodo nativo Unity, molto ottimizzato
                hitRb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, ForceMode.Impulse);
            }
            
            // Gestione danni (opzionale)
            if (dealDamage)
            {
                ApplyDamage(hit, explosionPosition);
            }
        }

        // Effetti visivi (audio handled separately now)
        SpawnEffects(explosionPosition);
        
        // Distruggi la granata
        Destroy(gameObject);
    }
    
    private void ApplyDamage(Collider hit, Vector3 explosionPosition)
    {
        // Calcola danno in base alla distanza (più vicino = più danno)
        float distance = Vector3.Distance(explosionPosition, hit.transform.position);
        float damagePercent = 1f - (distance / explosionRadius);
        int damage = Mathf.RoundToInt(maxDamage * damagePercent);

        // Prova a danneggiare zombie
        ZombieAISimple zombie = hit.GetComponent<ZombieAISimple>();
        if (zombie != null)
        {
            zombie.TakeDamage(damage);
            return;
        }

        // Prova a danneggiare player (usa PlayerCharacter, non PlayerHealth)
        PlayerCharacter playerChar = hit.GetComponent<PlayerCharacter>();
        if (playerChar != null)
        {
            playerChar.Hit(damage);
            return;
        }

        // Aggiungi qui altri componenti danneggiabili se necessario
    } 

    private void SpawnEffects(Vector3 position)
    {
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Audio now handled via PlayExplosionSound / Update loop
    }

    public void Activate()
    {
        timer = explosionDelay;
        // Schedule sound 1 second BEFORE explosion
        float soundDelay = Mathf.Max(0f, explosionDelay - 1.0f);
        soundTimer = soundDelay;

        hasExploded = false;
        soundPlayed = false;
        
        // Re-play activation sound if re-used
        if (activationSound != null)
        {
             float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            AudioSource.PlayClipAtPoint(activationSound, transform.position, vol);
        }
    }

    public void ExplodeNow()
    {
        timer = 0f;
    }
    

    public void SetInitialVelocity(Vector3 direction, float forwardForce, float upwardForce)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>(); // Ensure rb is assigned
            
        direction.Normalize();
        Vector3 velocity = direction * forwardForce + Vector3.up * upwardForce;
        rb.linearVelocity = velocity;  
    }

}
