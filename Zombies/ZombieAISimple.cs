using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAISimple : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 1.3f;
    [SerializeField] private float runSpeed = 2.5f;
    [Header("Distances")]
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float runDistance = 10f;
    [SerializeField] private float attackDistance = 2f;
    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 1;
    [Header("Target Object (Priority over Player)")]
    [SerializeField] private string targetObjectTag = "ZombieTarget";
    [SerializeField] private bool checkForTargetObject = true;
    private Transform targetObject;
    [Header("Death Settings")]
    [SerializeField] private string deadZombieLayer = "DeadZombie";
    [Header("References")]
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;
    private Transform player;
    private PlayerCharacter playerChar;
    private int zombieHealth = 100;
    private bool zombieIsDead = false;
    private float lastAttackTime = -999f;
    private float distanceToPlayer = 999f;
    private bool loggedMissingNavMesh = false;
    private bool isProvoked = false;
    private float provokedEndTime = 0f;
    private float targetSearchInterval = 0.5f;
    private float nextTargetSearchTime = 0f;
    private float nextPlayerSearchTime = 0f;
    private float nextAmbientSoundTime = 0f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] ambientSounds;
    public AudioClip[] attackSounds;
    [Range(0.1f, 3f)]
    [SerializeField] private float volumeMultiplier = 1.0f; //Boosts volume above 1
    private int cachedDeadLayer = -1;
    private int speedHash;
    private int attackHash;

    private void Awake() {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        speedHash = Animator.StringToHash("Speed");
        attackHash = Animator.StringToHash("Attack");
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0.5f; //Gets very close to player
        agent.updatePosition = true;
        agent.updateRotation = true;
        
        if(rb != null) rb.isKinematic = true; 
    }

    private void Start() {
        agent.enabled = true;

        //Zombie doesn't spawn in the correct way
        if (!agent.isOnNavMesh) {
            NavMeshHit hit;
            
            //Searches in a larger radius (10f)
            if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas)) {
                bool warped = agent.Warp(hit.position);
                #if UNITY_EDITOR

                if(warped) Debug.Log($"[Zombie] Successfully warped to: {hit.position}");
                else  Debug.LogError("[Zombie] Warp failed even with valid point"); 
                #endif
            }
            #if UNITY_EDITOR
            else Debug.LogError("[Zombie] No NavMesh found within 10 meters");
            #endif
        }
        
        //Finds player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            player = playerObj.transform;
            playerChar = playerObj.GetComponent<PlayerCharacter>();
        }
    }
    
    private void Update() {
        if (zombieIsDead) return;

        //Checks if stuck (after delay)
        if (Time.time > 1f && !agent.isOnNavMesh) {
             if(!loggedMissingNavMesh) {
                #if UNITY_EDITOR
                Debug.LogError("Zombie lost Navmesh!"); 
                #endif
                loggedMissingNavMesh = true;
                
                //Try to re warp once
                NavMeshHit hit;
                
                if (NavMesh.SamplePosition(transform.position, out hit, 3.0f, NavMesh.AllAreas))
                     agent.Warp(hit.position);
             }
             return; 
        }
        
        //Searches for player every n seconds instead of every frame
        if (player == null && Time.time >= nextPlayerSearchTime) {
            nextPlayerSearchTime = Time.time + targetSearchInterval;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            
            if (playerObj != null) {
                player = playerObj.transform;
                playerChar = playerObj.GetComponent<PlayerCharacter>();
            }
        }
        
        //Sounds
        if (Time.time >= nextAmbientSoundTime) {
            float delay = 7f + Random.Range(-2f, 2f);
            nextAmbientSoundTime = Time.time + delay;
            PlayAmbientSound();
        }

        bool playerAlive = player != null && (playerChar == null || !playerChar.IsDead);
        
        //Updates target (object/player)
        if (checkForTargetObject) UpdateTargetObject();
        
        Transform currentTarget = targetObject != null ? targetObject : player;
        bool hasValidTarget = (targetObject != null) || playerAlive;

        if (hasValidTarget && currentTarget != null) {
            distanceToPlayer = Vector3.Distance(transform.position, currentTarget.position);
            
            //Uses horizontal distance for detection and attack (ignores height difference)
            Vector3 horizontalDiff = currentTarget.position - transform.position;
            horizontalDiff.y = 0;
            float horizontalDistance = horizontalDiff.magnitude;
            
            //Should verify provoked expiration
            if (isProvoked && Time.time > provokedEndTime) isProvoked = false;

            //Checks vertical and horizontal distance 
            float verticalDistance = Mathf.Abs(currentTarget.position.y - transform.position.y);
            if (horizontalDistance <= attackDistance && verticalDistance < 2.5f) Attack();
            else ChaseTarget(currentTarget);
        }
        else {
            //If no target, stop
            agent.isStopped = true;

            if (animator != null) animator.SetFloat(speedHash, 0);
        }
        UpdateAnimations();
    }

    private float pathUpdateInterval = 0.2f;
    private float pathUpdateTimer = 0f;
    private Vector3 lastTargetPos = Vector3.zero;

    private void SafeSetDestination(Vector3 targetPos) {
        if (agent.isOnNavMesh && agent.isActiveAndEnabled) {
            //Tries direct set
            bool pathFound = agent.SetDestination(targetPos);
            
            //If failed, it could mean that the target is slightly off navmesh
            if (!pathFound) {
                NavMeshHit hit;
                
                //Looks for a valid point near the target (5 meters)
                if (NavMesh.SamplePosition(targetPos, out hit, 5.0f, NavMesh.AllAreas))
                    pathFound = agent.SetDestination(hit.position);
            }
        }
    }

    private void ChaseTarget(Transform target) {
        if (target == null) return;
        
        agent.isStopped = false;
        float distToLastPos = Vector3.Distance(target.position, lastTargetPos);
        bool targetMoved = distToLastPos > 1.5f; 
        bool nearEndOfPath = agent.remainingDistance < 2.0f && !agent.pathPending;

        if (Time.time >= pathUpdateTimer && (targetMoved || nearEndOfPath)) {
            SafeSetDestination(target.position);
            lastTargetPos = target.position;
            pathUpdateTimer = Time.time + pathUpdateInterval;
        }
        
        //Walks or runs based on distance
        bool shouldRun = distanceToPlayer <= runDistance;
        agent.speed = shouldRun ? runSpeed : walkSpeed;
    }

    private void Attack() {
        agent.isStopped = false;
        agent.speed = walkSpeed * 0.3f; //Moves slowly while attacking
        
        //Keeps approaching the player
        if(player != null) {
            SafeSetDestination(player.position);
            
            //Rotates towards target
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            if(direction != Vector3.zero) 
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }

        if (Time.time >= lastAttackTime + attackCooldown) {
            #if UNITY_EDITOR
            Debug.Log($"[Zombie] ATTACKING player!");
            #endif

            //Plays random attack sound
            if (audioSource && attackSounds != null && attackSounds.Length > 0) {
                AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
                float vol = (GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f) * volumeMultiplier;
                audioSource.PlayOneShot(clip, vol);
            }

            if (animator != null) {
                animator.SetTrigger(attackHash);
            }

            lastAttackTime = Time.time;
            Invoke(nameof(DealDamage), 0.3f); //Delays damage sync with animation
        }
    }

    private void UpdateAnimations() {
        if (animator == null) return;
        
        //Gets velocity from agent
        float speed = agent.velocity.magnitude;
        animator.SetFloat(speedHash, speed);
    }

    private void DealDamage() {
        if (zombieIsDead || player == null) return;
        
        //Uses horizontal distance for damage check
        Vector3 horizontalDiff = player.position - transform.position;
        horizontalDiff.y = 0;
        float horizontalDist = horizontalDiff.magnitude;
        float verticalDist = Mathf.Abs(player.position.y - transform.position.y);

        //Checks both horizontal (range) and vertical (height)
        if (horizontalDist > attackDistance * 1.5f || verticalDist > 2.5f) return;

        //Uses only PlayerCharacter for damage (single health system)
        if(playerChar != null)  {
            playerChar.Hit(attackDamage);
            #if UNITY_EDITOR
            Debug.Log($"[Zombie] Hit player for {attackDamage} damage");
            #endif
        }
        #if UNITY_EDITOR
        else Debug.LogError("[Zombie] PlayerCharacter component is null, can't deal damage");
        #endif
    }

    public void TakeDamage(int damage) {
        if (zombieIsDead) return;
        
        zombieHealth -= damage;
        
        if (player != null) {
            isProvoked = true;
            provokedEndTime = Time.time + 10f; //Chases for 10 seconds regardless of distance
            
            if (agent.isOnNavMesh) {
                SafeSetDestination(player.position);
                agent.speed = runSpeed; //Zombie starts running
            }
        }
        
        if (zombieHealth <= 0) Die();
    }
    
    private void Die() {
        zombieIsDead = true;
        agent.enabled = false;
        if(rb != null) rb.isKinematic = true;
        
        //Notifies SpawnManager
        ZombieDeathNotifier notifier = GetComponent<ZombieDeathNotifier>();
        
        if (notifier != null) notifier.NotifyDeath();
        
        ChangeToDeadZombieLayer();
        
        if (animator != null) animator.SetTrigger("Die");
        
        Destroy(gameObject, 3f);
    }

    private void ChangeToDeadZombieLayer() {
        //Caches the layer lookup
        if (cachedDeadLayer == -1) cachedDeadLayer = LayerMask.NameToLayer(deadZombieLayer);
        if (cachedDeadLayer == -1) return;
        
        gameObject.layer = cachedDeadLayer;
        //Uses Transform hierarchy instead of GetComponentsInChildren
        SetLayerRecursive(transform, cachedDeadLayer);
    }
    
    private void SetLayerRecursive(Transform t, int layer) {
        t.gameObject.layer = layer;
        
        for (int i = 0; i < t.childCount; i++){
            SetLayerRecursive(t.GetChild(i), layer);
        }
    }
    
    private void UpdateTargetObject() {
        if (Time.time < nextTargetSearchTime) return;

        nextTargetSearchTime = Time.time + targetSearchInterval;
        
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetObjectTag);
        
        if (targetObj != null && targetObj.activeSelf) targetObject = targetObj.transform;
        else targetObject = null;
    }
    
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, runDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

    }
    
    private void PlayAmbientSound() {
        if (audioSource && ambientSounds != null && ambientSounds.Length > 0) {
            //random sound
            AudioClip clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
            float volume = (GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f) * volumeMultiplier;
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
