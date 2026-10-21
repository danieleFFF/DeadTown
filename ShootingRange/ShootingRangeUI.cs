using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShootingRange
{
    public class ShootingRangeUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject gameUIPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject startButton;
        [Header("Game UI Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI waveText;
        [Header("Results UI Elements")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI finalTimeText;
        [SerializeField] private TextMeshProUGUI resultsMessageText;
        private void Start()
        {
            ShowGameUI(false);
            ShowResults(false);
            
            if (startButton != null)
            {
                startButton.SetActive(true);
            }
        }

        public void ShowGameUI(bool show)
        {
            if (gameUIPanel != null)
            {
                resultsPanel.SetActive(!show);
                gameUIPanel.SetActive(show);
            }

            if (startButton != null)
            {
                startButton.SetActive(!show);
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        public void UpdateTimer(float time)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
                timerText.text = $"Time: {minutes:00}:{seconds:00}:{milliseconds:00}";
            }
        }

        public void UpdateWave(int currentWave, int maxWaves)
        {
            if (waveText != null) {
                waveText.text = $"Wave: {currentWave}/{maxWaves}";
            }
        }

        public void ShowFinalResults(int finalScore, float finalTime)
        {
            ShowGameUI(false);
            ShowResults(true);

            if (finalScoreText != null) {
                finalScoreText.text = $"Final Score: {finalScore}";
            }

            if (finalTimeText != null)
            {
                int minutes = Mathf.FloorToInt(finalTime / 60f);
                int seconds = Mathf.FloorToInt(finalTime % 60f);
                finalTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            if (resultsMessageText != null)
            {
                string message = GetPerformanceMessage(finalScore, finalTime);
                resultsMessageText.text = message;
            }
        }

        private void ShowResults(bool show)
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(show);
            }
        }

        private string GetPerformanceMessage(int score, float time)
        {
            if (score >= 100 && time < 60f)
            {
                return "EXCELLENT!";
            }
            else if (score >= 75 && time < 90f)
            {
                return "GREAT! ";
            }
            else if (score >= 50)
            {
                return "GOOD!";
            }
            else
            {
                return "Keep trying!";
            }
        }

        public void OnRestartButtonClicked()
        {
            ShowResults(false);
            
            if (startButton != null)
            {
                startButton.SetActive(true);
            }
        }
    }
}
