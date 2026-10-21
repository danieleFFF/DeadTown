using System.Collections;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour {
    public enum FireMode { SemiAuto, FullAuto }

    [Header("Weapon Settings")]
    public FireMode fireMode = FireMode.SemiAuto;
    public float fireCooldown = 0.15f;

    [Header("Animation")]
    public Animator animator;

    [Header("References")]
    public PlayerShooting playerShooting;
    public CrosshairUI crosshair;
    public WeaponAiming weaponAiming;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootClip;
    public AudioClip reloadClip;

    [Header("Spread / Crosshair")]
    [Header("Crosshair Weapon Settings")]
    [SerializeField] private float weaponMinGap = 15f;

    public float spreadMinAngleDeg = 0f;
    public float spreadMaxAngleDeg = 6f;
    public float crosshairIncreasePerShot = 6f;
    public float crosshairReturnSpeed = 12f;

    [Header("Sprint Spread Penalty")]
    [SerializeField] private float sprintSpreadBonus = 3f; // Extra spread when sprinting
    [SerializeField] private float sprintCrosshairBonus = 10f; // Extra crosshair gap when sprinting

    private float lastFireTime;
    private float baseMinGap; // Store the original minGap
    private FPSInput fpsInput; // Reference to check if sprinting

    [SerializeField] private AmmoManager ammoManager;
    private bool hasReloadingParam;
    private static readonly int ReloadTriggerHash = Animator.StringToHash("Reload");
    private static readonly int ReloadingParamHash = Animator.StringToHash("Reloading");
    private static readonly int AnimSpeedParamHash = Animator.StringToHash("AnimSpeed");
    private float currentReloadSpeed = 1f;

    void Awake()
    {
        if (!playerShooting)
            playerShooting = FindFirstObjectByType<PlayerShooting>();

        if (!crosshair)
            crosshair = FindFirstObjectByType<CrosshairUI>();

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        if (!ammoManager)
            ammoManager = FindFirstObjectByType<AmmoManager>();

        if (!weaponAiming)
            weaponAiming = FindFirstObjectByType<WeaponAiming>();

        fpsInput = FindFirstObjectByType<FPSInput>();

        if (crosshair)
        {
            crosshair.SetMinGap(weaponMinGap);
            baseMinGap = weaponMinGap;
        }

        CacheAnimatorParams();
        CacheReloadSpeed();
        UpdateShellReloadTiming();
        BindReloadEvents();
    }

    void OnEnable()
    {
        UpdateShellReloadTiming();
        BindReloadEvents();
    }

    void OnDisable()
    {
        UnbindReloadEvents();
    }

    void Update()
    {
        HandleReload();
        HandleSprintSpread();
        HandleCrosshairReturn();
        HandleShootingInput();
    }

    void HandleSprintSpread()
    {
        if (!crosshair || fpsInput == null) return;

        bool isSprinting = fpsInput.IsSprinting();
        float targetMinGap = isSprinting ? baseMinGap + sprintCrosshairBonus : baseMinGap;
        
        // Smooth transition for crosshair minGap
        crosshair.minGap = Mathf.Lerp(crosshair.minGap, targetMinGap, Time.deltaTime * crosshairReturnSpeed);
        
        // Ensure currentGap doesn't go below minGap
        if (crosshair.currentGap < crosshair.minGap)
        {
            crosshair.currentGap = crosshair.minGap;
            crosshair.Apply();
        }
    }

    void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[INPUT] R key pressed - attempting reload");
            
            if (ammoManager == null)
            {
                Debug.LogError("[RELOAD] AmmoManager is NULL!");
                return;
            }
            
            bool reloadResult = ammoManager.TryReload();
            Debug.Log($"[RELOAD] TryReload result: {reloadResult}");
        }
    }

    void HandleReloadStarted()
    {
        if (animator != null)
        {
            animator.SetTrigger(ReloadTriggerHash);
            Debug.Log("[RELOAD] Reload animation triggered");
        }

        //Reload sound
        if (audioSource && reloadClip) {
            float volume = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.PlayOneShot(reloadClip, volume);
        }

        if (ammoManager != null && ammoManager.ReloadsOneByOne() && animator != null && hasReloadingParam)
            animator.SetBool(ReloadingParamHash, true); //Sets reloading to 'True'
    }

    void HandleReloadComplete()
    {
        if (animator != null && hasReloadingParam)
            animator.SetBool(ReloadingParamHash, false);
    }

    void HandleCrosshairReturn()
    {
        if (!crosshair) return;

        crosshair.currentGap = Mathf.Lerp(
            crosshair.currentGap,
            crosshair.minGap,
            Time.deltaTime * crosshairReturnSpeed
        );

        crosshair.Apply();
    }

    void HandleShootingInput()
    {
        // Non sparare se il gioco è in pausa (menu settings aperto)
        if (PauseMenuController.IsPaused)
            return;
            
        bool wantsToShoot =
            (fireMode == FireMode.SemiAuto && Input.GetMouseButtonDown(0)) ||
            (fireMode == FireMode.FullAuto && Input.GetMouseButton(0));

        if (wantsToShoot)
            TryShoot();
    }

    void TryShoot()
    {
        if (ammoManager != null && ammoManager.IsReloading() && ammoManager.CanInterruptReload())
            ammoManager.CancelReload();

        if (Time.time < lastFireTime + fireCooldown)
            return;

        if (ammoManager != null && !ammoManager.TryUseAmmo())
            return;

        Shoot();
        lastFireTime = Time.time;
    }

    void Shoot()
    {
        animator.Play("Shoot", 0, 0f);

        PlayShootSound();
        UpdateCrosshairAndSpread(out float spreadAngle);

        if (playerShooting) playerShooting.ShootWithSpread(spreadAngle);

        Debug.Log($"Shot | Spread: {spreadAngle:F2}");
    }

    [Header("Audio Tuning")]
    [SerializeField] private float cutOffDelay = 0.2f;
    private AudioSource[] audioPool;
    private int poolIndex = 0;
    private int poolSize = 6;
    
    //Uses multiple sources so every source has the same sound reproduced at different times
    void InitializeAudioPool() {
        if (!audioSource) return;

        audioPool = new AudioSource[poolSize];
        audioPool[0] = audioSource; //Uses the existing one as the first

        for (int i = 1; i < poolSize; i++) {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            newSource.spatialBlend = audioSource.spatialBlend;
            newSource.minDistance = audioSource.minDistance;
            newSource.maxDistance = audioSource.maxDistance;
            newSource.rolloffMode = audioSource.rolloffMode;
            newSource.playOnAwake = false;
            audioPool[i] = newSource;
        }
    }

    void PlayShootSound() {
        if (audioPool == null || audioPool.Length == 0) InitializeAudioPool();

        if (shootClip)
        {
            //Stops previous sound (-1) with a delay
            int prevIndex = (poolIndex - 1 + poolSize) % poolSize;
            
            if (audioPool[prevIndex].isPlaying)
                audioPool[prevIndex].SetScheduledEndTime(AudioSettings.dspTime + cutOffDelay);

            //Plays new sound
            AudioSource currentSource = audioPool[poolIndex];
            
            float volume = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            currentSource.clip = shootClip;
            currentSource.volume = volume;
            currentSource.Play();
            
            //Advances index
            poolIndex = (poolIndex + 1) % poolSize;
        }
    }

    [SerializeField] private float aimingBaseSpread = 1.2f; // Spread minimo quando si mira

    void UpdateCrosshairAndSpread(out float spreadAngle)
    {
        spreadAngle = spreadMinAngleDeg;

        if (!crosshair) return;

        // aumenta il gap
        crosshair.currentGap = Mathf.Clamp(
            crosshair.currentGap + crosshairIncreasePerShot,
            crosshair.minGap,
            crosshair.maxGap
        );

        crosshair.Apply();

        // normalizza gap 0–1
        float gap01 = Mathf.InverseLerp(
            crosshair.minGap,
            crosshair.maxGap,
            crosshair.currentGap
        );

        spreadAngle = Mathf.Lerp(
            spreadMinAngleDeg,
            spreadMaxAngleDeg,
            gap01
        );

        if (weaponAiming != null && weaponAiming.IsAiming())
        {
            // Riduciamo lo spread quando si mira, MA manteniamo un'imprecisione di base
            spreadAngle /= 6f; // Riduzione recoil
            
            // IMPONIAMO un minimo di spread anche mirando
            spreadAngle = Mathf.Max(spreadAngle, aimingBaseSpread);
        }
    }

    public void speedUpReload(float speed)
    {
        if (animator == null)
            return;

        if (Mathf.Approximately(currentReloadSpeed, speed))
            return;

        currentReloadSpeed = speed;
        animator.SetFloat(AnimSpeedParamHash, speed);
    }

    private void BindReloadEvents()
    {
        if (ammoManager == null)
            return;

        ammoManager.OnReloadStarted.RemoveListener(HandleReloadStarted);
        ammoManager.OnReloadStarted.AddListener(HandleReloadStarted);
        ammoManager.OnReloadComplete.RemoveListener(HandleReloadComplete);
        ammoManager.OnReloadComplete.AddListener(HandleReloadComplete);
    }

    private void UnbindReloadEvents()
    {
        if (ammoManager == null)
            return;

        ammoManager.OnReloadStarted.RemoveListener(HandleReloadStarted);
        ammoManager.OnReloadComplete.RemoveListener(HandleReloadComplete);
    }

    private void CacheAnimatorParams()
    {
        hasReloadingParam = false;
        if (animator == null)
            return;

        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool && param.name == "Reloading")
            {
                hasReloadingParam = true;
                break;
            }
        }
    }

    private void CacheReloadSpeed()
    {
        if (animator == null)
            return;

        currentReloadSpeed = animator.GetFloat(AnimSpeedParamHash);
    }

    private void UpdateShellReloadTiming()
    {
        if (ammoManager == null || animator == null)
            return;

        float reloadClipLength = FindReloadClipLength();
        if (reloadClipLength <= 0f)
            return;

        WeaponStats stats = ammoManager.GetCurrentWeaponStats();
        if (stats != null && stats.reloadTime > 0f)
            reloadClipLength = Mathf.Min(reloadClipLength, stats.reloadTime);

        ammoManager.SetShellReloadTimeOverride(reloadClipLength);
    }

    private float FindReloadClipLength()
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null || controller.animationClips == null)
            return -1f;

        float best = -1f;
        foreach (var clip in controller.animationClips)
        {
            if (clip == null)
                continue;

            if (clip.name.IndexOf("reload", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            if (best < 0f || clip.length < best)
                best = clip.length;
        }

        return best;
    }
}
