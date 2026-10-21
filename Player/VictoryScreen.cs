using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryScreen : MonoBehaviour {
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float titleAnimationDelay = 0.5f;
    [SerializeField] private float statsAnimationDelay = 1.5f;
    [SerializeField] private float buttonAnimationDelay = 2.5f;
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.02f, 0.95f);
    [SerializeField] private Color victoryColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color accentColor = new Color(0.9f, 0.4f, 0.1f, 1f);
    [SerializeField] private Color statsColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    private GameObject victoryPanel;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;
    private TextMeshProUGUI statsText;
    private GameObject buttonsContainer;
    private TMP_FontAsset antonFont;
    private bool isShowing = false;
    private int roundsCompleted = 3;
    private int zombiesKilled = 0;
    private float timeSurvived = 0f;
    private static VictoryScreen instance;
    public static VictoryScreen Instance => instance;
    void Awake()
    {
        if (instance == null) instance = this;
        
        LoadFont();
        CreateVictoryUI();
        
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }
    
    void LoadFont()
    {
        if (customFont != null) antonFont = customFont;
        else {
            antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
            
            if (antonFont == null) antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }
    
    void CreateVictoryUI()
    {
        //Canvas
        GameObject canvasObj = new GameObject("VictoryCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 600;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        //Main panel
        victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvasObj.transform, false);
        Image bgImage = victoryPanel.AddComponent<Image>();
        bgImage.color = backgroundColor;
        canvasGroup = victoryPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        RectTransform panelRect = victoryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        CreateVignetteOverlay(victoryPanel.transform);
        CreateGoldenGlow(victoryPanel.transform);
        CreateVictoryTitle(victoryPanel.transform);
        CreateSubtitle(victoryPanel.transform);
        CreateStatsDisplay(victoryPanel.transform);
        CreateButtons(victoryPanel.transform);
    }
    
    void CreateVignetteOverlay(Transform parent) {
        GameObject vignetteObj = new GameObject("VignetteOverlay");
        vignetteObj.transform.SetParent(parent, false);
        Image vignetteImage = vignetteObj.AddComponent<Image>();
        
        //Radial gradient texture
        int size = 256;
        Texture2D vignetteTex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float alpha = Mathf.Clamp01(distance * distance * 0.5f);
                vignetteTex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        vignetteTex.Apply();
        vignetteImage.sprite = Sprite.Create(vignetteTex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        vignetteImage.color = Color.white;
        RectTransform vignetteRect = vignetteObj.GetComponent<RectTransform>();
        vignetteRect.anchorMin = Vector2.zero;
        vignetteRect.anchorMax = Vector2.one;
        vignetteRect.offsetMin = Vector2.zero;
        vignetteRect.offsetMax = Vector2.zero;
    }
    
    void CreateGoldenGlow(Transform parent) {
        GameObject glowObj = new GameObject("GoldenGlow");
        glowObj.transform.SetParent(parent, false);
        Image glowImage = glowObj.AddComponent<Image>();
        
        //Radial glow texture
        int size = 256;
        Texture2D glowTex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float alpha = Mathf.Clamp01(1f - distance) * 0.3f;
                glowTex.SetPixel(x, y, new Color(1f, 0.8f, 0.2f, alpha));
            }
        }
        glowTex.Apply();
        glowImage.sprite = Sprite.Create(glowTex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        glowImage.color = Color.white;
        RectTransform glowRect = glowObj.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.2f, 0.5f);
        glowRect.anchorMax = new Vector2(0.8f, 0.85f);
        glowRect.offsetMin = Vector2.zero;
        glowRect.offsetMax = Vector2.zero;
    }
    void CreateVictoryTitle(Transform parent) {
        GameObject titleObj = new GameObject("VictoryTitle");
        titleObj.transform.SetParent(parent, false);
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "VICTORY";
        titleText.font = antonFont;
        titleText.fontSize = 150;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = victoryColor;
        titleText.enableVertexGradient = true;
        titleText.colorGradient = new VertexGradient(
            new Color(1f, 0.95f, 0.4f),
            new Color(1f, 0.95f, 0.4f),
            new Color(0.9f, 0.6f, 0.1f),
            new Color(0.9f, 0.6f, 0.1f)
        );
        titleText.outlineWidth = 0.15f;
        titleText.outlineColor = new Color(0.4f, 0.2f, 0f, 1f);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.55f);
        titleRect.anchorMax = new Vector2(1, 0.85f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    void CreateSubtitle(Transform parent) {
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(parent, false);
        subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "YOU SURVIVED";
        subtitleText.font = antonFont;
        subtitleText.fontSize = 42;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        subtitleText.characterSpacing = 20f;
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0, 0.54f);
        subtitleRect.anchorMax = new Vector2(1, 0.60f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
    }
    void CreateStatsDisplay(Transform parent) {
        GameObject statsObj = new GameObject("StatsText");
        statsObj.transform.SetParent(parent, false);
        statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = "ROUNDS COMPLETED: 3\nZOMBIES KILLED: 0\nTIME SURVIVED: 00:00";
        statsText.font = antonFont;
        statsText.fontSize = 30;
        statsText.alignment = TextAlignmentOptions.Center;
        statsText.color = statsColor;
        statsText.lineSpacing = 25f;
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.3f, 0.26f);
        statsRect.anchorMax = new Vector2(0.7f, 0.44f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;
    }
    
    void CreateButtons(Transform parent) {
        buttonsContainer = new GameObject("ButtonsContainer");
        buttonsContainer.transform.SetParent(parent, false);
        HorizontalLayoutGroup layout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 60f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        RectTransform buttonsRect = buttonsContainer.GetComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.25f, 0.08f);
        buttonsRect.anchorMax = new Vector2(0.75f, 0.2f);
        buttonsRect.offsetMin = Vector2.zero;
        buttonsRect.offsetMax = Vector2.zero;
        CreateStyledButton(buttonsContainer.transform, "RETRY", OnRetryClicked, victoryColor);
        CreateStyledButton(buttonsContainer.transform, "MAIN MENU", OnMenuClicked, accentColor);
    }
    
    Button CreateStyledButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick, Color accentColor) {
        GameObject buttonObj = new GameObject(text + "Button");
        buttonObj.transform.SetParent(parent, false);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(2, 2);
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        colors.highlightedColor = new Color(accentColor.r * 0.3f, accentColor.g * 0.3f, accentColor.b * 0.3f, 1f);
        colors.pressedColor = new Color(accentColor.r * 0.5f, accentColor.g * 0.5f, accentColor.b * 0.5f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(onClick);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(280, 65);
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.font = antonFont;
        buttonText.fontSize = 34;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
    
    public void Show(int rounds, int kills, float time) {
        if (isShowing) return;
        
        isShowing = true;
        roundsCompleted = rounds;
        zombiesKilled = kills;
        timeSurvived = time;
        UpdateStats();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(AnimateVictory());
    }
    
    void UpdateStats() {
        if (statsText == null) return;
        
        int minutes = Mathf.FloorToInt(timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(timeSurvived % 60f);
        
        statsText.text = $"ROUNDS COMPLETED: {roundsCompleted}\n" + $"ZOMBIES KILLED: {zombiesKilled}\n" + $"TIME SURVIVED: {minutes:00}:{seconds:00}";
    }
    
    IEnumerator AnimateVictory() {
        victoryPanel.SetActive(true);
        
        //Hides elements initially
        if (titleText != null) titleText.alpha = 0f;
        if (subtitleText != null) subtitleText.alpha = 0f;
        if (statsText != null) statsText.alpha = 0f;
        
        if (buttonsContainer != null) {
            CanvasGroup btnGroup = buttonsContainer.GetComponent<CanvasGroup>();
            if (btnGroup == null) btnGroup = buttonsContainer.AddComponent<CanvasGroup>();
            btnGroup.alpha = 0f;
        }
        
        //Fade
        float elapsed = 0f;
        while (elapsed < fadeInDuration) {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        //Animations
        yield return new WaitForSecondsRealtime(titleAnimationDelay);
        yield return StartCoroutine(AnimateTextScale(titleText, 0.3f));
        yield return StartCoroutine(AnimateTextFadeIn(subtitleText, 0.4f));
        yield return new WaitForSecondsRealtime(statsAnimationDelay - titleAnimationDelay - 0.7f);
        yield return StartCoroutine(AnimateTextFadeIn(statsText, 0.5f));
        yield return new WaitForSecondsRealtime(0.3f);
        CanvasGroup buttonsGroup = buttonsContainer.GetComponent<CanvasGroup>();
        
        if (buttonsGroup != null) {
            elapsed = 0f;

            while (elapsed < 0.4f) {
                elapsed += Time.unscaledDeltaTime;
                buttonsGroup.alpha = elapsed / 0.4f;

                yield return null;
            }
            buttonsGroup.alpha = 1f;
        }
        
        Time.timeScale = 0f;
        StartCoroutine(PulseTitleEffect());
    }
    
    IEnumerator AnimateTextScale(TextMeshProUGUI text, float duration)
    {
        if (text == null) yield break;
        
        float elapsed = 0f;
        RectTransform rect = text.GetComponent<RectTransform>();
        Vector3 targetScale = rect.localScale;
        rect.localScale = targetScale * 1.5f;
        text.alpha = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic
            rect.localScale = Vector3.Lerp(targetScale * 1.5f, targetScale, t);
            text.alpha = t;
            yield return null;
        }
        rect.localScale = targetScale;
        text.alpha = 1f;
    }
    
    IEnumerator AnimateTextFadeIn(TextMeshProUGUI text, float duration) {
        if (text == null) yield break;
        
        float elapsed = 0f;
        text.alpha = 0f;
        
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            text.alpha = elapsed / duration;

            yield return null;
        }
        text.alpha = 1f;
    }
    
    IEnumerator PulseTitleEffect() {
        while (isShowing && titleText != null) {
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2f) * 0.02f;
            titleText.transform.localScale = Vector3.one * pulse;
            yield return null;
        }
    }
    
    void OnRetryClicked() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void OnMenuClicked() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    
    public static void ShowVictory(int rounds, int kills, float time)
    {
        if (instance != null) instance.Show(rounds, kills, time);
        else {
            GameObject obj = new GameObject("VictoryScreenManager");
            VictoryScreen vs = obj.AddComponent<VictoryScreen>();
            vs.Show(rounds, kills, time);
        }
    }
}
