using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
namespace ShootingRange
{
    public class ShootingRangeManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int initialTargetCount = 5;
        [SerializeField] private int maxWaves = 10;
        [SerializeField] private int targetsAddedPerWave = 1;
        [Header("Scene References")]
        [SerializeField] private GameObject[] lights;
        [SerializeField] private GameObject[] barriers;
        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private ShootingRangeUI uiManager;
        [Header("Events")]
        public UnityEvent OnGameStart;
        public UnityEvent OnGameEnd;
        public UnityEvent OnWaveComplete;
        private bool isGameActive = false;
        private int currentWave = 0;
        private int totalScore = 0;
        private float gameTimer = 0f;
        private int currentTargetCount = 0;
        private int targetsHitThisWave = 0;
        private void Start()
        {
            SetGameEnvironment(false);
        }

        private void Update(){
            if (isGameActive){
                gameTimer += Time.deltaTime;
                if (uiManager != null)
                    uiManager.UpdateTimer(gameTimer);
            }
        }

        public void StartMinigame()
        {
            if (isGameActive)
            {
                Debug.LogWarning("Game is already active.");
                return;
            }

            Debug.Log("Starting Shooting Range Minigame.");
            
            isGameActive = true;
            currentWave = 0;
            totalScore = 0;
            gameTimer = 0f;
            targetsHitThisWave = 0;
            SetGameEnvironment(true);

            if (uiManager != null)
            {
                uiManager.ShowGameUI(true);
                uiManager.UpdateScore(totalScore);
                uiManager.UpdateWave(currentWave + 1, maxWaves);
            }

            StartNextWave();
            OnGameStart?.Invoke();
        }

        private void StartNextWave()
        {
            currentWave++;
            int requestedTargets = initialTargetCount + ((currentWave - 1) * targetsAddedPerWave);
            targetsHitThisWave = 0;

            if (targetSpawner != null)
                currentTargetCount = targetSpawner.SpawnWave(requestedTargets, currentWave);
            else
                currentTargetCount = requestedTargets;

            Debug.Log($"Starting Wave {currentWave}/{maxWaves} with {currentTargetCount} targets (requested: {requestedTargets})");

            if (uiManager != null)
                uiManager.UpdateWave(currentWave, maxWaves);
        }

        public void OnTargetHit(int points)
        {
            if (!isGameActive) return;

            totalScore += points;
            targetsHitThisWave++;

            Debug.Log($"Target hit! Points: {points}, Total: {totalScore}, Hits: {targetsHitThisWave}/{currentTargetCount}");

            if (uiManager != null)
                uiManager.UpdateScore(totalScore);
            if (targetsHitThisWave >= currentTargetCount)
                OnWaveCompleted();
        }

        private void OnWaveCompleted()
        {
            Debug.Log($"Wave {currentWave} completed!");

            OnWaveComplete?.Invoke();

            if (currentWave >= maxWaves)
                EndMinigame();
            else
                Invoke(nameof(StartNextWave), 1.5f);
        }
        public void EndMinigame()
        {
            if (!isGameActive) return;

            isGameActive = false;

            Debug.Log($"Minigame Complete! Final Score: {totalScore}, Time: {gameTimer:F2}s");

            SetGameEnvironment(false);

            if (targetSpawner != null)
                targetSpawner.HideAllTargets();
            if (uiManager != null)
                uiManager.ShowFinalResults(totalScore, gameTimer);
            OnGameEnd?.Invoke();
        }



        private void SetGameEnvironment(bool active)
        {
            foreach (var light in lights)
                if (light != null)
                    light.SetActive(active);
            foreach (var barrier in barriers)
                if (barrier != null)
                    barrier.SetActive(active);
        }

        public bool IsGameActive() => isGameActive;
        public int GetCurrentWave() => currentWave;
        public int GetTotalScore() => totalScore;
        public float GetGameTime() => gameTimer;
    }
}
