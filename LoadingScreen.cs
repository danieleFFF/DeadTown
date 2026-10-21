using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour {
    [Header("Loading Screen Settings")]
    [SerializeField] private float minimumLoadTime = 2f;
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
    [SerializeField] private Color progressBarColor = new Color(100f, 0.65f, 10f, 1f);
    [SerializeField] private Color progressBarBackgroundColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset mainFont;
    [SerializeField] private TMP_FontAsset tipsFont;
    [Header("Optional Custom UI")]
    [SerializeField] private GameObject customLoadingPanel;
    [SerializeField] private Slider customProgressBar;
    [SerializeField] private TextMeshProUGUI customProgressText;
    [SerializeField] private TextMeshProUGUI customTipText;
    [Header("Loading Tips")]
    [SerializeField] private string[] loadingTips = new string[]{
        "Prepare to fight the ordes...",
        "Aim for the head for critical damage!",
        "Find the perk machines for boosts",
        "Don't have enough ammo? Buy the same weapon, or another one!",
        "Explore the map to find better weapons",
        "What happens if you collect all the collectibles...?",
        "Zombies have an EXPLOSIVE attraction to monkeys...",
    };
    private Canvas loadingCanvas;
    private GameObject loadingPanel;
    private Slider progressBar;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI tipText;
    private TextMeshProUGUI loadingText;
    private Image spinnerImage;
    private RectTransform spinnerRect;
    private TMP_FontAsset antonFont;
    private TMP_FontAsset oswaldFont;
    private static LoadingScreen instance;
    private bool isLoading = false;
    public static bool IsLoading => instance != null && instance.isLoading; //'True' if it's loading a scene.

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadFont();
            CreateLoadingUI();
        } else {
            Destroy(gameObject);
        }
    }
    
    void LoadFont() {
        if (mainFont != null) antonFont = mainFont;
        else {
            antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");

            if (antonFont == null) antonFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
        
        //Load tips font
        if (tipsFont != null) oswaldFont = tipsFont;
        else {
            oswaldFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald Bold SDF");
            
            if (oswaldFont == null) oswaldFont = antonFont;
        }
    }
    
    void Update() {
        //Spinner rotation when loading
        if (isLoading && spinnerRect != null)
            spinnerRect.Rotate(0, 0, -200f * Time.unscaledDeltaTime);
    }
    
    void CreateLoadingUI() {
        //Use custom UI
        if (customLoadingPanel != null){
            loadingPanel = customLoadingPanel;
            progressBar = customProgressBar;
            progressText = customProgressText;
            tipText = customTipText;
            loadingPanel.SetActive(false);

            return;
        }
        
        //Canvas
        GameObject canvasObj = new GameObject("LoadingCanvas");
        canvasObj.transform.SetParent(transform);
        loadingCanvas = canvasObj.AddComponent<Canvas>();
        loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        loadingCanvas.sortingOrder = 999;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        //Background panel
        loadingPanel = new GameObject("LoadingPanel");
        loadingPanel.transform.SetParent(canvasObj.transform, false);
        Image bgImage = loadingPanel.AddComponent<Image>();
        bgImage.color = backgroundColor;
        RectTransform bgRect = loadingPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        //Container
        GameObject container = new GameObject("Container");
        container.transform.SetParent(loadingPanel.transform, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(600, 300);
        
        //Loading
        GameObject loadingTextObj = new GameObject("LoadingText");
        loadingTextObj.transform.SetParent(container.transform, false);
        loadingText = loadingTextObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = "LOADING";
        loadingText.font = antonFont;
        loadingText.fontSize = 72;
        loadingText.fontStyle = FontStyles.Bold;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = Color.white;
        RectTransform loadingTextRect = loadingTextObj.GetComponent<RectTransform>();
        loadingTextRect.anchorMin = new Vector2(0, 0.7f);
        loadingTextRect.anchorMax = new Vector2(1, 1f);
        loadingTextRect.offsetMin = Vector2.zero;
        loadingTextRect.offsetMax = Vector2.zero;
        
        //Spinner
        GameObject spinnerObj = new GameObject("Spinner");
        spinnerObj.transform.SetParent(container.transform, false);
        spinnerImage = spinnerObj.AddComponent<Image>();
        spinnerImage.color = progressBarColor;
        Texture2D spinnerTex = CreateSpinnerTexture();
        spinnerImage.sprite = Sprite.Create(spinnerTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        spinnerRect = spinnerObj.GetComponent<RectTransform>();
        spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
        spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        spinnerRect.sizeDelta = new Vector2(60, 60);
        spinnerRect.anchoredPosition = new Vector2(0, 20);
        
        //Progress bar background
        GameObject progressBg = new GameObject("ProgressBarBackground");
        progressBg.transform.SetParent(container.transform, false);
        Image progressBgImage = progressBg.AddComponent<Image>();
        progressBgImage.color = progressBarBackgroundColor;
        RectTransform progressBgRect = progressBg.GetComponent<RectTransform>();
        progressBgRect.anchorMin = new Vector2(0.1f, 0.25f);
        progressBgRect.anchorMax = new Vector2(0.9f, 0.35f);
        progressBgRect.offsetMin = Vector2.zero;
        progressBgRect.offsetMax = Vector2.zero;
        
        //Progress bar/slider
        GameObject sliderObj = new GameObject("ProgressBar");
        sliderObj.transform.SetParent(progressBg.transform, false);
        progressBar = sliderObj.AddComponent<Slider>();
        progressBar.interactable = false;
        progressBar.transition = Selectable.Transition.None;
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        //Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        //Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = progressBarColor;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        progressBar.fillRect = fillRect;
        progressBar.value = 0;
        
        //Percentage text 
        GameObject percentTextObj = new GameObject("PercentText");
        percentTextObj.transform.SetParent(container.transform, false);
        progressText = percentTextObj.AddComponent<TextMeshProUGUI>();
        progressText.text = "0%";
        progressText.font = antonFont;
        progressText.fontSize = 36;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.color = Color.white;
        RectTransform percentRect = percentTextObj.GetComponent<RectTransform>();
        percentRect.anchorMin = new Vector2(0, 0.1f);
        percentRect.anchorMax = new Vector2(1, 0.25f);
        percentRect.offsetMin = Vector2.zero;
        percentRect.offsetMax = Vector2.zero;
        
        //Tip text
        GameObject tipTextObj = new GameObject("TipText");
        tipTextObj.transform.SetParent(container.transform, false);
        tipText = tipTextObj.AddComponent<TextMeshProUGUI>();
        tipText.text = "";
        tipText.font = oswaldFont;
        tipText.fontSize = 22;
        tipText.characterSpacing = 2f;
        tipText.alignment = TextAlignmentOptions.Center;
        tipText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        RectTransform tipRect = tipTextObj.GetComponent<RectTransform>();
        tipRect.anchorMin = new Vector2(0, -0.15f);
        tipRect.anchorMax = new Vector2(1, 0.05f);
        tipRect.offsetMin = Vector2.zero;
        tipRect.offsetMax = Vector2.zero;
        
        //Hides the panel initially
        loadingPanel.SetActive(false);
    }
    
    Texture2D CreateSpinnerTexture() {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color transparent = new Color(0, 0, 0, 0);
        
        //Fill with transparent
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                tex.SetPixel(x, y, transparent);
            }
        }
        
        //Draws spinner shape
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerRadius = size / 2f - 2;
        float innerRadius = size / 2f - 10;
        
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist >= innerRadius && dist <= outerRadius) {
                    //Calculates angle
                    float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                    angle = (angle + 360) % 360;
                    
                    //Draws only part of the ring
                    if (angle < 270) {
                        float alpha = 1f - (angle / 270f) * 0.7f; //Fade effect
                        tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                    }
                }
            }
        }
        tex.Apply();

        return tex;
    }
    
    public void LoadScene(string sceneName) {
        if (!isLoading) StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    IEnumerator LoadSceneAsync(string sceneName) {
        isLoading = true;
        
        if (loadingPanel != null) {
            loadingPanel.SetActive(true);
            
            //Random tip
            if (tipText != null && loadingTips.Length > 0)
                tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
        
        //Reset progress
        if (progressBar != null) progressBar.value = 0;
        if (progressText != null) progressText.text = "0%";
        
        float startTime = Time.unscaledTime;
        
        //Starts loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        float displayProgress = 0f;
        
        while (!asyncLoad.isDone) {
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            //Progress animation
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.unscaledDeltaTime * 0.5f);
            
            //Updates ui
            if (progressBar != null) progressBar.value = displayProgress;
            if (progressText != null) progressText.text = Mathf.RoundToInt(displayProgress * 100f) + "%";
            
            if (asyncLoad.progress >= 0.9f) {
                float elapsedTime = Time.unscaledTime - startTime;
                
                if (elapsedTime >= minimumLoadTime) {
                    while (displayProgress < 1f) {
                        displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.unscaledDeltaTime * 2f);
                        if (progressBar != null) progressBar.value = displayProgress;
                        if (progressText != null) progressText.text = Mathf.RoundToInt(displayProgress * 100f) + "%";
                        yield return null;
                    }
                    
                    yield return new WaitForSecondsRealtime(0.3f);
                    
                    //Activates scene
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
        
        //Small delay
        yield return new WaitForSecondsRealtime(0.2f);
        //Hides loading panel
        if (loadingPanel != null) loadingPanel.SetActive(false);
        
        isLoading = false;
    }
}
