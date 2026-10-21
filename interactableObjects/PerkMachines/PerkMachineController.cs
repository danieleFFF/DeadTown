using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PerkMachineController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI perkText;
    [Header("Perk Index")]
    [Tooltip("0 = Juggernog, 1 = Speed Cola, 2 = Quick Revive")]
    public int perkIndex;
    [Header("Settings")]
    public int cost = 2500; // Inspector controllable cost

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip delayedSound;

    private List<PerkMachine> perkMachines;
    private PerkMachine perkMachine;
    private bool playerInside;
    private PointsDisplay playerPoints;
    private PlayerCharacter playerCharacter;
    
    void Awake()
    {
        // Setup text outline safely (try-catch to avoid internal TMP NREs)
        if (perkText != null)
        {
            try {
                perkText.outlineWidth = 0.15f;
                perkText.outlineColor = Color.black;
            } catch (System.Exception) { /* If shader not ready, ignore */ }
        }

        // Auto-get AudioSource
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        switch (perkIndex)
        {
            case 0:
                perkMachine = new JuggernogPerkMachine();
                break;
            case 1:
                perkMachine = new SpeedColaPerkMachine();
                break;
            case 2:
                perkMachine = new QuickRevivePerkMachine();
                break;
            default:
                Debug.LogError("Perk Index not valid.");
                break;
        }
    }

    void Update()
    {
        if (!playerInside || playerCharacter == null || perkMachine == null) return;
        
        // Don't show if already purchased
        if(playerCharacter.hasPurchasedPerk(perkMachine.PerkName))
        {
             // Handled in OnTriggerEnter but good to double check
             return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            // Use local 'cost' variable
            if (playerPoints != null && playerPoints.GetPoints() >= cost)
            {
                playerPoints.RemovePoints(cost);
                perkMachine.Purchase(playerCharacter);
                perkText.gameObject.SetActive(false);
                
                // Play Sound Sequence
                StartCoroutine(PlayPurchaseSequence());
            }
            else
            {
                perkText.text = "Insufficient points!";
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInside = true;
        playerPoints = other.GetComponent<PointsDisplay>();
        playerCharacter = other.GetComponent<PlayerCharacter>();
        
        // Wait for player components
        if(playerCharacter == null || playerPoints == null || perkMachine == null) return;

        perkText.gameObject.SetActive(true);

        if (playerCharacter.hasPurchasedPerk(perkMachine.PerkName))
        {
            perkText.text = $"{perkMachine.PerkName} already purchased.";
        }
        else
        {
            if (perkText != null)
            {
                if(perkText.outlineWidth < 0.1f) // Re-apply if missed in Awake
                {
                     try {
                        perkText.outlineWidth = 0.15f;
                        perkText.outlineColor = Color.black;
                        if (perkText.fontSharedMaterial != null) perkText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
                        perkText.UpdateMeshPadding(); 
                     } catch(System.Exception){}
                }
                
                perkText.text = $"Press <color=#FFCC00>F</color> to buy {perkMachine.PerkName} ({cost})";
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        perkText.gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator PlayPurchaseSequence()
    {
        float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;

        if (audioSource && purchaseSound)
        {
            audioSource.PlayOneShot(purchaseSound, vol);
            // Wait for clip length
            yield return new WaitForSeconds(purchaseSound.length);
        }

        // Wait additional 1 second
        yield return new WaitForSeconds(0.3f);

        if (audioSource && delayedSound)
        {
             audioSource.PlayOneShot(delayedSound, vol);
        }
    }
}
