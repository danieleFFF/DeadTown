using UnityEngine;

public class UnlockableZonePhysics : MonoBehaviour
{
    [Header("Cost Settings")]
    public string objectName = "Zone";
    public int objectCost = 500;

    [Header("Physics Settings")]
    public float baseGravity = 1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip unlockClip;

    private bool playerInside;
    private PointsDisplay playerPoints;
    private bool isUnlocked = false;

    void Start()
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!playerInside || isUnlocked) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (playerPoints != null)
            {
                if (playerPoints.GetPoints() >= objectCost)
                {
                    playerPoints.RemovePoints(objectCost);
                    UnlockZone();
                }
                else
                {
                    // Mostra messaggio di punti insufficienti temporaneamente
                    if (InteractionPrompt.Instance != null)
                    {
                        InteractionPrompt.Instance.ShowMessageTemporary($"Insufficient Points! Need {objectCost}", 2f);
                        Invoke(nameof(RestorePromptMessage), 2f);
                    }
                }
            }
        }
    }

    private void RestorePromptMessage()
    {
        if (playerInside && !isUnlocked && InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.ShowMessage($"Press <color=#FFCC00>F</color> to unlock {objectName}: {objectCost} points");
        }
    }

    private void UnlockZone()
    {
        isUnlocked = true;
        
        // Play unlock sound
        if (audioSource && unlockClip)
        {
            float volume = 1f;
            // Try to get SFX volume from settings
            GameObject settings = GameObject.Find("GameSettingsManager");
            if (settings != null)
            {
                var manager = settings.GetComponent<GameSettingsManager>();
                if (manager != null) volume = GameSettingsManager.SFXVolume;
            }
            
            audioSource.PlayOneShot(unlockClip, volume);
        }

        // Nascondi messaggio di interazione
        if (InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.HideMessage();
        }

        SpawnZone[] childSpawnZones = GetComponentsInChildren<SpawnZone>(true);
        foreach (SpawnZone zone in childSpawnZones)
        {
            zone.Unlock();
            Debug.Log($"Unlocked spawn zone: {zone.name}");
        }

        foreach (Transform child in transform)
        {
            Rigidbody rb = child.gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = child.gameObject.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;

            if (child.gameObject.GetComponent<FallingDebris>() == null)
            {
                child.gameObject.AddComponent<FallingDebris>();
            }
        }
        // disable colliders
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetPlayerStatus(true, other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetPlayerStatus(false, other);
        }
    }

    public void SetPlayerStatus(bool isInside, Collider player)
    {
        if (isUnlocked) return;

        if (isInside)
        {
            playerInside = true;
            playerPoints = player.GetComponent<PointsDisplay>();
            
            // Mostra messaggio di interazione
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.ShowMessage($"Press <color=#FFCC00>F</color> to unlock {objectName}: {objectCost} points");
            }
        }
        else
        {
            playerInside = false;
            
            // Nascondi messaggio di interazione
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.HideMessage();
            }
        }
    }
}
