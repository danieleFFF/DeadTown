using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Attach to button for hover and click sounds.
public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.7f;
    private static AudioSource sharedAudioSource;
    
    void Awake() {
        //Shared AudioSource for every button
        if (sharedAudioSource == null) {
            GameObject audioObj = new GameObject("ButtonAudioSource");
            sharedAudioSource = audioObj.AddComponent<AudioSource>();
            sharedAudioSource.playOnAwake = false;
            DontDestroyOnLoad(audioObj); //Doesn't destroy object when changing scene
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData) {
        if (hoverSound != null && sharedAudioSource != null) {
            float sfxVol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f; //Sets volume
            sharedAudioSource.PlayOneShot(hoverSound, hoverVolume * sfxVol); //Plays sound
        }
    }
    
    public void OnPointerClick(PointerEventData eventData) {
        if (clickSound != null && sharedAudioSource != null) {
            float sfxVol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            sharedAudioSource.PlayOneShot(clickSound, clickVolume * sfxVol);
        }
    }
}
