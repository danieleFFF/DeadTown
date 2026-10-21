using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AmmoManager : MonoBehaviour
{
    [Header("Current Weapon Stats")]
    [SerializeField] private WeaponStats currentWeaponStats;
    
    [Header("Current Ammo State")]
    [SerializeField] private int currentAmmoInMag;
    [SerializeField] private int currentReserveAmmo;
    [SerializeField] private int currentMaxAmmoInMag;

    [Header("Reload Behavior")]
    [SerializeField] private bool reloadsOneByOne = false;
    [SerializeField] private float shellReloadTimeOverride = -1f;
    
    [Header("Events")]
    public UnityEvent<int, int> OnAmmoChanged; // (currentInMag, reserve)
    public UnityEvent OnReloadStarted;
    public UnityEvent OnReloadComplete;
    public UnityEvent OnAmmoEmpty;
    
    private bool isReloading = false;
    private float reloadSpeedMultiplier = 1f;
    private Coroutine reloadRoutine;
    
    private void Start()
    {
        if (currentWeaponStats == null)
            currentWeaponStats = GetComponent<WeaponStats>();
        if (currentWeaponStats == null)
            currentWeaponStats = GetComponentInParent<WeaponStats>();
        if (currentWeaponStats == null)
            currentWeaponStats = FindFirstObjectByType<WeaponStats>();

        if (currentWeaponStats != null)
            InitializeAmmo(currentWeaponStats);   
    }
    
    public void SetWeapon(WeaponStats weaponStats)
    {
        if (weaponStats == null)
            return;

        ResetReloadState(false);
        
        currentWeaponStats = weaponStats;
        InitializeAmmo(weaponStats);
    }
    
    private void InitializeAmmo(WeaponStats stats)
    {
        currentMaxAmmoInMag = Mathf.Max(1, stats.maxAmmoInMag);
        currentAmmoInMag = currentMaxAmmoInMag;
        currentReserveAmmo = 75; // Player always starts with 75 reserve ammo
        isReloading = false;
        reloadRoutine = null;
        
        NotifyAmmoChanged();
        Debug.Log($"Initialized ammo for {stats.weaponName}: {currentAmmoInMag}/{currentReserveAmmo}");
    }
    
    public bool TryUseAmmo()
    {
        if (isReloading) return false;
        
        if (currentWeaponStats != null && currentWeaponStats.infiniteAmmo)
            return true;
        
        if (currentAmmoInMag > 0)
        {
            currentAmmoInMag--;
            NotifyAmmoChanged();
            
            if (currentAmmoInMag == 0)
            {
                OnAmmoEmpty?.Invoke();
            }
            
            return true;
        }
        
        return false;
    }
    
    // Chiamato quando si preme R o auto-reload
    public bool TryReload()
    {
        
        if (isReloading)
            return false;
        
        
        if (currentWeaponStats == null)
            return false;
        
        
        // Infinite ammo - ricarica sempre full
        if (currentWeaponStats.infiniteAmmo)
        {
            StartReload();
            return true;
        }
        
        // Non serve ricaricare se caricatore pieno
        if (currentAmmoInMag >= currentMaxAmmoInMag)
            return false;
        
        // Non c'è munizioni di riserva
        if (currentReserveAmmo <= 0)
            return false;
        
        if (reloadsOneByOne)
            StartShellReload();
        else
            StartReload();
        return true;
    }
    
    private void StartReload()
    {
        isReloading = true;
        OnReloadStarted?.Invoke();
        
        float actualReloadTime = GetReloadTime(); // Speed Cola riduce il tempo
        Invoke(nameof(CompleteReload), actualReloadTime);
        
    }

    private void StartShellReload()
    {
        isReloading = true;
        if (reloadRoutine != null)
            StopCoroutine(reloadRoutine);
        reloadRoutine = StartCoroutine(ReloadShells());
    }

    private IEnumerator ReloadShells()
    {
        while (true)
        {
            if (currentWeaponStats == null)
                break;

            if (!currentWeaponStats.infiniteAmmo)
            {
                if (currentAmmoInMag >= currentMaxAmmoInMag || currentReserveAmmo <= 0)
                    break;
            }

            float actualReloadTime = GetShellReloadTime();
            OnReloadStarted?.Invoke();
            yield return new WaitForSeconds(actualReloadTime);

            if (currentWeaponStats.infiniteAmmo)
            {
                currentAmmoInMag = currentMaxAmmoInMag;
                NotifyAmmoChanged();
                break;
            }

            currentAmmoInMag += 1;
            currentReserveAmmo -= 1;
            NotifyAmmoChanged();
        }

        isReloading = false;
        reloadRoutine = null;
        OnReloadComplete?.Invoke();
    }
    
    private void CompleteReload()
    {
        if (currentWeaponStats == null)
            return;
        
        
        if (currentWeaponStats.infiniteAmmo)
            currentAmmoInMag = currentMaxAmmoInMag;
    
        else
        {
            int ammoNeeded = currentMaxAmmoInMag - currentAmmoInMag;
            int ammoToReload = Mathf.Min(ammoNeeded, currentReserveAmmo);            
            currentAmmoInMag += ammoToReload;
            currentReserveAmmo -= ammoToReload;
        }
        
        isReloading = false;
        OnReloadComplete?.Invoke();
        NotifyAmmoChanged();
    }

    public void CancelReload()
    {
        if (!isReloading)
            return;

        ResetReloadState(true);
    }
    
    public void AddReserveAmmo(int amount)
    {
        if (currentWeaponStats == null) return;
        if (currentWeaponStats.infiniteAmmo) return;
        
        currentReserveAmmo = Mathf.Min(currentReserveAmmo + amount, currentWeaponStats.maxReserveAmmo);
        NotifyAmmoChanged();
    }
    
    /// <summary>
    /// Sets the reserve ammo to a specific amount (used when buying weapons).
    /// Forces the value without capping to maxReserveAmmo.
    /// </summary>
    public void SetReserveAmmo(int amount)
    {
        if (currentWeaponStats == null) return;
        if (currentWeaponStats.infiniteAmmo) return;
        
        // Force set reserve ammo to exact amount (no upper cap)
        currentReserveAmmo = Mathf.Max(0, amount);
        NotifyAmmoChanged();
        Debug.Log($"[AmmoManager] Reserve ammo set to {currentReserveAmmo}");
    }

    public void SetAmmoForWeapon(int ammoInMag, int reserveAmmo)
    {
        if (currentWeaponStats == null)
            currentWeaponStats = GetComponent<WeaponStats>();
        if (currentWeaponStats == null)
            currentWeaponStats = GetComponentInParent<WeaponStats>();
        if (currentWeaponStats == null)
            currentWeaponStats = FindFirstObjectByType<WeaponStats>();

        ResetReloadState(false);

        currentMaxAmmoInMag = Mathf.Max(1, ammoInMag);
        currentAmmoInMag = currentMaxAmmoInMag;
        currentReserveAmmo = Mathf.Max(0, reserveAmmo);
        if (currentWeaponStats != null)
            currentWeaponStats.maxAmmoInMag = currentMaxAmmoInMag;
        NotifyAmmoChanged();
    }
    
    private void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmoInMag, currentReserveAmmo);
    }
    
    public int GetCurrentAmmo() => currentAmmoInMag;
    public int GetReserveAmmo() => currentReserveAmmo;
    public int GetMaxAmmoInMag() => currentMaxAmmoInMag;
    public bool IsReloading() => isReloading;
    public bool HasAmmo() => currentAmmoInMag > 0 || currentReserveAmmo > 0 || (currentWeaponStats != null && currentWeaponStats.infiniteAmmo);
    public WeaponStats GetCurrentWeaponStats() => currentWeaponStats;
    public bool ReloadsOneByOne() => reloadsOneByOne;
    public bool CanInterruptReload() => reloadsOneByOne;
    
    public void SetReloadSpeedMultiplier(float multiplier)
    {
        reloadSpeedMultiplier = Mathf.Max(0.1f, multiplier); 
    }
    
    public void ResetReloadSpeed()
    {
        reloadSpeedMultiplier = 1f;
    }

    public void SetReloadsOneByOne(bool value)
    {
        reloadsOneByOne = value;
    }

    public void SetShellReloadTimeOverride(float seconds)
    {
        shellReloadTimeOverride = seconds > 0f ? seconds : -1f;
    }

    private float GetReloadTime()
    {
        float baseReloadTime = currentWeaponStats != null ? currentWeaponStats.reloadTime : 2f;
        return baseReloadTime / reloadSpeedMultiplier;
    }

    private float GetShellReloadTime()
    {
        float baseReloadTime = currentWeaponStats != null ? currentWeaponStats.reloadTime : 2f;
        if (shellReloadTimeOverride > 0f)
            baseReloadTime = shellReloadTimeOverride;
        return baseReloadTime / reloadSpeedMultiplier;
    }

    private void ResetReloadState(bool notify)
    {
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }

        CancelInvoke(nameof(CompleteReload));

        if (isReloading)
        {
            isReloading = false;
            if (notify)
                OnReloadComplete?.Invoke();
        }
    }
}
