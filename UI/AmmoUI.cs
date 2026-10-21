using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI ammoText;
    
    [Header("AmmoManager")]
    [SerializeField] private AmmoManager ammoManager;
    
    [Header("Display Format")]
    [SerializeField] private string displayFormat = "{0} / {1}"; // "30 / 120"
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField] private int lowAmmoThreshold = 10;
    
    private void Start()
    {
        // Auto-find AmmoManager se non assegnato
        if (ammoManager == null)
        {
            ammoManager = FindFirstObjectByType<AmmoManager>();
        }
        
        if (ammoManager == null)
        {
            Debug.LogWarning("AmmoManager not found! AmmoUI will not work.");
            return;
        }
        
        if (ammoText == null)
        {
            Debug.LogWarning("AmmoText not assigned! Please assign TextMeshProUGUI in Inspector.");
            return;
        }
        
        // Sottoscrivi agli eventi
        ammoManager.OnAmmoChanged.AddListener(UpdateAmmoDisplay);
        
        // Visualizza stato iniziale
        UpdateAmmoDisplay(ammoManager.GetCurrentAmmo(), ammoManager.GetReserveAmmo());
    }
    
    private void OnDestroy()
    {
        // Rimuovi listener per evitare memory leak
        if (ammoManager != null)
        {
            ammoManager.OnAmmoChanged.RemoveListener(UpdateAmmoDisplay);
        }
    }
    
    private void UpdateAmmoDisplay(int currentAmmo, int reserveAmmo)
    {
        if (ammoText == null) return;
        
        // Formatta testo
        ammoText.text = string.Format(displayFormat, currentAmmo, reserveAmmo);
        
        // Cambia colore in base alle munizioni
        if (currentAmmo == 0)
        {
            ammoText.color = emptyColor;
        }
        else if (currentAmmo <= lowAmmoThreshold)
        {
            ammoText.color = lowAmmoColor;
        }
        else
        {
            ammoText.color = normalColor;
        }
    }
    
    // Metodo pubblico per aggiornare manualmente (se necessario)
    public void RefreshDisplay()
    {
        if (ammoManager != null)
        {
            UpdateAmmoDisplay(ammoManager.GetCurrentAmmo(), ammoManager.GetReserveAmmo());
        }
    }
}
