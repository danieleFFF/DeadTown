using UnityEngine;

/// <summary>
/// Gestisce il lancio delle Monkey Bombs dal player
/// </summary>
public class MonkeyBombThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonkeyBombManager monkeyManager;
    [SerializeField] private GameObject monkeyBombPrefab;
    
    [Header("Throw Settings")]
    [Tooltip("Forza del lancio in avanti")]
    [SerializeField] private float throwForce = 10f;
    [Tooltip("Forza del lancio verso l'alto")]
    [SerializeField] private float throwUpwardForce = 3f;
    [Tooltip("Distanza davanti al player per lo spawn")]
    [SerializeField] private float spawnForwardDistance = 1.5f;
    [Tooltip("Offset verso il basso rispetto al centro del player")]
    [SerializeField] private float spawnDownOffset = 0.3f;
    
    [Header("Camera/Direction")]
    [SerializeField] private Transform playerCamera;
    
    void Start()
    {
        // Auto-find MonkeyBombManager
        if (monkeyManager == null)
        {
            monkeyManager = GetComponent<MonkeyBombManager>();
        }
        
        // Auto-find camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
        }
        
        if (monkeyBombPrefab == null)
        {
            Debug.LogError("[MonkeyBombThrower] Monkey Bomb Prefab NOT assigned!");
        }
    }
    
    void Update()
    {
        // Non processare input se il gioco è in pausa (menu Settings aperto)
        if (PauseMenuController.IsPaused)
            return;
            
        // Premi Q per lanciare Monkey Bomb
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryThrowMonkey();
        }
    }
    
    private void TryThrowMonkey()
    {
        // Verifica che abbiamo monkey bombs disponibili
        if (monkeyManager == null || !monkeyManager.TryUseMonkeyBomb())
        {
            Debug.Log("[MonkeyBombThrower] No monkey bombs available!");
            return;
        }
        
        if (monkeyBombPrefab == null)
        {
            Debug.LogError("[MonkeyBombThrower] Cannot throw - prefab is null!");
            return;
        }
        
        // Ottieni direzione di lancio (forward del player o della camera)
        Vector3 throwDirection = GetThrowDirection();
        
        // Calcola posizione di spawn (DAVANTI al player, non sotto)
        // Spawna davanti per evitare collisioni con il player
        Vector3 spawnPosition = transform.position 
            + throwDirection * spawnForwardDistance  // Davanti
            + Vector3.down * spawnDownOffset;        // Leggermente sotto
        
        // Spawna la monkey bomb
        GameObject monkey = Instantiate(monkeyBombPrefab, spawnPosition, Quaternion.identity);
        
        // Ottieni il componente Grenade e imposta la velocità iniziale
        Grenade grenadeScript = monkey.GetComponent<Grenade>();
        if (grenadeScript != null)
        {
            grenadeScript.SetInitialVelocity(throwDirection, throwForce, throwUpwardForce);
            Debug.Log($"[MonkeyBombThrower] Monkey Bomb lanciata! Direction: {throwDirection}");
        }
        else
        {
            // Fallback: applica forza manualmente se non c'è Grenade script
            Rigidbody rb = monkey.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 throwVelocity = throwDirection * throwForce + Vector3.up * throwUpwardForce;
                rb.linearVelocity = throwVelocity;
                Debug.Log("[MonkeyBombThrower] Monkey Bomb lanciata (fallback mode)!");
            }
        }
    }
    
    private Vector3 GetThrowDirection()
    {
        // Usa la direzione della camera se disponibile (First Person)
        if (playerCamera != null)
        {
            // Proietta il forward della camera sul piano orizzontale
            Vector3 cameraForward = playerCamera.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();
            
            return cameraForward;
        }
        
        // Fallback: usa il forward del player
        Vector3 playerForward = transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        
        return playerForward;
    }
}
