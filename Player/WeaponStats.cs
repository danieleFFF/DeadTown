using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "New Weapon";
    public int weaponIndex = 0;
    
    [Header("Ammunition")]
    public int maxAmmoInMag = 30;
    public int maxReserveAmmo = 120;
    public int startingReserveAmmo = 120;
    [Tooltip("Infinite ammo mode - never runs out")]
    public bool infiniteAmmo = false;
    
    public enum FireMode { SemiAuto, FullAuto, Burst }
    
    [Header("Fire Settings")]
    public FireMode fireMode = FireMode.SemiAuto;
    public float fireRate = 0.1f; // Time between shots
    public int damage = 25;
    
    [Header("Spread")]
    public float spreadMinAngleDeg = 0.5f;
    public float spreadMaxAngleDeg = 5.0f;
    
    [Header("ADS Positioning")]
    [Tooltip("Position offset when aiming down sights")]
    public Vector3 adsPosition = new Vector3(0f, -0.1f, 0.2f);
    [Tooltip("Rotation when aiming down sights")]
    public Vector3 adsRotation = Vector3.zero;
    
    [Header("Normal Positioning")]
    [Tooltip("Normal hip-fire position")]
    public Vector3 normalPosition = Vector3.zero;
    [Tooltip("Normal hip-fire rotation")]
    public Vector3 normalRotation = Vector3.zero;
    
    [Header("Animation")]
    public float reloadTime = 2.0f;
    
    [Header("Crosshair")]
    public float crosshairIncreasePerShot = 6f;
    public float crosshairReturnSpeed = 12f;
    public void resetStats()
    {
        weaponIndex = 0;
        maxAmmoInMag = 30;
        maxReserveAmmo = 120;
        startingReserveAmmo = 120;
    }
}
