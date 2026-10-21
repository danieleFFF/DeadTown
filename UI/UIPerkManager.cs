using UnityEngine;
using UnityEngine.UI;

public class UIPerkManager : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerCharacter player;

    [Header("Stemma 1 - Speed Cola")]
    [SerializeField] private GameObject stemma1;
    [SerializeField] private Material speedColaDefault;
    [SerializeField] private Material speedColaActive;
    private MeshRenderer stemma1Renderer;
    private Image stemma1Image;

    [Header("Stemma 2 - Juggernog")]
    [SerializeField] private GameObject stemma2;
    [SerializeField] private Material juggernogDefault;
    [SerializeField] private Material juggernogActive;
    private MeshRenderer stemma2Renderer;
    private Image stemma2Image;

    [Header("Stemma 3 - Quick Revive")]
    [SerializeField] private GameObject stemma3;
    [SerializeField] private Material quickReviveDefault;
    [SerializeField] private Material quickReviveActive;
    private MeshRenderer stemma3Renderer;
    private Image stemma3Image;

    private bool lastSpeedColaState = false;
    private bool lastJuggernogState = false;
    private bool lastQuickReviveState = false;

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
            player = FindFirstObjectByType<PlayerCharacter>();

        // Get renderers or images for each stemma
        if (stemma1 != null)
        {
            stemma1Renderer = stemma1.GetComponent<MeshRenderer>();
            stemma1Image = stemma1.GetComponent<Image>();
        }

        if (stemma2 != null)
        {
            stemma2Renderer = stemma2.GetComponent<MeshRenderer>();
            stemma2Image = stemma2.GetComponent<Image>();
        }

        if (stemma3 != null)
        {
            stemma3Renderer = stemma3.GetComponent<MeshRenderer>();
            stemma3Image = stemma3.GetComponent<Image>();
        }

        // Initialize all stemmas to default state
        UpdateStemma1(false);
        UpdateStemma2(false);
        UpdateStemma3(false);
    }

    void Update()
    {
        if (player == null) return;

        // Check Speed Cola status
        bool speedColaActive = player.boughtSpeedColaStatus();
        if (speedColaActive != lastSpeedColaState)
        {
            UpdateStemma1(speedColaActive);
            lastSpeedColaState = speedColaActive;
        }

        // Check Juggernog status
        bool juggernogActive = player.boughtJuggernogStatus();
        if (juggernogActive != lastJuggernogState)
        {
            UpdateStemma2(juggernogActive);
            lastJuggernogState = juggernogActive;
        }

        // Check Quick Revive status
        bool quickReviveActive = player.boughtQuickReviveStatus();
        if (quickReviveActive != lastQuickReviveState)
        {
            UpdateStemma3(quickReviveActive);
            lastQuickReviveState = quickReviveActive;
        }
    }

    private void UpdateStemma1(bool isActive)
    {
        if (stemma1 == null) return;

        Material materialToUse = isActive ? speedColaActive : speedColaDefault;

        // Update MeshRenderer if present
        if (stemma1Renderer != null && materialToUse != null)
        {
            stemma1Renderer.material = materialToUse;
        }

        // Update Image if present (for UI)
        if (stemma1Image != null && materialToUse != null)
        {
            stemma1Image.material = materialToUse;
        }

        Debug.Log($"Stemma 1 (Speed Cola): {(isActive ? "ATTIVO" : "Default")}");
    }

    private void UpdateStemma2(bool isActive)
    {
        if (stemma2 == null) return;

        Material materialToUse = isActive ? juggernogActive : juggernogDefault;

        // Update MeshRenderer if present
        if (stemma2Renderer != null && materialToUse != null)
        {
            stemma2Renderer.material = materialToUse;
        }

        // Update Image if present (for UI)
        if (stemma2Image != null && materialToUse != null)
        {
            stemma2Image.material = materialToUse;
        }

        Debug.Log($"Stemma 2 (Juggernog): {(isActive ? "ATTIVO" : "Default")}");
    }

    private void UpdateStemma3(bool isActive)
    {
        if (stemma3 == null) return;

        Material materialToUse = isActive ? quickReviveActive : quickReviveDefault;

        // Update MeshRenderer if present
        if (stemma3Renderer != null && materialToUse != null)
        {
            stemma3Renderer.material = materialToUse;
        }

        // Update Image if present (for UI)
        if (stemma3Image != null && materialToUse != null)
        {
            stemma3Image.material = materialToUse;
        }

        Debug.Log($"Stemma 3 (Quick Revive): {(isActive ? "ATTIVO" : "Default")}");
    }
}
