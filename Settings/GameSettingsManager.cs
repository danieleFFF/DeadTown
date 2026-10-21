using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//Singleton that persists across scenes and ensures settings (FOV, Audio, Quality) are applied.
public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance { get; private set; }
    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 1f;
    public static float SFXVolume { get; private set; } = 1f;

    [Header("Render Pipeline Assets")]
    [Tooltip("Assegna qui Mobile_RPAsset (per LOW)")]
    public UniversalRenderPipelineAsset lowQualityAsset;
    [Tooltip("Assegna qui PC_RPAsset (per HIGH)")]
    public UniversalRenderPipelineAsset highQualityAsset;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else {
            Destroy(gameObject);
        }
    }
    
    private void Start() {
        ApplyAllSettings();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        ApplyAllSettings();
    }

    public void ApplyAllSettings() {
        ApplyQuality();
        //ApplyFOV();
        ApplyAudio();
    }

    public void ApplyQuality()
    {
        // Default to High (1) if not set
        int qualityIndex = PlayerPrefs.GetInt("QualityLevel", 1); 
        // 0 = Low, 1 = High
        
        // Cambia il Render Pipeline Asset
        if (qualityIndex == 0 && lowQualityAsset != null)
        {
            // LOW: usa Mobile_RPAsset
            GraphicsSettings.defaultRenderPipeline = lowQualityAsset;
            QualitySettings.renderPipeline = lowQualityAsset;
            Debug.Log("[GameSettingsManager] Applied LOW quality with Mobile_RPAsset");
        }
        else if (highQualityAsset != null)
        {
            // HIGH: usa PC_RPAsset
            GraphicsSettings.defaultRenderPipeline = highQualityAsset;
            QualitySettings.renderPipeline = highQualityAsset;
            Debug.Log("[GameSettingsManager] Applied HIGH quality with PC_RPAsset");
        }
        
        // Applica anche il Quality Level di Unity
        string[] names = QualitySettings.names;
        if (names.Length > 0)
        {
            int targetLevel = 0;
            if (qualityIndex == 0) targetLevel = 0; // Low
            else targetLevel = names.Length - 1; // High (Max available)

            if (QualitySettings.GetQualityLevel() != targetLevel)
            {
                QualitySettings.SetQualityLevel(targetLevel, true);
            }
        }
        
        // Imposta il target frame rate per LOW (60 FPS stabili)
        if (qualityIndex == 0)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0; // Disabilita VSync per garantire 60fps
        }
        else
        {
            Application.targetFrameRate = -1; // Nessun limite per HIGH
            QualitySettings.vSyncCount = 1; // VSync abilitato
        }
    }

/*
    public void ApplyFOV()
    {
        float fov = PlayerPrefs.GetFloat("FOV", 80f);
        
        // Cerca tutte le camere in tutte le scene (incluse quelle disattivate)
        Camera[] allCameras = Resources.FindObjectsOfTypeAll<Camera>();
        bool foundAny = false;
        
        foreach (Camera cam in allCameras)
        {
            // Salta camere nella scena Settings o senza scene valida
            if (cam == null || !cam.gameObject.scene.IsValid()) continue;
            if (cam.gameObject.scene.name == "Settings") continue;
            
            // Applica FOV a Main Camera e WeaponCamera
            if (cam.CompareTag("MainCamera") || cam.name.Contains("Main") || cam.name.Contains("Weapon"))
            {
                cam.fieldOfView = fov;
                foundAny = true;
                Debug.Log($"[GameSettingsManager] Applied FOV {fov} to camera: {cam.name}");
            }
        }
        
        // Fallback: prova Camera.main
        if (!foundAny)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.fieldOfView = fov;
                Debug.Log($"[GameSettingsManager] Applied FOV {fov} to Camera.main");
            }
        }
    }
*/
    public void ApplyAudio() {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        AudioListener.volume = MasterVolume;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
