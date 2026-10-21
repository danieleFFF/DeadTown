using UnityEngine;
using UnityEngine.UI;

namespace ShootingRange
{
    public class StartGameButton : MonoBehaviour
    {
        [SerializeField] private ShootingRangeManager gameManager;
        private Button button;

        private void Start()
        {
            button = GetComponent<Button>();
            
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<ShootingRangeManager>();
            }
        }

        private void OnButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.StartMinigame();
            }
            else
            {
                Debug.LogError("ShootingRangeManager not found");
            }
        }
    }
}
