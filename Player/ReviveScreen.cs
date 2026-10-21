using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReviveScreen : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float glowPulseIntensity = 0.3f;
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.85f);
    [SerializeField] private Color reviveColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color progressBarColor = new Color(1f, 0.6f, 0.1f, 1f);
    [SerializeField] private Color heartbeatColor = new Color(0.8f, 0.1f, 0.1f, 0.6f);
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont;
    private GameObject revivePanel;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI revivingText;
    private TextMeshProUGUI countdownText;
    private Image progressBarFill;
    private Image progressBarGlow;
    private Image heartbeatVignette;
    private TMP_FontAsset antonFont;
    private GameObject progressBarContainer;
    private bool isActive = false;
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private int currentCountdown = 3;
    private static ReviveScreen instance;
    public static ReviveScreen Instance => instance;
    void Awake()
    {
        if (instance == null) instance = this;
        
        LoadFont();
        CreateReviveUI();
        
        if (revivePanel != null) revivePanel.SetActive(false);
    }
    
    void LoadFont()
    {
        if (customFont != null) antonFont = customFont;
        else {
            antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
            
            if (antonFont == null) antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }
    
    void Update()
    {
        if (!isActive) return;
        
        //Progress bar
        currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * 10f);
        
        if (progressBarFill != null) progressBarFill.fillAmount = currentProgress;
        
        UpdatePulseEffects();
    }
    
    void UpdatePulseEffects() {
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        
        //Glow pulse on progress bar
        if (progressBarGlow != null) {
            Color glowColor = progressBarColor;
            glowColor.a = 0.3f + pulse * glowPulseIntensity;
            progressBarGlow.color = glowColor;
        }
        
        //Heartbeat vignette pulse
        if (heartbeatVignette != null) {
            float heartbeat = Mathf.Sin(Time.time * 4f); //Faster heartbeat
            heartbeat = heartbeat > 0.7f ? 1f : 0f; //'Sharp' pulse
            Color vignetteColor = heartbeatColor;
            vignetteColor.a = heartbeat * 0.4f;
            heartbeatVignette.color = vignetteColor;
        }
        
        //Text pulse
        if (revivingText != null) {
            float textPulse = 1f + pulse * 0.05f;
            revivingText.transform.localScale = Vector3.one * textPulse;
        }
    }
    
    void CreateReviveUI() {
        //Canvas
        GameObject canvasObj = new GameObject("ReviveCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        //Main panel
        revivePanel = new GameObject("RevivePanel");
        revivePanel.transform.SetParent(canvasObj.transform, false);
        Image bgImage = revivePanel.AddComponent<Image>();
        bgImage.color = backgroundColor;
        canvasGroup = revivePanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        RectTransform panelRect = revivePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        CreateRadialVignette(revivePanel.transform);
        CreateRevivingText(revivePanel.transform);
        CreateCountdownText(revivePanel.transform);
        CreatePerkWarning(revivePanel.transform);
    }
    void CreateRadialVignette(Transform parent) {
        GameObject vignetteObj = new GameObject("RadialVignette");
        vignetteObj.transform.SetParent(parent, false);
        Image vignetteImage = vignetteObj.AddComponent<Image>();
        int size = 256;
        Texture2D vignetteTex = new Texture2D(size, size);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                float alpha = Mathf.Clamp01(distance * distance * 0.7f);
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
    void CreateRevivingText(Transform parent) {
        GameObject textObj = new GameObject("RevivingText");
        textObj.transform.SetParent(parent, false);
        revivingText = textObj.AddComponent<TextMeshProUGUI>();
        revivingText.text = "REVIVING...";
        revivingText.font = antonFont;
        revivingText.fontSize = 72;
        revivingText.alignment = TextAlignmentOptions.Center;
        revivingText.color = reviveColor;
        revivingText.characterSpacing = 5f;
        revivingText.outlineWidth = 0.2f;
        revivingText.outlineColor = new Color(0.1f, 0.4f, 0.2f, 1f);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    void CreateCountdownText(Transform parent)
    {
        GameObject countdownObj = new GameObject("CountdownText");
        countdownObj.transform.SetParent(parent, false);
        countdownText = countdownObj.AddComponent<TextMeshProUGUI>();
        countdownText.text = "3";
        countdownText.font = antonFont;
        countdownText.fontSize = 120;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.color = new Color(1f, 0.5f, 0f, 0.95f);
        RectTransform countdownRect = countdownObj.GetComponent<RectTransform>();
        countdownRect.anchorMin = new Vector2(0, 0.35f);
        countdownRect.anchorMax = new Vector2(1, 0.5f);
        countdownRect.offsetMin = Vector2.zero;
        countdownRect.offsetMax = Vector2.zero;
    }
    void CreatePerkWarning(Transform parent) {
        GameObject warningObj = new GameObject("PerkWarning");
        warningObj.transform.SetParent(parent, false);
        TextMeshProUGUI warningText = warningObj.AddComponent<TextMeshProUGUI>();
        warningText.text = "PERKS WILL BE LOST";
        warningText.font = antonFont;
        warningText.fontSize = 24;
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.color = new Color(1f, 1f, 1f, 0.9f);
        RectTransform warningRect = warningObj.GetComponent<RectTransform>();
        warningRect.anchorMin = new Vector2(0, 0.22f);
        warningRect.anchorMax = new Vector2(1, 0.28f);
        warningRect.offsetMin = Vector2.zero;
        warningRect.offsetMax = Vector2.zero;
    }
    
    public void Show() {
        if (isActive) return;
        
        isActive = true;
        currentProgress = 0f;
        targetProgress = 0f;
        currentCountdown = 3;
        
        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (countdownText != null) countdownText.text = "3";
        
        StartCoroutine(FadeIn());
    }
    
    public void Hide() {
        isActive = false;
        StartCoroutine(FadeOut());
    }
    
    public void SetProgress(float progress) {
        targetProgress = Mathf.Clamp01(progress);
        
        //Updates countdown based on progress
        int newCountdown = 3 - Mathf.FloorToInt(progress * 3f);
        newCountdown = Mathf.Clamp(newCountdown, 0, 3);
        
        if (newCountdown != currentCountdown && countdownText != null) {
            currentCountdown = newCountdown;
            countdownText.text = currentCountdown > 0 ? currentCountdown.ToString() : "GO!";
            StartCoroutine(CountdownPopEffect());
        }
    }
    
    IEnumerator CountdownPopEffect() {
        if (countdownText == null) yield break;
        
        Vector3 originalScale = Vector3.one;
        countdownText.transform.localScale = originalScale * 1.3f;
        float elapsed = 0f;

        while (elapsed < 0.2f) {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.2f;
            countdownText.transform.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, t);

            yield return null;
        }
        countdownText.transform.localScale = originalScale;
    }
    
    IEnumerator FadeIn() {
        revivePanel.SetActive(true);
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / duration;

            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOut() {
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        revivePanel.SetActive(false);
    }
    
    public static void ShowRevive()
    {
        if (instance != null) instance.Show();
        else {
            GameObject obj = new GameObject("ReviveScreenManager");
            ReviveScreen rs = obj.AddComponent<ReviveScreen>();
            rs.Show();
        }
    }
    
    public static void HideRevive()
    {
        if (instance != null) instance.Hide();
    }
    public static void UpdateProgress(float progress)
    {
        if (instance != null) instance.SetProgress(progress);
    }
}
