using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefab  ;
    [SerializeField] private WeaponStats playerWeaponStats;
    [SerializeField] private int[] magSizeByWeaponIndex = { 30, 4, 50, 30, 40, 30, 35, 12, 8, 6 };
    [SerializeField] private int startWeaponIndex = 6;
    public AmmoManager ammoManager;
    //The weapon with 60dmg is the shotgun which has a single bullet reloading animation, so it needs to be handled differently.
    private int [] dmgAmount = {000,60,5,15,8,15,10,80,90,100};
    private const int ShotgunDamage = 60;
    private const float SpeedColaReloadMultiplier = 2f;
    private int currentWeaponDmg =-1;
    private int currentWeaponIndex = -1;
    private PlayerCharacter playerCharacter;
    
    void Start()
    {
        if (ammoManager == null)
            ammoManager = FindFirstObjectByType<AmmoManager>();
        playerCharacter = GetComponentInParent<PlayerCharacter>();
        ResolvePlayerWeaponStats();
        
        SwitchWeapon(startWeaponIndex);
    }

    void Update()
    {
        
    }
    

    public void SwitchWeapon(int weaponIndex)
    {
        SwitchWeapon(weaponIndex, -1); // Default: don't override reserve ammo
    }
    
    public void SwitchWeapon(int weaponIndex, int setReserveAmmo)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponPrefab.Length)
            return;

        if (transform.childCount > 0){
            for(int i = 0; i < transform.childCount; i++){
                Destroy(transform.GetChild(i).gameObject);
            }
        }
            

        GameObject newWeapon = Instantiate(weaponPrefab[weaponIndex], transform);
     if (ammoManager == null)
            ammoManager = FindFirstObjectByType<AmmoManager>();

        currentWeaponIndex = weaponIndex;
        RefillMagForCurrentWeapon();

        if (playerWeaponStats == null)
            Debug.LogWarning("[WeaponManager] WeaponStats not found on player. Reload may not work.");

        currentWeaponDmg = dmgAmount[weaponIndex];
        if (ammoManager != null)
            ammoManager.SetReloadsOneByOne(currentWeaponDmg == ShotgunDamage);

        WeaponAnimator weaponAnimator = newWeapon != null
            ? newWeapon.GetComponentInChildren<WeaponAnimator>()
            : null;
        ApplySpeedColaIfActive(weaponAnimator);
    }

    public int getCurrentWeaponDmg(){
        return currentWeaponDmg;
    }
    
    public int GetCurrentWeaponIndex(){
        return currentWeaponIndex;
    }

    public void RefillMagForCurrentWeapon()
    {
        if (currentWeaponIndex < 0)
            return;

        int magSize = GetMagSizeForIndex(currentWeaponIndex);

        ResolvePlayerWeaponStats();
        if (playerWeaponStats != null)
            playerWeaponStats.maxAmmoInMag = magSize;

        if (ammoManager != null)
        {
            // Reserve ammo is always 75 for every weapon.
            ammoManager.SetAmmoForWeapon(magSize, 75);
        }
    }

    private WeaponStats ResolvePlayerWeaponStats()
    {
        if (playerWeaponStats != null)
            return playerWeaponStats;

        playerWeaponStats = GetComponentInParent<WeaponStats>();
        if (playerWeaponStats == null && ammoManager != null)
            playerWeaponStats = ammoManager.GetComponentInParent<WeaponStats>();
        if (playerWeaponStats == null)
            playerWeaponStats = FindFirstObjectByType<WeaponStats>();

        return playerWeaponStats;
    }

    private int GetMagSizeForIndex(int weaponIndex)
    {
        if (magSizeByWeaponIndex != null && weaponIndex >= 0 && weaponIndex < magSizeByWeaponIndex.Length)
            return Mathf.Max(1, magSizeByWeaponIndex[weaponIndex]);

        return 30;
    }

    private void ApplySpeedColaIfActive(WeaponAnimator weaponAnimator)
    {
        if (playerCharacter == null)
            playerCharacter = GetComponentInParent<PlayerCharacter>();

        if (playerCharacter != null)
            playerCharacter.RefreshWeaponAnimator();

        if (playerCharacter != null && playerCharacter.boughtSpeedColaStatus())
        {
            if (ammoManager == null)
                ammoManager = FindFirstObjectByType<AmmoManager>();

            if (ammoManager != null)
                ammoManager.SetReloadSpeedMultiplier(SpeedColaReloadMultiplier);

            if (weaponAnimator != null)
                weaponAnimator.speedUpReload(SpeedColaReloadMultiplier);
        }
    }

}
