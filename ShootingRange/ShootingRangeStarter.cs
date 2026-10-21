using UnityEngine;

namespace ShootingRange
{
    /// <summary>
    /// Trigger zone that allows the player to start the shooting range game by pressing F
    /// </summary>
    public class ShootingRangeStarter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShootingRangeManager rangeManager;
        
        [Header("Display Settings")]
        [SerializeField] private string startPrompt = "Press <color=#FFCC00>F</color> to start Shooting Range";
        [SerializeField] private string gameActiveMessage = "Shooting Range is already active";
        [SerializeField] private string noManagerWarning = "No ShootingRangeManager assigned";
        
        private bool playerInside = false;
        
        void Update()
        {
            if (playerInside && Input.GetKeyDown(KeyCode.F))
            {
                TryStartGame();
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            playerInside = true;
            UpdatePromptText();
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = false;
                HidePromptText();
            }
        }
        
        private void TryStartGame()
        {
            if (rangeManager == null)
            {
                Debug.LogWarning(noManagerWarning);
                if (InteractionPrompt.Instance != null)
                {
                    InteractionPrompt.Instance.ShowMessageTemporary(noManagerWarning, 2f);
                }
                return;
            }
            
            if (rangeManager.IsGameActive())
            {
                Debug.Log(gameActiveMessage);
                if (InteractionPrompt.Instance != null)
                {
                    InteractionPrompt.Instance.ShowMessageTemporary(gameActiveMessage, 1.5f);
                    Invoke(nameof(UpdatePromptText), 1.5f);
                }
                return;
            }
            
            // Start the game
            rangeManager.StartMinigame();
            HidePromptText();
            Debug.Log("Player started the shooting range game.");
        }
        
        private void UpdatePromptText()
        {
            if (!playerInside || InteractionPrompt.Instance == null) return;
            
            if (rangeManager != null && rangeManager.IsGameActive())
            {
                InteractionPrompt.Instance.ShowMessage(gameActiveMessage);
            }
            else
            {
                InteractionPrompt.Instance.ShowMessage(startPrompt);
            }
        }
        
        private void HidePromptText()
        {
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.HideMessage();
            }
        }
        
        // Optional: Auto-hide messages after delay
        private void OnDisable()
        {
            CancelInvoke();
        }
    }
}
