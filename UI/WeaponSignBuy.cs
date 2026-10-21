using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponSignBuy : MonoBehaviour {
    public int cost = 1500;
    public TextMeshProUGUI weaponText;
    public int weaponIndex;
    private bool playerInside = false;
    private PointsDisplay playerPoints;
    private WeaponManager playerWeapon;
    private GameObject playerObj;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip purchaseClip;
    
    void Start()
    {
        if (weaponText != null)
        {
            weaponText.outlineWidth = 0.15f;
            weaponText.outlineColor = Color.black;
        }
    }
    
    void Update() {
        if (playerInside && Input.GetKeyDown(KeyCode.F))
        {
            // Block if player has this weapon equipped AND ammo is full
            if (playerWeapon != null && playerWeapon.GetCurrentWeaponIndex() == weaponIndex)
            {
                // Find AmmoManager and WeaponStats
                AmmoManager ammoManager = playerObj.GetComponent<AmmoManager>();
                if (ammoManager == null) ammoManager = playerObj.GetComponentInChildren<AmmoManager>();
                
                WeaponStats currentStats = ammoManager != null ? ammoManager.GetCurrentWeaponStats() : null;
                
                if (ammoManager != null && currentStats != null)
                {
                    // Check only RESERVE ammo as requested
                    int currentReserve = ammoManager.GetReserveAmmo();
                    int maxReserve = currentStats.maxReserveAmmo;
                    
                    if (currentReserve >= maxReserve)
                    {
                        weaponText.text = "Ammo is already full!";
                        return;
                    }
                }
                // If we fall through here, it means we have the weapon but ammo is NOT full, so we proceed to buy refill
            }
            
            if (playerPoints.GetPoints() >= cost)
            {
                playerPoints.RemovePoints(cost);
                
                // If we don't have the weapon, this switches to it. 
                // If we DO have it, this just ensures it's active.
                playerWeapon.SwitchWeapon(weaponIndex);
                
                // Refill ammo if we already had the weapon
                StartCoroutine(RefillAmmoRoutine());
                
                //Purchase sound
                if (audioSource && purchaseClip) {
                    float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
                    audioSource.PlayOneShot(purchaseClip, vol);
                }

                weaponText.text = "You already have this weapon!";
                Debug.Log("Weapon/Ammo bought.");
            } else {
                weaponText.text = "Insufficient points.";
            }
        }
    }

    private IEnumerator RefillAmmoRoutine()
    {
        // Wait a frame to ensure weapon stats are active
        yield return null;
        
        AmmoManager ammoManager = null;
        if (playerObj != null)
        {
            ammoManager = playerObj.GetComponent<AmmoManager>();
            if (ammoManager == null) ammoManager = playerObj.GetComponentInChildren<AmmoManager>();
        }

        if (ammoManager != null)
        {
            WeaponStats currentStats = ammoManager.GetCurrentWeaponStats();
            if (currentStats != null)
            {
                // Fill reserve to max
                ammoManager.SetReserveAmmo(currentStats.maxReserveAmmo);
                Debug.Log($"Refilled reserve ammo to {currentStats.maxReserveAmmo}");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        playerObj = other.gameObject;
        
        // Try to find on the collider object first, then parent
        playerPoints = other.GetComponent<PointsDisplay>();
        if (playerPoints == null)
            playerPoints = other.GetComponentInParent<PointsDisplay>();
            
        playerWeapon = other.GetComponentInChildren<WeaponManager>();
        if (playerWeapon == null)
            playerWeapon = other.GetComponentInParent<WeaponManager>();
        if (playerWeapon == null && other.transform.parent != null)
            playerWeapon = other.transform.parent.GetComponentInChildren<WeaponManager>();

        if (playerPoints == null || playerWeapon == null) {
            Debug.LogWarning("PlayerPoints or WeaponManager not found on Player");
            return;
        }
        
        // Enhance visibility
        weaponText.outlineWidth = 0.15f;
        weaponText.outlineColor = Color.black;
        weaponText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        weaponText.UpdateMeshPadding();
        
        weaponText.text = $"Press <color=#FFCC00>F</color> to buy weapon: ({cost})";
        weaponText.gameObject.SetActive(true);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            weaponText.gameObject.SetActive(false);
        }
    }
}
