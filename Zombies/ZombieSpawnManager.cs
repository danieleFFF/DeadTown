using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

//Manages zombie spawning with round-based waves.
public class ZombieSpawnManager : MonoBehaviour {
    [Header("Zombie Prefab")]
    [SerializeField] private GameObject zombiePrefab;
    [Header("Round Settings")]
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int maxRounds = 3;
    [SerializeField] private int baseZombiesPerRound = 6;
    [SerializeField] private float zombiesPerRoundMultiplier = 0.2f; //+20% per round
    [SerializeField] private int maxZombiesAlive = 24;
    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 3f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float timeBetweenRounds = 10f;
    [Header("Spawn Zones")]
    [SerializeField] private List<SpawnZone> spawnZones = new List<SpawnZone>();
    [SerializeField] private bool autoFindSpawnZones = true;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI roundText;
    [Header("Custom Round Settings")]
    [Tooltip("Specific zombie counts for each round (i0 = Round 1). If round > count, use formula.")]
    [SerializeField] private List<int> zombiesPerRoundConfig = new List<int> { 12, 15, 20 };
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip roundChangeClip;
    private int zombiesToSpawn;
    private int zombiesSpawnedThisRound;
    private int zombiesAlive;
    private int totalZombiesKilled;
    private bool roundInProgress = false;
    private bool isSpawning = false;
    private float timeSurvived = 0f;
    private bool gameComplete = false;
    private static ZombieSpawnManager instance; //Singleton 
    public static ZombieSpawnManager Instance => instance;
    private List<SpawnZone> cachedUnlockedZones = new List<SpawnZone>();//Cached lists and objects
    private GUIStyle debugStyle;
    public int CurrentRound => currentRound;
    public int ZombiesAlive => zombiesAlive;
    public int TotalKills => totalZombiesKilled;
    public float TimeSurvived => timeSurvived;
    void Update() {
        if (!gameComplete)
            timeSurvived += Time.deltaTime;
    }
    
    void Awake() {
        instance = this;
    }
    
    void Start() {
        if (autoFindSpawnZones) FindAllSpawnZones();
        if (roundText == null) CreateRoundTextUI();
        
        StartCoroutine(StartFirstRound());
    }
    
    void CreateRoundTextUI(){
        //Canvas
        GameObject canvasObj = new GameObject("RoundTimerCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        //Round text
        GameObject textObj = new GameObject("RoundText");
        textObj.transform.SetParent(canvasObj.transform, false);
        roundText = textObj.AddComponent<TextMeshProUGUI>();
        roundText.text = "";
        roundText.fontSize = 48;
        roundText.alignment = TextAlignmentOptions.Center;
        roundText.color = Color.white;
        roundText.outlineWidth = 0.2f;
        roundText.outlineColor = Color.black;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0, -20);
        textRect.sizeDelta = new Vector2(600, 80);
        Debug.Log("[ZombieSpawnManager] Created round timer ui");
    }
    void FindAllSpawnZones() {
        spawnZones.Clear();
        SpawnZone[] zones = FindObjectsOfType<SpawnZone>();
        spawnZones.AddRange(zones);
        #if UNITY_EDITOR
        Debug.Log($"[SpawnManager] Found {spawnZones.Count} spawn zones");
        #endif
    }
    
    IEnumerator StartFirstRound() {
        yield return new WaitForSeconds(initialSpawnDelay);

        StartRound();
    }
    
    void StartRound() {
        roundInProgress = true;
        
        //Plays round sound
        if (audioSource && roundChangeClip) {
            float volume = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.PlayOneShot(roundChangeClip, volume);
        }
        
        if (roundText != null) roundText.text = $"Round: {currentRound}";
        
        UnlockZonesForRound(currentRound);
        zombiesToSpawn = CalculateZombiesForRound(currentRound);
        zombiesSpawnedThisRound = 0;
        
        #if UNITY_EDITOR
        Debug.Log($"Round {currentRound} Start - Zombies: {zombiesToSpawn}");
        #endif
        
        GameOverScreen.SetRound(currentRound);
        StartCoroutine(SpawnRoutine());
    }
    
    int CalculateZombiesForRound(int round) {
        //Uses custom config if available
        int roundIndex = round - 1; //0-based index
        
        if (roundIndex >= 0 && roundIndex < zombiesPerRoundConfig.Count)
            return zombiesPerRoundConfig[roundIndex];

        //Formula: base + (round * multiplier * base)
        float zombies = baseZombiesPerRound + (round * zombiesPerRoundMultiplier * baseZombiesPerRound);
        
        return Mathf.RoundToInt(zombies);
    }
    
    IEnumerator SpawnRoutine() {
        isSpawning = true;
        
        while (zombiesSpawnedThisRound < zombiesToSpawn) {
            //Waits if max zombies alive
            while (zombiesAlive >= maxZombiesAlive){
                yield return new WaitForSeconds(1f);
            }
            
            if (TrySpawnZombie()) {
                zombiesSpawnedThisRound++;
                zombiesAlive++;
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        
        isSpawning = false;
        CheckRoundState();
    }
    
    //Avoids code duplication and race conditions
    void CheckRoundState() {
        if (zombiesAlive <= 0 && zombiesSpawnedThisRound >= zombiesToSpawn && !isSpawning) {
            //checks that we haven't already started the end round sequence
            if (roundInProgress) StartCoroutine(EndRound());
        }
    }
    
    bool TrySpawnZombie() {
        //Gets available unlocked spawn zones
        List<SpawnZone> availableZones = GetUnlockedZones();
        
        if (availableZones.Count == 0) {
            #if UNITY_EDITOR
            Debug.LogWarning("[SpawnManager] No unlocked spawn zones available");
            #endif

            return false;
        }
        
        SpawnZone chosenZone = ChooseWeightedZone(availableZones);
        Vector3 spawnPos = chosenZone.GetRandomSpawnPosition();
        
        if (spawnPos == Vector3.zero) {
            #if UNITY_EDITOR
            Debug.LogWarning($"[SpawnManager] Zone '{chosenZone.name}' couldn't find valid spawn position");
            #endif

            return false;
        }
        SpawnZombieAt(spawnPos);

        return true;
    }
    
    //Reuses cached list instead of allocating new one
    List<SpawnZone> GetUnlockedZones() {
        cachedUnlockedZones.Clear();
        
        foreach (SpawnZone zone in spawnZones) {
            if (zone != null && zone.isUnlocked && zone.gameObject.activeInHierarchy) {
                cachedUnlockedZones.Add(zone);
            }
        }
        return cachedUnlockedZones;
    }
    
    SpawnZone ChooseWeightedZone(List<SpawnZone> zones) {
        //Zones that match current round get higher weight
        int totalWeight = 0;
        int[] adjustedWeights = new int[zones.Count];
        
        for (int i = 0; i < zones.Count; i++) {
            var zone = zones[i];
            int weight = zone.spawnWeight;
            
            if (zone.unlockOnRound == currentRound) weight *= 4;
            else if (zone.unlockOnRound == currentRound - 1) weight *= 2;
            
            adjustedWeights[i] = weight;
            totalWeight += weight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        for (int i = 0; i < zones.Count; i++) {
            currentWeight += adjustedWeights[i];
            
            if (randomValue < currentWeight) return zones[i];
        }
        return zones[0];
    }
    
    void SpawnZombieAt(Vector3 position) {
        if (zombiePrefab == null) {
            Debug.LogError("[SpawnManager] Zombie prefab not assigned");

            return;
        }
        
        GameObject zombie = Instantiate(zombiePrefab, position, Quaternion.identity);
        zombie.tag = "Enemy";
        
        //Registers for death callback
        ZombieDeathNotifier notifier = zombie.GetComponent<ZombieDeathNotifier>();

        if (notifier == null) notifier = zombie.AddComponent<ZombieDeathNotifier>();

        notifier.onDeath = OnZombieDeath;
        
        #if UNITY_EDITOR
        if (showDebugInfo) Debug.Log($"[SpawnManager] Spawned zombie at {position} (alive: {zombiesAlive + 1})");
        #endif
    }

    //Called when a zombie dies
    public void OnZombieDeath() {
        zombiesAlive = Mathf.Max(0, zombiesAlive - 1);
        totalZombiesKilled++;
        
        //Awards points for killing a zombie
        PointsDisplay pointsDisplay = FindFirstObjectByType<PointsDisplay>();
        if (pointsDisplay != null) pointsDisplay.AddPoints(50);
        
        GameOverScreen.AddKill();
        
        #if UNITY_EDITOR
        if (showDebugInfo) Debug.Log($"[SpawnManager] Zombie killed. Alive: {zombiesAlive}, total kills: {totalZombiesKilled}");
        #endif
        
        //Checks if round is complete
        CheckRoundState();
    }
    
    IEnumerator EndRound() {
        roundInProgress = false;
        #if UNITY_EDITOR
        Debug.Log($"Round {currentRound} complete.");
        #endif
        
        if (currentRound >= maxRounds) {
            HandleVictory();

            yield break;
        }
        
        //Countdown timer between rounds
        float timeRemaining = timeBetweenRounds;

        while (timeRemaining > 0) {
            if (roundText != null) roundText.text = $"Next round in: {Mathf.CeilToInt(timeRemaining)}";

            yield return null;
            
            timeRemaining -= Time.deltaTime;
        }
        currentRound++;
        StartRound();
    }
    
    void HandleVictory() {
        gameComplete = true;
        #if UNITY_EDITOR
        Debug.Log($"Victory. All {maxRounds} rounds completed");
        #endif
        
        VictoryScreen.ShowVictory(currentRound, totalZombiesKilled, timeSurvived);
    }
    
    //Registers a new spawn zone
    public void RegisterSpawnZone(SpawnZone zone) {
        if (!spawnZones.Contains(zone)) {
            spawnZones.Add(zone);
            #if UNITY_EDITOR
            Debug.Log($"[SpawnManager] Registered new spawn zone: {zone.name}");
            #endif
        }
    }
    
    public static void UnlockZone(SpawnZone zone) {
        if (zone != null) zone.Unlock();
    }
    
    //Unlocks all zones that are scheduled for this round (or earlier).
    void UnlockZonesForRound(int round) {
        int unlockedCount = 0;

        foreach (var zone in spawnZones) {
            if (zone != null && !zone.isUnlocked && zone.unlockOnRound <= round) {
                zone.Unlock();
                unlockedCount++;
            }
        }
        
        if (unlockedCount > 0)
            #if UNITY_EDITOR
            Debug.Log($"[SpawnManager] Unlocked {unlockedCount} new spawn zones for round {round}");
            #endif
    }
    
    //Caches GUIStyle to avoid allocation every frame
    void OnGUI() {
        if (!showDebugInfo) return;
        
        if (debugStyle == null) {
            debugStyle = new GUIStyle();
            debugStyle.fontSize = 20;
            debugStyle.normal.textColor = Color.white;
        }
        
        string info = $"Round: {currentRound}\n" + $"Zombies Alive: {zombiesAlive}\n" + $"Spawned: {zombiesSpawnedThisRound}/{zombiesToSpawn}\n" + $"Total Kills: {totalZombiesKilled}";
        GUI.Label(new Rect(10, 10, 300, 100), info, debugStyle);
    }
}

//Notifies spawn manager when zombie dies
public class ZombieDeathNotifier : MonoBehaviour {
    public System.Action onDeath;
    private bool notified = false;
    
    public void NotifyDeath() {
        if (notified) return;

        notified = true;
        onDeath?.Invoke();
    }
}
