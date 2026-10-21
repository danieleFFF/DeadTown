using UnityEngine;

//Attach to a 3D object in the Menuscene. Opens a credits popup when clicked.
public class EasterEggCredits : MonoBehaviour
{
    [Header("Credits Popup")]
    [Tooltip("Drag here credits UI panel (disabled by default)")]
    [SerializeField] private GameObject creditsPopup;
    [Header("Outline")]
    [Tooltip("Drag here panel's child (disabled by default)")]
    [SerializeField] private GameObject outlineObject;
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip closeSound;
    private bool isHovering = false;
    private AudioSource audioSource;
    
    void Start() {
        audioSource = GetComponent<AudioSource>();

        //Checks
        if (audioSource == null && (hoverSound != null || clickSound != null || closeSound != null)) {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (outlineObject != null) outlineObject.SetActive(false);
        if (creditsPopup != null) creditsPopup.SetActive(false);
    }
    
    void OnMouseEnter() {
        if (isHovering) return;
        
        //Does nothing if popup already open
        if (creditsPopup != null && creditsPopup.activeSelf) return;
        
        isHovering = true;
        
        //Shows child
        if (outlineObject != null) outlineObject.SetActive(true);
        
        //Reproduces hover sound
        if (audioSource != null && hoverSound != null) {
            float sfxVol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.PlayOneShot(hoverSound, 0.5f * sfxVol);
        }
    }
    
    void OnMouseExit() {
        isHovering = false;
        
        //Hides child
        if (outlineObject != null) outlineObject.SetActive(false);
    }
    
    void OnMouseDown() {
        //Does nothing if popup already open
        if (creditsPopup != null && creditsPopup.activeSelf) return;
        
        //Open popup
        if (creditsPopup != null) {
            creditsPopup.SetActive(true);

            //Reproduces click sound
            if (audioSource != null && clickSound != null) {
                float sfxVol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
                audioSource.PlayOneShot(clickSound, sfxVol);
            }
        } else {
            Debug.LogWarning("Credits popup not assigned");
        }
    }
    
    //'X' button.
    public void CloseCredits() {
        //Reproduces close sound
        if (audioSource != null && closeSound != null) {
            float sfxVol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            audioSource.PlayOneShot(closeSound, sfxVol);
        }
        
        if (creditsPopup != null) creditsPopup.SetActive(false);
    }
    
    void Update() {
        //Popup can be closed with ESC
        if (creditsPopup != null && creditsPopup.activeSelf && Input.GetKeyDown(KeyCode.Escape)) 
            CloseCredits();
    }
    
    void OnDestroy()
    {
        //Hides child when destroyed
        if (outlineObject != null) outlineObject.SetActive(false);
    }
}
