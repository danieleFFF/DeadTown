using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Settings")]
    [Tooltip("Nome della scena da caricare come menu di pausa")]
    public string settingsSceneName = "Settings";
    

    public static bool IsPaused { get; private set; } = false;
    
    private bool isLoadingSettings = false;
    
    private FPSInput fpsInput;
    private MouseLook[] mouseLooks;
    
    void Start()
    {
        IsPaused = false;
        fpsInput = GetComponent<FPSInput>();
        mouseLooks = GetComponentsInChildren<MouseLook>();
    }
    
    void Update()
    {
        if (LoadingScreen.IsLoading)
            return;
        
        if (GameOverScreen.IsGameOver)
            return;
            
        if (Time.timeScale == 0f && !IsPaused)
            return;
        
        if (Input.GetKeyDown(KeyCode.Escape) && !isLoadingSettings)
        {
            if (!IsPaused)
                PauseGame();
            else
                ResumeGame();
        }
    }
    

    public void PauseGame()
    {
        if (IsPaused) return;
        
        isLoadingSettings = true;
        IsPaused = true;
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SetPlayerControlsEnabled(false);
        
        var asyncOp = SceneManager.LoadSceneAsync(settingsSceneName, LoadSceneMode.Additive);
        if (asyncOp != null)
        {
            asyncOp.completed += OnSettingsLoaded;
        }
        else
        {
            Debug.LogError($"[PauseMenuController] Impossibile caricare la scena '{settingsSceneName}'. " +
                          "Assicurati che sia aggiunta nelle Build Settings!");
            isLoadingSettings = false;
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }
    
    void OnSettingsLoaded(AsyncOperation op)
    {
        isLoadingSettings = false;
        Debug.Log("[PauseMenuController] Menu Settings caricato");
    }
    
    public void ResumeGame()
    {
        if (!IsPaused) return;
        
        IsPaused = false;
        
        var scene = SceneManager.GetSceneByName(settingsSceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(settingsSceneName);
        }
        
        Time.timeScale = 1f;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        SetPlayerControlsEnabled(true);
        
        Debug.Log("[PauseMenuController] Gioco ripreso");
    }
    

    void SetPlayerControlsEnabled(bool enabled)
    {
        if (fpsInput != null) 
            fpsInput.enabled = enabled;
            
        if (mouseLooks != null)
        {
            foreach (var ml in mouseLooks)
            {
                if (ml != null) ml.enabled = enabled;
            }
        }
    }

    public static void RequestResume()
    {
        var controller = FindObjectOfType<PauseMenuController>();
        if (controller != null)
        {
            controller.ResumeGame();
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] Controller non trovato. " +
                            "Tornando alla scena precedente...");
            Time.timeScale = 1f;
            SceneManager.UnloadSceneAsync("Settings");
        }
    }
    
    void OnDestroy()
    {
        IsPaused = false;
    }
}
