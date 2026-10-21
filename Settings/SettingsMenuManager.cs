using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

//Gestisce il menu Settings con 3 tab: General, Graphics, Sound
public class SettingsMenuManager : MonoBehaviour
{
    [Header("Tab Buttons")]
    public Button generalTabButton;
    public Button graphicsTabButton;
    public Button soundTabButton;
    public Button leftArrowButton;
    public Button rightArrowButton;
    [Header("Tab Panels")]
    public GameObject generalPanel;
    public GameObject graphicsPanel;
    public GameObject soundPanel;
    [Header("Tab Colors")]
    public Color activeTabColor = new Color(1f, 0.4f, 0f); // Orange #FF6600
    public Color inactiveTabColor = new Color(0.29f, 0.35f, 0.39f); // Gray #4A5A64
    [Header("General Panel")]
    public Button returnToMenuButton;
    public Button returnToDesktopButton;
    [Header("Graphics Settings")]
    public TMP_Dropdown resolutionDropdown;
    public TextMeshProUGUI resolutionValueText;
    public TMP_Dropdown displayModeDropdown;
    public TextMeshProUGUI displayModeValueText;
    public Slider fovSlider;
    public TextMeshProUGUI fovValueText;
    public TMP_Dropdown qualityDropdown;
    public TextMeshProUGUI qualityValueText;
    [Header("Sound Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    [Header("Footer Buttons")]
    public Button applyButton;
    public Button backButton;
    [Header("Keyboard Sounds")]
    [SerializeField] private AudioClip keyPressSound;
    private AudioSource audioSource;
    private List<Button> tabButtons;
    private List<GameObject> tabPanels;
    private int currentTabIndex = 0; //Starts with general tab
    
    void Awake() {
        //Auto-bind UI references if not set
        AutoBindReferences();
    }
    
    void AutoBindReferences()
    {
        Transform canvas = transform;
        
        // Tab Buttons
        if (generalTabButton == null) generalTabButton = FindButton(canvas, "TabContainer/GeneralTab");
        if (graphicsTabButton == null) graphicsTabButton = FindButton(canvas, "TabContainer/GraphicsTab");
        if (soundTabButton == null) soundTabButton = FindButton(canvas, "TabContainer/SoundTab");
        if (leftArrowButton == null) leftArrowButton = FindButton(canvas, "TabContainer/LeftArrow");
        if (rightArrowButton == null) rightArrowButton = FindButton(canvas, "TabContainer/RightArrow");
        
        // Tab Panels
        if (generalPanel == null) generalPanel = FindChild(canvas, "ContentArea/GeneralPanel");
        if (graphicsPanel == null) graphicsPanel = FindChild(canvas, "ContentArea/GraphicsPanel");
        if (soundPanel == null) soundPanel = FindChild(canvas, "ContentArea/SoundPanel");
        
        // General Panel buttons
        if (returnToMenuButton == null) returnToMenuButton = FindButton(canvas, "ContentArea/GeneralPanel/ReturnToMenuBtn");
        if (returnToDesktopButton == null) returnToDesktopButton = FindButton(canvas, "ContentArea/GeneralPanel/ReturnToDesktopBtn");
        
        // Footer
        if (backButton == null) backButton = FindButton(canvas, "Footer/BackButton");
        if (applyButton == null) applyButton = FindButton(canvas, "Footer/ApplyButton");
        
        // Graphics Settings
        if (resolutionDropdown == null) resolutionDropdown = FindDropdown(canvas, "ContentArea/GraphicsPanel/ResolutionRow/Dropdown");
        if (resolutionValueText == null) resolutionValueText = FindText(canvas, "ContentArea/GraphicsPanel/ResolutionRow/ResolutionValue");
        if (displayModeDropdown == null) displayModeDropdown = FindDropdown(canvas, "ContentArea/GraphicsPanel/DisplayModeRow/Dropdown");
        if (displayModeValueText == null) displayModeValueText = FindText(canvas, "ContentArea/GraphicsPanel/DisplayModeRow/DisplayModeValue");
        if (fovSlider == null) fovSlider = FindSlider(canvas, "ContentArea/GraphicsPanel/FOVRow/Slider");
        if (fovValueText == null) fovValueText = FindText(canvas, "ContentArea/GraphicsPanel/FOVRow/Value");
        if (qualityDropdown == null) qualityDropdown = FindDropdown(canvas, "ContentArea/GraphicsPanel/QualityRow/Dropdown");
        if (qualityValueText == null) qualityValueText = FindText(canvas, "ContentArea/GraphicsPanel/QualityRow/QualityValue");
        
        // Sound Settings
        if (masterVolumeSlider == null) masterVolumeSlider = FindSlider(canvas, "ContentArea/SoundPanel/MasterVolumeRow/Slider");
        if (masterVolumeText == null) masterVolumeText = FindText(canvas, "ContentArea/SoundPanel/MasterVolumeRow/Value");
        if (musicVolumeSlider == null) musicVolumeSlider = FindSlider(canvas, "ContentArea/SoundPanel/MusicVolumeRow/Slider");
        if (musicVolumeText == null) musicVolumeText = FindText(canvas, "ContentArea/SoundPanel/MusicVolumeRow/Value");
        if (sfxVolumeSlider == null) sfxVolumeSlider = FindSlider(canvas, "ContentArea/SoundPanel/SFXVolumeRow/Slider");
        if (sfxVolumeText == null) sfxVolumeText = FindText(canvas, "ContentArea/SoundPanel/SFXVolumeRow/Value");
        
        // Set initial text values
        SetInitialTexts();
    }
    
    void SetInitialTexts()
    {
        var titleText = transform.Find("Header/TitleText")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null) titleText.text = "SETTINGS";
    }
    
    void SetLabelText(Transform parent, string text)
    {
        if (parent == null) return;
        var label = parent.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (label != null) label.text = text;
    }
    
    void SetTextOnButton(Button btn, string text)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
    }
    
    GameObject FindChild(Transform parent, string path)
    {
        var t = parent.Find(path);
        return t != null ? t.gameObject : null;
    }
    
    Button FindButton(Transform parent, string path)
    {
        var t = parent.Find(path);
        return t != null ? t.GetComponent<Button>() : null;
    }
    
    TMP_Dropdown FindDropdown(Transform parent, string path)
    {
        var t = parent.Find(path);
        return t != null ? t.GetComponent<TMP_Dropdown>() : null;
    }
    
    Slider FindSlider(Transform parent, string path)
    {
        var t = parent.Find(path);
        return t != null ? t.GetComponent<Slider>() : null;
    }
    
    Toggle FindToggle(Transform parent, string path)
    {
        var t = parent.Find(path);
        return t != null ? t.GetComponent<Toggle>() : null;
    }
    
    TextMeshProUGUI FindText(Transform parent, string path)
    {
        var t = parent.Find(path);
        return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
    }
    
    void Start()
    {
        // Initialize tab lists
        tabButtons = new List<Button> { generalTabButton, graphicsTabButton, soundTabButton };
        tabPanels = new List<GameObject> { generalPanel, graphicsPanel, soundPanel };
        
        // Setup tab button listeners
        if (generalTabButton != null) generalTabButton.onClick.AddListener(() => SwitchTab(0));
        if (graphicsTabButton != null) graphicsTabButton.onClick.AddListener(() => SwitchTab(1));
        if (soundTabButton != null) soundTabButton.onClick.AddListener(() => SwitchTab(2));
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(PreviousTab);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(NextTab);
        
        // Setup General panel buttons
        if (returnToMenuButton != null) returnToMenuButton.onClick.AddListener(ReturnToMenu);
        if (returnToDesktopButton != null) returnToDesktopButton.onClick.AddListener(ReturnToDesktop);
        
        // Setup slider listeners
        if (fovSlider != null) fovSlider.onValueChanged.AddListener(UpdateFovValue);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(UpdateSfxVolume);
        
        // Setup dropdown listeners for value text updates
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(UpdateResolutionValue);
        if (displayModeDropdown != null) displayModeDropdown.onValueChanged.AddListener(UpdateDisplayModeValue);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(UpdateQualityValue);
        
        // Setup footer buttons
        if (applyButton != null) applyButton.onClick.AddListener(ApplySettings);
        if (backButton != null) backButton.onClick.AddListener(Back);
        
        // Initialize dropdowns
        InitializeSettings();
        
        // Show initial tab
        SwitchTab(currentTabIndex);
    }
    
    void InitializeSettings()
    {
        // Resolution dropdown
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> resolutions = new List<string>();
            foreach (var res in Screen.resolutions)
            {
                resolutions.Add($"{res.width}x{res.height}");
            }
            resolutionDropdown.AddOptions(resolutions);
            // Select current resolution
            string current = $"{Screen.width}x{Screen.height}";
            int index = resolutions.IndexOf(current);
            if (index >= 0) resolutionDropdown.value = index;
            resolutionDropdown.RefreshShownValue();
            UpdateResolutionValue(resolutionDropdown.value);
        }
        
        // Display mode dropdown
        if (displayModeDropdown != null)
        {
            displayModeDropdown.ClearOptions();
            displayModeDropdown.AddOptions(new List<string> { "Full Screen", "Windowed", "Borderless" });
            displayModeDropdown.value = Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? 0 : 1;
            displayModeDropdown.RefreshShownValue();
            UpdateDisplayModeValue(displayModeDropdown.value);
        }
        
            // Quality dropdown - Custom Low/High
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new List<string> { "LOW", "HIGH" });
                // Get saved value, default to High (1)
                qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 1);
                qualityDropdown.RefreshShownValue();
                UpdateQualityValue(qualityDropdown.value);
            }
        
        // FOV slider
        if (fovSlider != null)
        {
            fovSlider.minValue = 60;
            fovSlider.maxValue = 120;
            fovSlider.value = PlayerPrefs.GetFloat("FOV", 80);
            UpdateFovValue(fovSlider.value);
        }
        
        // Volume sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0;
            masterVolumeSlider.maxValue = 1;
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            UpdateMasterVolume(masterVolumeSlider.value);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0;
            musicVolumeSlider.maxValue = 1;
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            UpdateMusicVolume(musicVolumeSlider.value);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0;
            sfxVolumeSlider.maxValue = 1;
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            UpdateSfxVolume(sfxVolumeSlider.value);
        }

        // Resolution and Display Mode are usually correct by default, but let's ensure:
        if (displayModeDropdown != null)
        {
             // 0=Full, 1=Windowed, 2=Borderless
             // Sync with current screen state which might have been set by OS or previous run
             if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) displayModeDropdown.value = 0;
             else if (Screen.fullScreenMode == FullScreenMode.Windowed) displayModeDropdown.value = 1;
             else displayModeDropdown.value = 2; // Treat others as borderless/maximized

             
             displayModeDropdown.RefreshShownValue();
        }
    }
    
    void SwitchTab(int tabIndex)
    {
        currentTabIndex = tabIndex;
        
        // Update tab button colors
        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] == null) continue;
            TextMeshProUGUI buttonText = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = (i == tabIndex) ? activeTabColor : inactiveTabColor;
            }
        }
        
        // Show/hide panels
        for (int i = 0; i < tabPanels.Count; i++)
        {
            if (tabPanels[i] != null)
                tabPanels[i].SetActive(i == tabIndex);
        }
    }
    
    void NextTab()
    {
        int nextIndex = (currentTabIndex + 1) % tabButtons.Count;
        SwitchTab(nextIndex);
    }
    
    void PreviousTab()
    {
        int prevIndex = (currentTabIndex - 1 + tabButtons.Count) % tabButtons.Count;
        SwitchTab(prevIndex);
    }
    
    void UpdateFovValue(float value)
    {
        if (fovValueText != null)
            fovValueText.text = Mathf.RoundToInt(value).ToString();
    }
    
    void UpdateMasterVolume(float value)
    {
        if (masterVolumeText != null)
            masterVolumeText.text = value.ToString("F2");
    }
    
    void UpdateMusicVolume(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = value.ToString("F2");
    }
    
    void UpdateSfxVolume(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = value.ToString("F2");
    }
    
    void UpdateResolutionValue(int index)
    {
        if (resolutionValueText != null && resolutionDropdown != null && resolutionDropdown.options.Count > index)
            resolutionValueText.text = resolutionDropdown.options[index].text;
    }
    
    void UpdateDisplayModeValue(int index)
    {
        if (displayModeValueText != null && displayModeDropdown != null && displayModeDropdown.options.Count > index)
            displayModeValueText.text = displayModeDropdown.options[index].text;
    }
    
    void UpdateQualityValue(int index)
    {
        if (qualityValueText != null && qualityDropdown != null && qualityDropdown.options.Count > index)
            qualityValueText.text = qualityDropdown.options[index].text;
    }
    void ApplySettings()
    {
        Debug.Log("[SettingsMenuManager] Applying settings...");
        
        // Apply resolution
        if (resolutionDropdown != null && resolutionDropdown.options.Count > 0)
        {
            string[] res = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
            if (res.Length == 2)
            {
                int width = int.Parse(res[0]);
                int height = int.Parse(res[1]);
                Screen.SetResolution(width, height, Screen.fullScreenMode);
            }
        }
        
        // Apply display mode
        if (displayModeDropdown != null)
        {
            switch (displayModeDropdown.value)
            {
                case 0: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
                case 1: Screen.fullScreenMode = FullScreenMode.Windowed; break;
                case 2: Screen.fullScreenMode = FullScreenMode.MaximizedWindow; break;
            }
        }
        
        // Apply quality
            // Save our custom index: 0=Low, 1=High
            PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
            
            // Apply immediately if GameSettingsManager exists
            if (GameSettingsManager.Instance != null)
            {
               GameSettingsManager.Instance.ApplyQuality();
            }
            else 
            {
                // Fallback direct application
                string[] names = QualitySettings.names;
                if (names.Length > 0)
                {
                    int targetLevel = (qualityDropdown.value == 0) ? 0 : names.Length - 1;
                    QualitySettings.SetQualityLevel(targetLevel, true);
                }
            }
        
        // FOV DISABILITATO - causa troppi bug con scene additive
        // Il FOV rimane quello di default impostato sulla camera
        /*
        if (fovSlider != null)
        {
            float fovValue = fovSlider.value;
            PlayerPrefs.SetFloat("FOV", fovValue);
            // ...
        }
        */
        
        // Apply volumes
        if (masterVolumeSlider != null)
        {
            AudioListener.volume = masterVolumeSlider.value;
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            if (GameSettingsManager.Instance != null) GameSettingsManager.Instance.ApplyAudio();
        }
        
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        
        PlayerPrefs.Save();
        Debug.Log("[SettingsMenuManager] Settings applied!");
    }
    
    void ReturnToMenu()
    {
        Debug.Log("[SettingsMenuManager] Returning to Menu...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    
    void ReturnToDesktop()
    {
        Debug.Log("[SettingsMenuManager] Quitting to Desktop...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    void Back()
    {
        Debug.Log("[SettingsMenuManager] Back - closing settings menu");
        PauseMenuController.RequestResume();
    }
    
    void Update()
    {
        // Arrow key navigation
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousTab();
            
        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextTab();
            
        // Hotkeys
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayKeySound();
            ApplySettings();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayKeySound();
            Back();
        }
    }
    
    void PlayKeySound() {
        if (keyPressSound != null) {
            if (audioSource == null) {
                audioSource = GetComponent<AudioSource>();
                
                if (audioSource == null) {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                }
            }
            audioSource.PlayOneShot(keyPressSound, GameSettingsManager.SFXVolume);
        }
    }
}
