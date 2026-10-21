using UnityEngine;
using TMPro;


public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Settings")]
    [SerializeField] private float defaultDisplayDuration = 2f;

    private bool isShowingTemporaryMessage = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("[InteractionPrompt] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        if (promptText == null)
            promptText = GetComponentInChildren<TextMeshProUGUI>();

        if (promptText != null)
        {
            // Apply outline
            promptText.outlineWidth = 0.15f;
            promptText.outlineColor = Color.black;
            promptText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message)
    {
        if (promptText == null)
        {
            Debug.LogWarning("[InteractionPrompt] No TextMeshProUGUI assigned!");
            return;
        }

        CancelInvoke(nameof(HideMessage)); 
        isShowingTemporaryMessage = false;
        
        // Enhance visibility
        promptText.outlineWidth = 0.15f;
        promptText.outlineColor = Color.black;
        // Ensure shader keywords are enabled for outline
        if (promptText.fontSharedMaterial != null)
        {
            promptText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
        }
        promptText.UpdateMeshPadding();

        promptText.text = message;
        promptText.gameObject.SetActive(true);
    }

    public void ShowMessageTemporary(string message, float duration = -1f)
    {
        if (duration < 0) duration = defaultDisplayDuration;

        ShowMessage(message);
        isShowingTemporaryMessage = true;
        Invoke(nameof(HideMessage), duration);
    }


    public void HideMessage()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
        isShowingTemporaryMessage = false;
    }


    public bool IsShowingMessage()
    {
        return promptText != null && promptText.gameObject.activeSelf;
    }


    public bool IsTemporaryMessage()
    {
        return isShowingTemporaryMessage;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
