using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class AudioProximityTrigger : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Sound to play when entering zone")]
    [SerializeField] private AudioClip proximitySound;
    [Tooltip("If set to false, it plays the sound every time you enter the zone")]
    [SerializeField] private bool playOnce = false;
    private bool hasPlayed = false;
    private AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null) {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; //Default is 3D sound
        }
        
        //The collider is a trigger
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null) box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other) {
        if (playOnce && hasPlayed) return;

        if (other.CompareTag("Player")) {
            //Prevents overlap
            if (audioSource.isPlaying) return;
            
            PlaySound();
        }
    }

    private void PlaySound() {
        if (audioSource && proximitySound) {
            float vol = GameSettingsManager.Instance != null ? GameSettingsManager.SFXVolume : 1f;
            
            //Allow isPlaying check
            audioSource.clip = proximitySound;
            audioSource.volume = vol;
            audioSource.Play();
            hasPlayed = true;
        }
    }
}
