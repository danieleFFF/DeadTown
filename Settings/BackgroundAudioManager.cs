using System.Collections;
using UnityEngine;

public class BackgroundAudioManager : MonoBehaviour {
    [Header("Global Settings")]
    [Tooltip("If true, it updates volume in real time when settings change")]
    [SerializeField] private bool updateRealtime = true;
    [Header("Audio 1")]
    [SerializeField] private AudioSource layer1Source;
    [SerializeField] private AudioClip layer1Clip;
    [Range(0f, 1f)] [SerializeField] private float layer1Volume = 0.5f;
    [Header("Audio 2")]
    [SerializeField] private AudioSource layer2Source;
    [SerializeField] private AudioClip layer2Clip;
    [Range(0f, 1f)] [SerializeField] private float layer2Volume = 0.5f;

    void Awake() {
        if (layer1Source) layer1Source.Stop();
        if (layer2Source) layer2Source.Stop();
    }

    IEnumerator Start() {
        yield return new WaitForSeconds(0.25f); //Delay before playing audio
        PlayLayer(layer1Source, layer1Clip, layer1Volume);
        PlayLayer(layer2Source, layer2Clip, layer2Volume);
    }

    void Update() {
        if (updateRealtime) {
             UpdateVolume(layer1Source, layer1Volume);
             UpdateVolume(layer2Source, layer2Volume);
        }
    }

    void PlayLayer(AudioSource source, AudioClip clip, float baseVolume) {
        if (source != null && clip != null) {
            source.clip = clip; //Sets audio.
            source.loop = true;
            source.playOnAwake = false;
            UpdateVolume(source, baseVolume);
            
            if (!source.isPlaying) source.Play();
        }
    }

    void UpdateVolume(AudioSource source, float baseVolume) {
        if (source == null) return;
        
        float globalMult = 1f;

        //Uses MusicVolume for background tracks.
        if (GameSettingsManager.Instance != null) globalMult = GameSettingsManager.MusicVolume; 

        source.volume = baseVolume * globalMult;
    }
}
