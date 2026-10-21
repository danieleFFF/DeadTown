using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class UnlockableObj : MonoBehaviour {
    [Header("Unlock Animation")]
    public float moveDistance = 3f;
    public float moveDuration = 1.5f;
    public bool destroyAfterMove = true;
    public TextMeshProUGUI unlockableObjText;
    public string objectName;
    public int objectCost;
    public float rotationSpeed = 180f;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip unlockClip;
    private bool playerInside;
    private PointsDisplay playerPoints;
    private PlayerCharacter playerCharacter;
    private bool isUnlocked = false;

    void Start()
    {
        if (unlockableObjText != null) {
            unlockableObjText.outlineWidth = 0.15f;
            unlockableObjText.outlineColor = Color.black;
        }
    }

    void Update()
    {
        if (!playerInside || isUnlocked) return;

        if (Input.GetKeyDown(KeyCode.F)) {
            if (playerPoints.GetPoints() >= objectCost)
            {
                playerPoints.RemovePoints(objectCost);
                unlockableObjText.gameObject.SetActive(false);
                isUnlocked = true;
                
                //Activates any child SpawnZones
                SpawnZone[] childSpawnZones = GetComponentsInChildren<SpawnZone>(true);
                
                foreach (SpawnZone zone in childSpawnZones) zone.Unlock();
                
                //Plays 'unlock' sound without being cut off
                if (unlockClip) {
                    float volume = 1f;
                    GameObject settings = GameObject.Find("GameSettingsManager");

                    if (settings != null) {
                        var manager = settings.GetComponent<GameSettingsManager>();

                        if (manager != null) volume = GameSettingsManager.SFXVolume;
                    }
                    PlaySoundIndependent(unlockClip, volume);
                }
                StartCoroutine(MoveAndDisappear());
            } else {
                unlockableObjText.text = "Insufficient points!";
            }
        }
    }
    
    //Temporary object to play sound
    void PlaySoundIndependent(AudioClip clip, float volume) {
        GameObject audioObj = new GameObject("UnlockSound_Temp_" + objectName);
        
        //Spawns sound at Player's position so it's heard clearly
        if (playerPoints != null) audioObj.transform.position = playerPoints.transform.position;
        else audioObj.transform.position = transform.position;
        
        AudioSource tempSource = audioObj.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 1f; //3D sound
        
        //Copies settings from main audio source if available
        if (audioSource != null) {
            tempSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            tempSource.minDistance = audioSource.minDistance;
            tempSource.maxDistance = audioSource.maxDistance;
            tempSource.rolloffMode = audioSource.rolloffMode;
        } else {
            tempSource.minDistance = 2f;
            tempSource.maxDistance = 20f;
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        tempSource.Play();
        Destroy(audioObj, clip.length + 0.2f);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetPlayerStatus(true, other);
        }
    }

    void OnTriggerExit(Collider other){
        if (other.CompareTag("Player")) {
            SetPlayerStatus(false, other);
        }
    }
    public void SetPlayerStatus(bool isInside, Collider player)
    {
        if (isUnlocked) return;

        if (isInside)
        {
            playerInside = true;
            playerPoints = player.GetComponent<PointsDisplay>();
            if (playerPoints != null)
            {
                // Enhance visibility
                unlockableObjText.outlineWidth = 0.15f;
                unlockableObjText.outlineColor = Color.black;
                if (unlockableObjText.fontSharedMaterial != null) unlockableObjText.fontSharedMaterial.EnableKeyword("OUTLINE_ON");
                unlockableObjText.UpdateMeshPadding();

                unlockableObjText.text = $"Press <color=#FFCC00>F</color> to unlock {objectName}: {objectCost} points";
                unlockableObjText.gameObject.SetActive(true);
            }
        }else {
            playerInside = false;
            unlockableObjText.gameObject.SetActive(false);
        }
    }

    IEnumerator MoveAndDisappear()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * moveDistance;
        float elapsed = 0f;
        Collider col = GetComponent<Collider>();
        
        if (col != null) col.enabled = false;

        while (elapsed < moveDuration){
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            //transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        if (destroyAfterMove) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
