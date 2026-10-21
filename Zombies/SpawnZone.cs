using UnityEngine;
using UnityEngine.AI;

//Defines a spawn zone area using box colliders. Zombies can spawn at random positions within these
[RequireComponent(typeof(BoxCollider))]
public class SpawnZone : MonoBehaviour {
    [Header("Zone Settings")]
    [Tooltip("Is this zone available for spawning?")]
    public bool isUnlocked = false;
    [Tooltip("Round number when this zone becomes active")]
    public int unlockOnRound = 1;
    [Tooltip("Priority weight for this zone (higher = more likely to spawn zombies)")]
    [Range(1, 10)]
    public int spawnWeight = 1;
    [Header("Spawn Validation")]
    [Tooltip("Max distance to search for valid NavMesh position")]
    public float navMeshSearchRadius = 0.2f; //It's small to ensure spawn stays inside zone
    [Tooltip("Minimum distance from player to allow spawning")]
    public float minDistanceFromPlayer = 4f;
    [Tooltip("Maximum distance from player (0 = no limit)")]
    public float maxDistanceFromPlayer = 50f;
    [Header("Visibility Check")]
    [Tooltip("Don't spawn if player can see the spawn point")]
    public bool checkPlayerVisibility = false;
    [Tooltip("How close to screen edge counts as 'visible' (0.1 = 10% margin)")]
    [Range(0f, 0.3f)]
    public float screenEdgeMargin = 0.1f;
    private BoxCollider boxCollider;
    private Transform playerTransform;
    private Camera playerCamera;
    
    void Awake() {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        
        //Finds player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null) playerTransform = player.transform;
        
        playerCamera = Camera.main;
    }
    
    //Gets a random valid spawn position within this zone (like behind a wall)
    public Vector3 GetRandomSpawnPosition(int maxAttempts = 10) {
        if (!isUnlocked) return Vector3.zero;
        
        for (int i = 0; i < maxAttempts; i++) {
            Vector3 randomPos = GetRandomPointInBox();
            
            //Validates on NavMesh
            NavMeshHit hit;
            
            if (NavMesh.SamplePosition(randomPos, out hit, navMeshSearchRadius, NavMesh.AllAreas)) {
                Vector3 validPos = hit.position;
                
                //Checks distance from player
                if (playerTransform != null) {
                    float distToPlayer = Vector3.Distance(validPos, playerTransform.position);
                    
                    if (distToPlayer < minDistanceFromPlayer) continue; //Too close
                    if (maxDistanceFromPlayer > 0 && distToPlayer > maxDistanceFromPlayer) continue; //Too far
                }
                
                //Checks visibility
                if (checkPlayerVisibility && IsVisibleToPlayer(validPos)) continue; //Player can see this point
                
                return validPos;
            }
        }
        return Vector3.zero; //No valid position found
    }
    
    //Gets a random point inside the box collider bounds
    private Vector3 GetRandomPointInBox() {
        Bounds bounds = boxCollider.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = bounds.center.y;
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        
        return new Vector3(randomX, randomY, randomZ);
    }
    
    //Checks if a position is visible to the player
    private bool IsVisibleToPlayer(Vector3 position) {
        if (playerCamera == null) return false;
        
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(position);
        
        //Checks if in viewport
        bool inViewport = viewportPoint.x > screenEdgeMargin && viewportPoint.x < (1f - screenEdgeMargin) && //Not too left/right
                          viewportPoint.y > screenEdgeMargin && viewportPoint.y < (1f - screenEdgeMargin) && //Not too at the top/bottom
                          viewportPoint.z > 0; //Is in front of the camera
        
        if (!inViewport) return false;
        
        //Raycasts to check if something blocks the view
        Vector3 dirToPoint = position - playerCamera.transform.position;
        float distance = dirToPoint.magnitude;
        
        if (Physics.Raycast(playerCamera.transform.position, dirToPoint.normalized, out RaycastHit hit, distance)){
            //Something is blocking (position is not directly visible)
            if (hit.point != position)
                return false;
        }
        
        return true; //Position is visible
    }
    
    public void Unlock() {
        isUnlocked = true;
        Debug.Log($"SpawnZone '{gameObject.name}' unlocked!");
    }
    
    //Checks if this zone has any valid spawn positions available.
    public bool HasValidSpawnPosition() {
        return isUnlocked && GetRandomSpawnPosition(3) != Vector3.zero;
    }
    
    //Visualization of spawn areas in editor
    void OnDrawGizmos() {
        BoxCollider col = GetComponent<BoxCollider>();

        if (col == null) return;
        
        Gizmos.color = isUnlocked ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.center, col.size);
        Gizmos.color = isUnlocked ? Color.green : Color.red;
        Gizmos.DrawWireCube(col.center, col.size);

        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2, $"Round {unlockOnRound}");
        #endif
    }
    
    void OnDrawGizmosSelected() {
        if (playerTransform == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
        
        if (maxDistanceFromPlayer > 0) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        }
    }
}
