using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

//Manages collectibles items + achievement popup
public class CollectibleManager : MonoBehaviour
{
    [Header("Achievement UI")]
    [SerializeField] private float popupDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    [SerializeField] private Color textColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color countColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    private static CollectibleManager instance;
    public static CollectibleManager Instance => instance;
    [Header("Completion Bonus")]
    [SerializeField] private int allCollectedBonus = 1000;
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip allCollectedSound;
    private int totalCollectibles = 0;
    private int collectedCount = 0;
    private List<Collectible> allCollectibles = new List<Collectible>();
    private PointsDisplay pointsDisplay;
    private GameObject achievementPanel;
    private TextMeshProUGUI achievementText;
    private TextMeshProUGUI countText;
    private CanvasGroup canvasGroup;
    private Coroutine currentPopup;
    void Awake() {
        if (instance == null) instance = this;
        
        CreateAchievementUI();
        
        if (achievementPanel != null) achievementPanel.SetActive(false);
    }
    
    void Start() {
        RegisterAllCollectibles();
        pointsDisplay = FindFirstObjectByType<PointsDisplay>();
    }
    
    void RegisterAllCollectibles() {
        Collectible[] collectibles = FindObjectsOfType<Collectible>();
        allCollectibles.AddRange(collectibles);
        totalCollectibles = collectibles.Length;
    }
    
    void CreateAchievementUI() {
        //Canvas
        GameObject canvasObj = new GameObject("AchievementCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        //achievement panel
        achievementPanel = new GameObject("AchievementPanel");
        achievementPanel.transform.SetParent(canvasObj.transform, false);
        Image panelImage = achievementPanel.AddComponent<Image>();
        panelImage.color = backgroundColor;
        Outline outline = achievementPanel.AddComponent<Outline>();
        outline.effectColor = textColor;
        outline.effectDistance = new Vector2(2, 2);
        canvasGroup = achievementPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        RectTransform panelRect = achievementPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-30, -30);
        panelRect.sizeDelta = new Vector2(350, 100);
        
        //icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(achievementPanel.transform, false);
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "★";
        iconText.fontSize = 50;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = textColor;
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0);
        iconRect.anchorMax = new Vector2(0, 1);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(15, 0);
        iconRect.sizeDelta = new Vector2(60, 100);
        
        //text
        GameObject textObj = new GameObject("AchievementText");
        textObj.transform.SetParent(achievementPanel.transform, false);
        achievementText = textObj.AddComponent<TextMeshProUGUI>();
        achievementText.text = "COLLECTIBLE FOUND!";
        achievementText.fontSize = 24;
        achievementText.fontStyle = FontStyles.Bold;
        achievementText.alignment = TextAlignmentOptions.Left;
        achievementText.color = textColor;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0, 0.5f);
        textRect.anchoredPosition = new Vector2(80, 0);
        textRect.sizeDelta = new Vector2(-100, 50);
        GameObject countObj = new GameObject("CountText");
        countObj.transform.SetParent(achievementPanel.transform, false);
        countText = countObj.AddComponent<TextMeshProUGUI>();
        countText.text = "1 / 3";
        countText.fontSize = 32;
        countText.fontStyle = FontStyles.Bold;
        countText.alignment = TextAlignmentOptions.Left;
        countText.color = countColor;
        RectTransform countRect = countObj.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0, 0);
        countRect.anchorMax = new Vector2(1, 0.5f);
        countRect.pivot = new Vector2(0, 0.5f);
        countRect.anchoredPosition = new Vector2(80, 0);
        countRect.sizeDelta = new Vector2(-100, 50);
    }
    
    public void OnCollectibleCollected(Collectible collectible) {
        collectedCount++;
        
        if (audioSource != null) {
            float volume = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            
            if (collectedCount >= totalCollectibles && allCollectedSound != null) {
                audioSource.PlayOneShot(allCollectedSound, volume);
            }
            else if (collectSound != null) {
                audioSource.PlayOneShot(collectSound, volume);
            }
        }
        
        //popup
        if (currentPopup != null) StopCoroutine(currentPopup);
        currentPopup = StartCoroutine(ShowAchievementPopup());
        Debug.Log($"(CollectibleManager) Collected {collectedCount}/{totalCollectibles}");
        
        if (collectedCount >= totalCollectibles) {
            Debug.Log("(CollectibleManager) ALL COLLECTIBLES FOUND!");
            
            //1000 points for collecting all collectibles
            if (pointsDisplay != null) {
                pointsDisplay.AddPoints(allCollectedBonus);
                Debug.Log($"(CollectibleManager) awarded {allCollectedBonus} bonus points!");
            }
            else Debug.LogWarning("(CollectibleManager) could not award bonus points!");
        }
    }
    
    IEnumerator ShowAchievementPopup() {
        if (countText != null) countText.text = $"{collectedCount} / {totalCollectibles}";
        
        if (collectedCount >= totalCollectibles && achievementText != null) {
            achievementText.text = "ALL COLLECTIBLES!";
            achievementText.color = new Color(0.3f, 1f, 0.3f, 1f);
        }
        else if (achievementText != null) {
            achievementText.text = "COLLECTIBLE FOUND!";
            achievementText.color = textColor;
        }
        
        achievementPanel.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeInDuration) {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        yield return new WaitForSeconds(popupDuration);
        
        elapsed = 0f;
        
        while (elapsed < fadeOutDuration) {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);

            yield return null;
        }
        canvasGroup.alpha = 0f;
        achievementPanel.SetActive(false);
    }
    
    public int GetCollectedCount() => collectedCount;
    public int GetTotalCount() => totalCollectibles;
}
