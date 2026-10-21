using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerCharacter : MonoBehaviour {
    [Header("Debugging")]
    public bool setDead = false;
    public bool IsReviving => _isReviving;
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int _health;
    [Header("Movement Speed")]
    [SerializeField] private int _baseSpeed = 6;
    private int _speed = 6;
    [Header("Perks Bought")]
    [SerializeField] private bool boughtQuickRevive = false;
    [SerializeField] private bool boughtSpeedCola = false;
    [SerializeField] private bool boughtJuggernog = false;
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject revivePanel;
    [SerializeField] private Slider reviveProgressBar;
    [Header("Revive Settings")]
    [SerializeField] private float reviveTime = 3.0f;
    [SerializeField] private float invincibilityDuration = 3.0f; //Invincibility after self revive
    private WeaponAnimator _weaponAnimator;
    private bool _isReviving = false;
    private bool _isInvincible = false;
    private float invincibilityEndTime = 0f;
    public bool IsDead => _health <= 0;
    public float crouchSpeed = 3f;
    public CrouchCoroutine _crouch;
    [Header("Health Regeneration")]
    [SerializeField] private float regenRate = 10f;
    [SerializeField] private float regenDelay = 2f;
    private float lastHitTime = -999f;
    [Header("Damage Vignette")]
    [SerializeField] private DamageVignette damageVignette;
    [Header("Mouse Look References")]
    [Tooltip("MouseLook sul Player (rotazione orizzontale X)")]
    [SerializeField] private MouseLook playerMouseLook;
    [Tooltip("MouseLook sulla Camera (rotazione verticale Y)")]
    [SerializeField] private MouseLook cameraMouseLook;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reviveClip;
    [SerializeField] private AudioClip invincibilityClip;

    public void Start() {
        _health = maxHealth;
        _speed = _baseSpeed;
        _weaponAnimator = GetComponentInChildren<WeaponAnimator>();
        _crouch = GetComponent<CrouchCoroutine>();
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (damageVignette == null) damageVignette = FindFirstObjectByType<DamageVignette>(); //Red vignette for blood
        if (damageVignette == null) {
            GameObject vignetteObj = new GameObject("DamageVignetteManager");
            damageVignette = vignetteObj.AddComponent<DamageVignette>();
        }

        //Closes panels
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (revivePanel) revivePanel.SetActive(false);

        UpdateVignette();
    }

    public void Update()
    {
        if (setDead) {
            Hit(1234);
            setDead = false;
        }

        if (_isInvincible && Time.time >= invincibilityEndTime) _isInvincible = false;

        HandleHealthRegeneration();
    }

    private float _healthRegenAccumulator = 0f;

    private void HandleHealthRegeneration()
    {
        if (IsDead || _isReviving) return;

        if (_health >= maxHealth) {
            _healthRegenAccumulator = 0f;

            return;
        }

        //Waits for delay after last hit
        if (Time.time - lastHitTime < regenDelay) return;

        //Regenerates health
        _healthRegenAccumulator += regenRate * Time.deltaTime;
        
        if (_healthRegenAccumulator >= 1f) {
            int healthToAdd = Mathf.FloorToInt(_healthRegenAccumulator);
            _health += healthToAdd;
            _healthRegenAccumulator -= healthToAdd;
            _health = Mathf.Min(_health, maxHealth);

            UpdateVignette();
        }
    }

    private void UpdateVignette()
    {
        if (damageVignette != null) {
            float healthPercent = ((float)_health / maxHealth) * 100f;
            damageVignette.UpdateVignette(healthPercent);
        }
    }

    public void Hit(int damage = 1) {
        //Ignores damage if dead/reviving/invincible
        if (IsDead || _isReviving || _isInvincible) return;

        _health -= damage;
        lastHitTime = Time.time;
        Debug.Log($"Player Health: {_health}/{maxHealth}");
        UpdateVignette();

        if (damageVignette != null) damageVignette.FlashDamage();

        if (_health <= 0) {
            if (boughtQuickRevive) StartCoroutine(HandleQuickRevive());
            else HandleTrueDeath();
        }
    }

    //Game over logic without Quick revive
    private void HandleTrueDeath() {
        GameOverScreen gameOverScreen = FindFirstObjectByType<GameOverScreen>();

        if (gameOverScreen != null) {
            gameOverScreen.Show();

            return;
        }

        //Fallback to old panel if GameOverScreen not found
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (gameOverPanel) gameOverPanel.SetActive(true);
        
        DisablePlayerControls();
        Time.timeScale = 0f;
    }

    private void DisablePlayerControls() {
        //MouseLook
        if (playerMouseLook != null) playerMouseLook.enabled = false;
        if (cameraMouseLook != null) cameraMouseLook.enabled = false;

        //Movement
        FPSInput fpsInput = GetComponent<FPSInput>();
        if (fpsInput != null) fpsInput.enabled = false;

        //CharacterController
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        //Crosshair
        CrosshairUI crosshair = FindFirstObjectByType<CrosshairUI>();
        if (crosshair != null) crosshair.gameObject.SetActive(false);
        GameObject crosshairObj = GameObject.Find("Crosshair");
        if (crosshairObj != null) crosshairObj.SetActive(false);
        crosshairObj = GameObject.Find("CrosshairUI");
        if (crosshairObj != null) crosshairObj.SetActive(false);
    }

    //Quick revive logic with Perk
    private IEnumerator HandleQuickRevive()
    {
        _isReviving = true;
        Debug.Log("Starting Quick revive...");

        this.gameObject.tag = "Untagged"; //Zombies don't target player
        ReviveScreen.ShowRevive();
        if (revivePanel) revivePanel.SetActive(true);
        
        //Revive sound
        if (audioSource && reviveClip) {
            float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.clip = reviveClip;
            audioSource.volume = vol;
            audioSource.loop = true;
            audioSource.Play();
        }

        //progress bar
        float timer = 0f;
        if (reviveProgressBar) {
            reviveProgressBar.minValue = 0;
            reviveProgressBar.maxValue = 1;
            reviveProgressBar.value = 0;
        }

        while (timer < reviveTime) {
            timer += Time.deltaTime;
            float progress = timer / reviveTime;
            
            //Updates slider (0-1)
            if (reviveProgressBar) reviveProgressBar.value = progress;
            
            ReviveScreen.UpdateProgress(progress);

            yield return null; //Skips to next frame (its used in coroutines created with IEnumerator)
        }
        
        //Stops sound
        if (audioSource && audioSource.clip == reviveClip) {
            audioSource.Stop();
            audioSource.loop = false;
        }

        //Revive complete
        _health = maxHealth;
        _isReviving = false;
        _isInvincible = true;
        invincibilityEndTime = Time.time + invincibilityDuration;
        
        //Invincibility sound
        if (audioSource && invincibilityClip) {
            float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.PlayOneShot(invincibilityClip, vol);
        }
        
        Debug.Log($"Invincible for {invincibilityDuration} seconds!");

        //Lose perks/boosts
        boughtQuickRevive = false;
        _speed = _baseSpeed;
        boughtSpeedCola = false;
        boughtJuggernog = false;
        AmmoManager ammoManager = GetComponent<AmmoManager>();
        
        if (ammoManager != null) ammoManager.ResetReloadSpeed();
        if (_weaponAnimator != null) _weaponAnimator.speedUpReload(1f);

        //Player is back to normal
        this.gameObject.tag = "Player";
        ReviveScreen.HideRevive();
        if (revivePanel) revivePanel.SetActive(false);

        UpdateVignette();
    }

    public void RetryGame() {
        Time.timeScale = 1f;
        //Reloads scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame() {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void SetHealth(int value) => maxHealth = value;
    public void SetSpeed(int speedBonus) { _speed += speedBonus; }
    public void SetAutoRevive(bool autoReviveBonus) => boughtQuickRevive = autoReviveBonus;

    public float GetCurrentSpeed()
    {
        if (IsDead || IsReviving) return 0f;

        if (_crouch != null && _crouch.IsCrouching) return crouchSpeed;

        return _speed;
    }
    public WeaponAnimator GetWeaponAnimator()
    {
        if (_weaponAnimator == null)
            _weaponAnimator = GetComponentInChildren<WeaponAnimator>();
        return _weaponAnimator;
    }

    public void RefreshWeaponAnimator()
    {
        WeaponAnimator[] animators = GetComponentsInChildren<WeaponAnimator>();
        _weaponAnimator = animators.Length > 0 ? animators[animators.Length - 1] : null;
    }
    public bool hasPurchasedPerk(string perkName)
    {
        switch (perkName) {
            case "SpeedCola":
                return boughtSpeedCola;
            case "Juggernog!":
                return boughtJuggernog;
            case "Quick Revive":
                return boughtQuickRevive;
            default:
                Debug.LogWarning("Unknown perk name: " + perkName);
                return false;
        }
    }
    public bool boughtSpeedColaStatus() => boughtSpeedCola;
    public bool boughtJuggernogStatus() => boughtJuggernog;
    public bool boughtQuickReviveStatus() => boughtQuickRevive;
    public void SetBoughtSpeedCola(bool status) => boughtSpeedCola = status;
    public void SetBoughtJuggernog(bool status) => boughtJuggernog = status;
}
