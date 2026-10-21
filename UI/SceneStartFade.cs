using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneStartFade : MonoBehaviour {
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2.0f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private int sortOrder = 9999;
    private Canvas fadeCanvas;
    private Image fadeImage;

    void Awake() {
        CreateFadeUI();
    }

    void Start() {
        StartCoroutine(FadeOutRoutine());
    }

    void CreateFadeUI()
    {
        //Canvas
        GameObject canvasObj = new GameObject("SceneStartFadeCanvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = sortOrder;

        //Scaler (adapts objects based on screen)
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        //Panel
        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = panelObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        
        //Rectangle
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    IEnumerator FadeOutRoutine() {
        float elapsed = 0f;
        Color startColor = fadeColor;
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            //Similar to Lerp but smoother
            t = Mathf.SmoothStep(0f, 1f, t);
            
            if (fadeImage != null) fadeImage.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        if (fadeCanvas != null) {
            Destroy(fadeCanvas.gameObject);
        }
    }
}
