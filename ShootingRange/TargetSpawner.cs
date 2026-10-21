using UnityEngine;
using System.Collections.Generic;

namespace ShootingRange
{
    public enum WeaponRangePreference
    { Close, Medium, Long }

    public class TargetSpawner : MonoBehaviour
    {
        [Header("Target Setup")]
        [SerializeField] private SoldierTarget targetPrefab; 
        [SerializeField] private Transform[] spawnPoints; 
        [Header("Wave Settings")]
        [SerializeField] private int maxTargetsPerWave = 5;
        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 15; 
        [SerializeField] private Transform poolParent;
        [Header("Distance Reference")]
        [SerializeField] private Transform distanceReferencePoint;
        private List<SoldierTarget> targetPool = new List<SoldierTarget>();
        private List<SoldierTarget> activeTargets = new List<SoldierTarget>();
        private void Start()
        {
            InitializePool();
            
            if (distanceReferencePoint == null)
            {
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    distanceReferencePoint = player.transform;
                }
            }
        }

        private void InitializePool()
        {
            if (targetPrefab == null)
            {
                Debug.LogError("Target prefab is not assigned");
                return;
            }

            if (poolParent == null)
            {
                GameObject poolObj = new GameObject("TargetPool");
                poolParent = poolObj.transform;
                poolParent.SetParent(transform);
            }

            for (int i = 0; i < poolSize; i++)
            {
                CreateNewTarget();
            }

            Debug.Log($"Target pool initialized with {poolSize} targets");
        }

        private SoldierTarget CreateNewTarget()
        {
            SoldierTarget newTarget = Instantiate(targetPrefab, poolParent);
            newTarget.gameObject.SetActive(false);
            targetPool.Add(newTarget);
            return newTarget;
        }

        private SoldierTarget GetTargetFromPool()
        {
            foreach (var target in targetPool)
            {
                if (!target.gameObject.activeInHierarchy)
                {
                    return target;
                }
            }

            Debug.LogWarning("Pool exhausted, creating new target");
            return CreateNewTarget();
        }

        public int SpawnWave(int targetCount, int waveNumber){
            return SpawnWave(targetCount, waveNumber, WeaponRangePreference.Medium);
        }

        public int SpawnWave(int targetCount, int waveNumber, WeaponRangePreference weaponPreference) {
            HideAllTargets();

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("No spawn points assigned!");
                return 0;
            }

            int actualTargetCount = Mathf.Min(targetCount, maxTargetsPerWave, spawnPoints.Length);
            List<Transform> selectedSpawnPoints = SelectRandomSpawnPoints(actualTargetCount, weaponPreference);

            foreach (var spawnPoint in selectedSpawnPoints)  {
                SpawnTargetAt(spawnPoint, waveNumber);
            }

            Debug.Log($"Spawned {selectedSpawnPoints.Count} targets for wave {waveNumber} (max: {maxTargetsPerWave})");
            
            return selectedSpawnPoints.Count;
        }

        private List<Transform> SelectRandomSpawnPoints(int count, WeaponRangePreference preference)
        {
            List<Transform> availablePoints = new List<Transform>(spawnPoints);
            List<Transform> selectedPoints = new List<Transform>();
            
            if (distanceReferencePoint != null && preference != WeaponRangePreference.Medium) {
                availablePoints.Sort((a, b) =>
                {
                    float distA = Vector3.Distance(distanceReferencePoint.position, a.position);
                    float distB = Vector3.Distance(distanceReferencePoint.position, b.position);
                    return distA.CompareTo(distB);
                });

                if (preference == WeaponRangePreference.Close)
                {
                    int halfCount = Mathf.Max(count, availablePoints.Count / 2);
                    availablePoints = availablePoints.GetRange(0, Mathf.Min(halfCount, availablePoints.Count));
                }
                else if (preference == WeaponRangePreference.Long)
                {
                    int startIndex = Mathf.Max(0, availablePoints.Count / 2);
                    availablePoints = availablePoints.GetRange(startIndex, availablePoints.Count - startIndex);
                }
            }

            for (int i = 0; i < count && availablePoints.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availablePoints.Count);
                selectedPoints.Add(availablePoints[randomIndex]);
                availablePoints.RemoveAt(randomIndex);
            }

            return selectedPoints;
        }

        private void SpawnTargetAt(Transform spawnPoint, int waveNumber)
        {
            if (spawnPoint == null) return;

            SoldierTarget target = GetTargetFromPool();
            target.ResetTarget();
            target.transform.position = spawnPoint.position;
            target.transform.rotation = spawnPoint.rotation * Quaternion.Euler(270f, 0f, 0f);
            target.gameObject.SetActive(true);
            
            if (!activeTargets.Contains(target)) {
                activeTargets.Add(target);
            }

        }

        public void HideAllTargets()
        {
            foreach (var target in activeTargets)
            {
                if (target != null)
                {
                    target.gameObject.SetActive(false);
                }
            }
            activeTargets.Clear();
        }

        public int GetActiveTargetCount()
        {
            return activeTargets.Count;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
                    UnityEditor.Handles.Label(spawnPoints[i].position + Vector3.up, $"Spawn {i + 1}");
                }
            }
        }
#endif
    }
}
