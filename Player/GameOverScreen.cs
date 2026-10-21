using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour {
    [Header("UI References - Auto-created if not assigned")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float textAnimationDelay = 0.5f;
    [SerializeField] private float buttonAnimationDelay = 1.5f;
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.02f, 0.95f);
    [SerializeField] private Color titleColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color statsColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    private CanvasGroup canvasGroup;
    private TMP_FontAsset antonFont;
    private bool isShowing = false;
    public static bool IsGameOver => instance != null && instance.isShowing; //'True' if game over screen is visible
    private int roundsSurvived = 1;
    private int zombiesKilled = 0;
    private float timeSurvived = 0f;
    private static GameOverScreen instance;
    
    void Awake() {
        if (instance == null) instance = this;
        
        LoadFont();
        
        if (gameOverPanel == null) CreateGameOverUI();
        
        //Hide screen at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    
    void LoadFont()
    {
        if (customFont != null) {
            antonFont = customFont;
        } else {
            antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
            
            if (antonFont == null) {
                antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
        }
    }
    
    void Update() {
        if (!isShowing) timeSurvived += Time.deltaTime;
    }
    
    void CreateGameOverUI()
    {
        //XCanvas
        GameObject canvasObj = new GameObject("GameOverCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        //Main panel
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        Image bgImage = gameOverPanel.AddComponent<Image>();
        bgImage.color = backgroundColor;
        canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        //"GAME OVER"
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(gameOverPanel.transform, false);
        gameOverText = titleObj.AddComponent<TextMeshProUGUI>();
        gameOverText.text = "GAME OVER";
        gameOverText.font = antonFont;
        gameOverText.fontSize = 120;
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.color = titleColor;
        gameOverText.enableVertexGradient = true;
        gameOverText.colorGradient = new VertexGradient(
            new Color(1f, 0.2f, 0.2f),
            new Color(1f, 0.2f, 0.2f),
            new Color(0.5f, 0f, 0f),
            new Color(0.5f, 0f, 0f)
        );
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.55f);
        titleRect.anchorMax = new Vector2(1, 0.85f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        //"YOU DIED"
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "YOU DIED";
        subtitleText.font = antonFont;
        subtitleText.fontSize = 36;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        subtitleText.characterSpacing = 15f;
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0, 0.48f);
        subtitleRect.anchorMax = new Vector2(1, 0.55f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
        
        //Statistics
        GameObject statsObj = new GameObject("StatsText");
        statsObj.transform.SetParent(gameOverPanel.transform, false);
        statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "ROUND: 1\nZOMBIES KILLED: 0\nTIME SURVIVED: 00:00";
        statsText.font = antonFont;
        statsText.fontSize = 28;
        statsText.alignment = TextAlignmentOptions.Center;
        statsText.color = statsColor;
        statsText.lineSpacing = 20f;
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.3f, 0.28f);
        statsRect.anchorMax = new Vector2(0.7f, 0.45f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;
        
        //Buttons container
        GameObject buttonsContainer = new GameObject("ButtonsContainer");
        buttonsContainer.transform.SetParent(gameOverPanel.transform, false);
        HorizontalLayoutGroup layout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 50f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.25f, 0.1f);
        buttonsRect.anchorMax = new Vector2(0.75f, 0.22f);
        buttonsRect.offsetMin = Vector2.zero;
        buttonsRect.offsetMax = Vector2.zero;
        
        //Retry
        retryButton = CreateStyledButton(buttonsContainer.transform, "RETRY", OnRetryClicked);
        //Menu
        menuButton = CreateStyledButton(buttonsContainer.transform, "MAIN MENU", OnMenuClicked);
    }
    
    Button CreateStyledButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick) {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent, false);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        Button button = buttonObj.AddComponent<Button>();
        //Colors
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.highlightedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        colors.pressedColor = new Color(0.4f, 0.05f, 0.05f, 1f);
        colors.selectedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        button.colors = colors;
        button.onClick.AddListener(onClick);
        //Size
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(250, 60);
        //Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.font = antonFont;
        buttonText.fontSize = 32;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    public void Show(int rounds = -1, int kills = -1) {
        if (isShowing) return;
        
        isShowing = true;
        
        //Update stats if valid values are passed (otherwise use tracked values)
        if (rounds >= 0) roundsSurvived = rounds;
        if (kills >= 0) zombiesKilled = kills;
        
        UpdateStats();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Start animation
        StartCoroutine(AnimateGameOver());
    }
    
    void UpdateStats() {
        if (statsText == null) return;
        
        int minutes = Mathf.FloorToInt(timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(timeSurvived % 60f);
        
        statsText.text = $"ROUND: {roundsSurvived}\n" + $"ZOMBIES KILLED: {zombiesKilled}\n" + $"TIME SURVIVED: {minutes:00}:{seconds:00}";
    }
    
    IEnumerator AnimateGameOver() {
        gameOverPanel.SetActive(true);
        
        //Fades in background
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration) {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / fadeInDuration;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        //Pauses game after fade
        Time.timeScale = 0f;
    }
    
    void OnRetryClicked() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void OnMenuClicked() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    
    //Shows game over from anywhere
    public static void ShowGameOver(int rounds = 1, int kills = 0) {
        if (instance != null) instance.Show(rounds, kills);
    }
    
    //Called when a zombie is killed
    public static void AddKill() {
        if (instance != null) instance.zombiesKilled++;
    }
    
    //Called when round changes
    public static void SetRound(int round) {
        if (instance != null) instance.roundsSurvived = round;
    }
}
