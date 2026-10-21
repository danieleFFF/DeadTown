using UnityEngine;
using System.Collections;

public class MysteryBox : MonoBehaviour
{
    [Header("Cost Settings")]
    [SerializeField] private int useCost = 950;
    
    [Header("Box Parts")]
    [SerializeField] private Transform boxLid;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Light boxLight;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip revealSound;
    [SerializeField] private AudioClip closeSound;
    
    [Header("Weapon Pool")]
    [SerializeField] private GameObject[] weaponPrefabs; // Array di armi possibili
    [Tooltip("Indici delle armi nel WeaponManager corrispondenti ai prefab sopra (stesso ordine)")]
    [SerializeField] private int[] weaponIndices; // Mappa prefab -> weaponIndex per WeaponManager
    [Tooltip("Prefab della Monkey Bomb (opzionale - se presente, può essere estratto random)")]
    [SerializeField] private GameObject monkeyBombPrefab; // Monkey Bomb speciale
    [Range(0f, 1f)]
    [Tooltip("Probabilità di ottenere una Monkey Bomb invece di un'arma (0 = mai, 1 = sempre)")]
    [SerializeField] private float monkeyBombChance = 0.15f; // 15% di probabilità
    
    [Header("Animation Settings")]
    [SerializeField] private float lidOpenDuration = 0.5f;
    [SerializeField] private float lidOpenAngle = 100f;
    [SerializeField] private float weaponRiseDuration = 2f;
    [SerializeField] private float weaponTargetHeight = 3.7f;
    [SerializeField] private float weaponDisplayDuration = 3f;
    [SerializeField] private float weaponRotationSpeed = 90f; // gradi/secondo
    
    private bool playerInZone = false;
    private bool isActive = false;
    private PointsDisplay playerPoints; // Riferimento temporaneo mentre player è nel trigger
    private PointsDisplay activePlayer; // Riferimento al player che ha attivato la box (persiste durante sequenza)
    private GameObject currentWeapon;
    private Quaternion lidClosedRotation;
    private Vector3 lidOpenRotation;
    private bool isCurrentItemMonkey = false; // Track se l'item corrente è una Monkey Bomb
    private int currentWeaponIndex = -1; // Track dell'indice arma corrente per WeaponManager

    private void Start()
    {
        if (boxLid != null)
        {
            lidClosedRotation = boxLid.localRotation;
            lidOpenRotation = new Vector3(lidOpenAngle, 0f, 0f);
        }
        
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        // Auto-find AudioSource se non assegnato
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }
        }
    }

    private void Update()
    {
        if (playerInZone && !isActive && Input.GetKeyDown(KeyCode.F))
        {
            TryUseMysteryBox();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            playerPoints = other.GetComponent<PointsDisplay>();
            
            if (!isActive && InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.ShowMessage($"Press <color=#FFCC00>F</color> to use Mystery Box: {useCost} points");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            playerPoints = null;
            
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.HideMessage();
            }
        }
    }

    private void TryUseMysteryBox()
    {
        if (playerPoints == null) return;
        

        if (playerPoints.GetPoints() < useCost)
        {
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.ShowMessageTemporary($"Insufficient Points! Need {useCost}", 2f);
                Invoke(nameof(RestorePromptMessage), 2f);
            }
            return;
        }

        playerPoints.RemovePoints(useCost);
        
        // Salva il riferimento al player per tutta la sequenza
        activePlayer = playerPoints;
        
        if (InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.HideMessage();
        }
        StartCoroutine(MysteryBoxSequence());
    }

    private void RestorePromptMessage()
    {
        if (playerInZone && !isActive && InteractionPrompt.Instance != null)
        {
            InteractionPrompt.Instance.ShowMessage($"Press F to use Mystery Box: {useCost} points");
        }
    }

    private IEnumerator MysteryBoxSequence()
    {
        PlaySound(revealSound);
        isActive = true;
        yield return StartCoroutine(OpenLid());
        
        GameObject weapon = SpawnRandomWeapon();
        yield return StartCoroutine(RaiseWeapon(weapon));
        
        // Display Weapon
        yield return StartCoroutine(DisplayWeapon(weapon));
        
        // Give weapon and start closing
        GiveWeaponToPlayer(weapon);
        
        // Start closing lid
        StartCoroutine(CloseLid());
        
        // CLOSE SOUND (1 second AFTER closing starts)
        yield return new WaitForSeconds(0.07f);
        PlaySound(closeSound);
        
        // Ensure closing is visually finished (lidOpenDuration is 0.5, so 1s wait covers it)
        // Just small buffer to be safe
        yield return new WaitForSeconds(0.5f);
        
        isActive = false;
        
        if (playerInZone && InteractionPrompt.Instance != null)
            InteractionPrompt.Instance.ShowMessage($"Press F to use Mystery Box: {useCost} points");
        
    }

    private IEnumerator OpenLid()
    {
        if (boxLid == null)
        {
            Debug.LogWarning("[MysteryBox] Box lid non assegnato!");
            yield break;
        }

        Quaternion startRotation = boxLid.localRotation;
        Quaternion endRotation = Quaternion.Euler(lidOpenRotation);
        float elapsedTime = 0f;

        // Suono apertura (immediato)
        PlaySound(openSound);

        while (elapsedTime < lidOpenDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lidOpenDuration;
            
            // Ease-out
            t = 1f - (1f - t) * (1f - t);
            
            boxLid.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        boxLid.localRotation = endRotation;
        
        // Attiva la luce quando il coperchio è aperto
        if (boxLight != null)
            boxLight.enabled = true;
    }

    private IEnumerator CloseLid()
    {
        if (boxLid == null) yield break;

        Quaternion startRotation = boxLid.localRotation;
        Quaternion endRotation = lidClosedRotation;
        float elapsedTime = 0f;

        // PlaySound(closeSound); // Moved to Sequence for timing

        while (elapsedTime < lidOpenDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lidOpenDuration;
            
            // Ease-in
            t = t * t;
            
            boxLid.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        boxLid.localRotation = endRotation;
        
        // Disattiva la luce quando il coperchio è chiuso
        if (boxLight != null)
            boxLight.enabled = false;
    }

    private GameObject SpawnRandomWeapon()
    {
        GameObject weaponPrefab = null;
        isCurrentItemMonkey = false;
        currentWeaponIndex = -1;
        
        // Controlla se abbiamo il prefab monkey bomb e tira random
        if (monkeyBombPrefab != null && Random.value < monkeyBombChance)
        {
            // Spawna Monkey Bomb!
            weaponPrefab = monkeyBombPrefab;
            isCurrentItemMonkey = true;
            Debug.Log("[MysteryBox] Monkey Bomb selezionata!");
        }
        else
        {
            // Spawna arma normale
            if (weaponPrefabs == null || weaponPrefabs.Length == 0)
            {
                Debug.LogError("[MysteryBox] Nessuna arma nel pool!");
                return null;
            }
            
            int randomIndex = Random.Range(0, weaponPrefabs.Length);
            weaponPrefab = weaponPrefabs[randomIndex];
            
            // Salva l'indice dell'arma per WeaponManager
            if (weaponIndices != null && randomIndex < weaponIndices.Length)
            {
                currentWeaponIndex = weaponIndices[randomIndex];
            }
            else
            {
                Debug.LogWarning($"[MysteryBox] weaponIndices non configurato per indice {randomIndex}. Impossibile equipaggiare l'arma.");
            }
            
            Debug.Log($"[MysteryBox] Spawned weapon: {weaponPrefab.name}, weaponIndex: {currentWeaponIndex}");
        }

        // Spawna alla posizione del spawn point
        Vector3 spawnPosition = spawnPoint.position;
        GameObject weapon = Instantiate(weaponPrefab, spawnPosition, Quaternion.identity);
        
        currentWeapon = weapon;
        
        return weapon;
    }

    private IEnumerator RaiseWeapon(GameObject weapon)
    {
        if (weapon == null) yield break;

        Vector3 startPosition = weapon.transform.position;
        
        // Calcola posizione target in coordinate locali rispetto al MysteryBox
        Vector3 localTargetPosition = new Vector3(spawnPoint.localPosition.x, weaponTargetHeight, spawnPoint.localPosition.z);
        Vector3 worldTargetPosition = transform.TransformPoint(localTargetPosition);
        
        float elapsedTime = 0f;

        while (elapsedTime < weaponRiseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / weaponRiseDuration;
            
            // Ease-out
            t = 1f - (1f - t) * (1f - t);
            
            if (weapon != null)
                weapon.transform.position = Vector3.Lerp(startPosition, worldTargetPosition, t);
            yield return null;
        }

        if (weapon != null)
            weapon.transform.position = worldTargetPosition;
    }

    private IEnumerator DisplayWeapon(GameObject weapon)
    {
        if (weapon == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < weaponDisplayDuration)
        {
            elapsedTime += Time.deltaTime;

            if (weapon != null)
                weapon.transform.Rotate(Vector3.up, weaponRotationSpeed * Time.deltaTime, Space.World);
            else
                yield break;
            
            yield return null;
        }
    }

    private void GiveWeaponToPlayer(GameObject weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("[MysteryBox] GiveWeaponToPlayer: weapon is NULL!");
            return;
        }

        Debug.Log($"[MysteryBox] GiveWeaponToPlayer called - isCurrentItemMonkey: {isCurrentItemMonkey}, weapon: {weapon.name}");

        // Gestione speciale per Monkey Bomb
        if (isCurrentItemMonkey)
        {
            Debug.Log("[MysteryBox] Detected Monkey Bomb - attempting to add to player");
            
            // Usa activePlayer invece di playerPoints (persiste durante la sequenza)
            if (activePlayer != null)
            {
                Debug.Log($"[MysteryBox] activePlayer found: {activePlayer.gameObject.name}");
                
                MonkeyBombManager monkeyManager = activePlayer.GetComponent<MonkeyBombManager>();
                if (monkeyManager != null)
                {
                    Debug.Log("[MysteryBox] MonkeyBombManager found! Adding monkey bomb...");
                    monkeyManager.AddMonkeyBomb();
                    Debug.Log("[MysteryBox] Monkey Bomb aggiunta al player!");
                }
                else
                {
                    Debug.LogError("[MysteryBox] MonkeyBombManager NON TROVATO sul player!");
                    Debug.LogError($"[MysteryBox] Player GameObject: {activePlayer.gameObject.name}");
                    Debug.LogError("[MysteryBox] Assicurati che MonkeyBombManager sia attachato al player!");
                }
            }
            else
            {
                Debug.LogError("[MysteryBox] activePlayer is NULL! This should not happen!");
            }
            
            // Distruggi il visual della Monkey Bomb
            Destroy(weapon);
            currentWeapon = null;
            isCurrentItemMonkey = false;
        }
        else
        {
            // Equipaggia l'arma al player usando WeaponManager (come WeaponSignBuy ma senza acquisto)
            if (activePlayer != null && currentWeaponIndex >= 0)
            {
                WeaponManager playerWeapon = activePlayer.GetComponentInChildren<WeaponManager>();
                if (playerWeapon == null)
                    playerWeapon = activePlayer.GetComponentInParent<WeaponManager>();
                if (playerWeapon == null && activePlayer.transform.parent != null)
                    playerWeapon = activePlayer.transform.parent.GetComponentInChildren<WeaponManager>();
                
                if (playerWeapon != null)
                {
                    playerWeapon.SwitchWeapon(currentWeaponIndex);
                    Debug.Log($"[MysteryBox] Arma equipaggiata al player: {weapon.name}, weaponIndex: {currentWeaponIndex}");
                }
                else
                {
                    Debug.LogError("[MysteryBox] WeaponManager NON TROVATO sul player!");
                }
            }
            else
            {
                if (activePlayer == null)
                    Debug.LogError("[MysteryBox] activePlayer is NULL!");
                if (currentWeaponIndex < 0)
                    Debug.LogError("[MysteryBox] currentWeaponIndex non valido! Configura weaponIndices nell'Inspector.");
            }
            
            // Distruggi l'arma visualizzata (il player ha ora l'arma equipaggiata tramite WeaponManager)
            Destroy(weapon);
            currentWeapon = null;
        }
        
        // Pulisci il riferimento activePlayer alla fine della sequenza
        activePlayer = null;
    }

    private void OnDestroy()
    {
        // Cleanup
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.PlayOneShot(clip, vol);
        }
    }
}
