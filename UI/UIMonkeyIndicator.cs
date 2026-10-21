using UnityEngine;
using UnityEngine.UI;

public class UIMonkeyIndicator : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private MonkeyBombManager monkeyManager;
    
    [Header("UI Element")]
    [SerializeField] private GameObject monkeyIndicator;
    
    [Header("Materials")]
    [SerializeField] private Material material_locked;   // Quando count = 0
    [SerializeField] private Material material_unlocked; // Quando count > 0
    
    [Header("Counter Text (Optional)")]
    [Tooltip("TextMeshProUGUI che mostra il count (es. 'x3'). Opzionale.")]
    [SerializeField] private TMPro.TextMeshProUGUI counterText;
    
    private MeshRenderer meshRenderer;
    private Image imageComponent;
    
    void Start()
    {
        // Auto-find MonkeyBombManager se non assegnato
        if (monkeyManager == null)
        {
            monkeyManager = FindFirstObjectByType<MonkeyBombManager>();
            
            if (monkeyManager == null)
            {
                Debug.LogError("[UIMonkeyIndicator] MonkeyBombManager not found!");
                return;
            }
        }
        
        // Ottieni il renderer o l'image component
        if (monkeyIndicator != null)
        {
            meshRenderer = monkeyIndicator.GetComponent<MeshRenderer>();
            imageComponent = monkeyIndicator.GetComponent<Image>();
        }
        
        // Sottoscrivi all'evento di cambio count
        if (monkeyManager != null)
        {
            monkeyManager.OnMonkeyCountChanged.AddListener(UpdateIndicator);
            
            // Aggiorna immediatamente con il count corrente
            UpdateIndicator(monkeyManager.GetMonkeyCount());
        }
    }
    
    void OnDestroy()
    {
        // Rimuovi il listener quando l'oggetto viene distrutto
        if (monkeyManager != null)
        {
            monkeyManager.OnMonkeyCountChanged.RemoveListener(UpdateIndicator);
        }
    }
    
    /// <summary>
    /// Aggiorna il material in base al count
    /// </summary>
    private void UpdateIndicator(int count)
    {
        if (monkeyIndicator == null)
        {
            Debug.LogWarning("[UIMonkeyIndicator] Monkey indicator object is null!");
            return;
        }
        
        // Determina quale material usare
        Material targetMaterial = count > 0 ? material_unlocked : material_locked;
        
        if (targetMaterial == null)
        {
            Debug.LogWarning($"[UIMonkeyIndicator] Material is null! (locked: {material_locked}, unlocked: {material_unlocked})");
            return;
        }
        
        // Applica al MeshRenderer (per oggetti 3D)
        if (meshRenderer != null)
        {
            meshRenderer.material = targetMaterial;
        }
        
        // Applica all'Image (per UI Canvas)
        if (imageComponent != null)
        {
            imageComponent.material = targetMaterial;
        }
        
        // Aggiorna il counter text se presente
        if (counterText != null)
        {
            counterText.text = $"x{count}";
        }
        
        Debug.Log($"[UIMonkeyIndicator] Updated - Count: {count}, Material: {(count > 0 ? "UNLOCKED" : "LOCKED")}");
    }
}
