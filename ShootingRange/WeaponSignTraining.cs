using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSignTraining : MonoBehaviour
{
    [Header("Training Settings")]
    public TextMeshProUGUI weaponText;
    public int weaponIndex;
    
    [Header("Display Settings")]
    public string weaponDisplayName = "Weapon";
    
    private bool playerInside = false;
    private WeaponManager playerWeapon;
    
    void Start()
    {
        if (weaponText != null)
        {
            weaponText.outlineWidth = 0.15f;
            weaponText.outlineColor = Color.black;
        }
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.F))
        {
            if (playerWeapon != null)
            {
                playerWeapon.SwitchWeapon(weaponIndex);
                weaponText.text = $"{weaponDisplayName} equipped!";
                Debug.Log($"Training weapon {weaponDisplayName} equipped.");
                
                // Hide the message after a short delay
                Invoke(nameof(HideMessage), 1.5f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        playerWeapon = other.GetComponentInChildren<WeaponManager>(); 

        if (playerWeapon == null)
            return;

        // Enhance visibility
        weaponText.outlineWidth = 0.15f;
        weaponText.outlineColor = Color.black;
        if (weaponText.fontSharedMaterial != null) weaponText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        weaponText.UpdateMeshPadding();
        
        weaponText.text = $"Press <color=#FFCC00>F</color> to use {weaponDisplayName}";
        weaponText.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            weaponText.gameObject.SetActive(false);
            CancelInvoke(nameof(HideMessage));
        }
    }
    
    private void HideMessage()
    {
        if (!playerInside)
        {
            weaponText.gameObject.SetActive(false);
        }
        else
        {
            // Show the original prompt again if player is still inside
            weaponText.text = $"Press <color=#FFCC00>F</color> to use {weaponDisplayName} (No points needed)";
        }
    }
}
