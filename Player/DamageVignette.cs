using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    [Header("Vignette Settings")]
    [SerializeField] private Color vignetteColor = new Color(0.6f, 0f, 0f, 1f);
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseIntensity = 0.3f; // How much it pulses
    
    [Header("Intensity Levels")]
    [SerializeField] private float level1Alpha = 0.25f; // <75% health
    [SerializeField] private float level2Alpha = 0.45f; // <50% health
    [SerializeField] private float level3Alpha = 0.7f;  // <25% health
    
    [Header("Vignette Shape")]
    [SerializeField] private int textureSize = 512;
    [SerializeField] private float innerRadius = 0.3f;  // Where vignette starts (0-1)
    [SerializeField] private float outerRadius = 1.0f;  // Where vignette is full strength
    [SerializeField] private float edgeSoftness = 2.5f; // How soft the edge is
    
    private Canvas vignetteCanvas;
    private RawImage vignetteImage;
    private Texture2D vignetteTexture;
    
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private float pulseTimer = 0f;
    private bool isPulsing = false;
    
    void Awake()
    {
        CreateVignetteUI();
    }
    
    void CreateVignetteUI()
    {
        // Create dedicated Canvas for vignette
        GameObject canvasObj = new GameObject("DamageVignetteCanvas");
        canvasObj.transform.SetParent(transform);
        
        vignetteCanvas = canvasObj.AddComponent<Canvas>();
        vignetteCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        vignetteCanvas.sortingOrder = 100; // Above game UI
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create vignette image
        GameObject imageObj = new GameObject("VignetteImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        vignetteImage = imageObj.AddComponent<RawImage>();
        vignetteImage.raycastTarget = false;
        
        // Stretch to fill screen
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Create radial vignette texture
        vignetteTexture = CreateRadialVignetteTexture();
        vignetteImage.texture = vignetteTexture;
        
        // Start invisible
        SetVignetteAlpha(0f);
    }
    
    Texture2D CreateRadialVignetteTexture()
    {
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float maxDist = textureSize / 2f;
        
        Color[] pixels = new Color[textureSize * textureSize];
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Calculate distance from center (normalized 0-1)
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                
                // Calculate vignette intensity with smooth falloff
                float vignette = 0f;
                
                if (dist > innerRadius)
                {
                    // Smooth interpolation from inner to outer radius
                    float t = (dist - innerRadius) / (outerRadius - innerRadius);
                    t = Mathf.Clamp01(t);
                    
                    // Apply power curve for softer edges
                    vignette = Mathf.Pow(t, edgeSoftness);
                }
                
                // Store as white with alpha (color applied at runtime)
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, vignette);
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return tex;
    }
    
    void Update()
    {
        // Early return se completamente invisibile e non in transizione
        if (targetAlpha == 0 && currentAlpha < 0.001f)
        {
            if (currentAlpha != 0)
            {
                currentAlpha = 0;
                SetVignetteAlpha(0);
            }
            return;
        }
        
        // Pulse effect when damaged
        if (targetAlpha > 0)
        {
            isPulsing = true;
            pulseTimer += Time.deltaTime * pulseSpeed;
            
            // Pulse between targetAlpha and targetAlpha * (1 + pulseIntensity)
            float pulse = 1f + Mathf.Sin(pulseTimer) * pulseIntensity;
            float targetWithPulse = targetAlpha * pulse;
            
            // Smooth transition to target
            currentAlpha = Mathf.Lerp(currentAlpha, targetWithPulse, Time.deltaTime * 10f);
        }
        else
        {
            // Fade out smoothly
            currentAlpha = Mathf.Lerp(currentAlpha, 0f, Time.deltaTime * 5f);
            
            if (currentAlpha < 0.01f)
            {
                currentAlpha = 0f;
                isPulsing = false;
                pulseTimer = 0f;
            }
        }
        
        SetVignetteAlpha(currentAlpha);
    }
    
    void SetVignetteAlpha(float alpha)
    {
        if (vignetteImage != null)
        {
            Color c = vignetteColor;
            c.a = alpha;
            vignetteImage.color = c;
        }
    }
    
    /// <summary>
    /// Update vignette based on health percentage (0-100)
    /// </summary>
    public void UpdateVignette(float healthPercent)
    {
        if (healthPercent >= 75f)
        {
            targetAlpha = 0f;
        }
        else if (healthPercent >= 50f)
        {
            targetAlpha = level1Alpha;
        }
        else if (healthPercent >= 25f)
        {
            targetAlpha = level2Alpha;
        }
        else
        {
            targetAlpha = level3Alpha;
        }
    }
    
    /// <summary>
    /// Flash effect when hit - immediate red flash
    /// </summary>
    public void FlashDamage()
    {
        // Immediate boost to alpha
        currentAlpha = Mathf.Max(currentAlpha + 0.3f, 0.5f);
        pulseTimer = 0f; // Reset pulse for dramatic effect
    }
    
    void OnDestroy()
    {
        if (vignetteTexture != null)
        {
            Destroy(vignetteTexture);
        }
    }
}
