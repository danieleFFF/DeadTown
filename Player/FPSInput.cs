using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]
public class FPSInput : MonoBehaviour
{
    public float gravity = -9.8f;
    public float jumpForce = 7f;
    [Header("Sprint Settings")]
    [SerializeField] private float sprintSpeedMultiplier = 1.3f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOVBonus = 10f; //FOV increases
    [SerializeField] private float fovTransitionSpeed = 8f;
    private float sprintFOV;
    private CharacterController charController;
    private PlayerCharacter playerChar;
    private float verticalVelocity = -2f;
    private bool isClimbing = false; //Managed by LadderClimb script
    private Camera playerCamera;
    private bool isSprinting = false;
    private float currentFOV;
    private WeaponAiming cachedWeaponAiming; //Cached to prevent Find every frame
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;

    void Start() {
        charController = GetComponent<CharacterController>();
        playerChar = GetComponent<PlayerCharacter>();
        playerCamera = Camera.main;
        cachedWeaponAiming = FindFirstObjectByType<WeaponAiming>();
        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        if (playerCamera != null) {
            //Reads FOV from PlayerPrefs and not from the camera (could have serialized values in the Inspector)
            normalFOV = 60f;
            sprintFOV = normalFOV + sprintFOVBonus;
            currentFOV = normalFOV;
            playerCamera.fieldOfView = normalFOV; //Apples correct FOV corretto
        }
    }

    void Update() {
        if (playerChar.IsDead || playerChar.IsReviving) return;

        //Movimento verticale gestito da Ladder
        if (isClimbing) return;

        //Shift + W (works while jumping too for momentum)
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0.1f;
        isSprinting = wantsToSprint; //Removed isGrounded check to maintain momentum while jumping
        
        UpdateFOV();
        float currentSpeed = playerChar.GetCurrentSpeed();
        
        if (isSprinting) {
            currentSpeed *= sprintSpeedMultiplier;
        }
        
        float deltaX = Input.GetAxis("Horizontal") * currentSpeed;
        float deltaZ = Input.GetAxis("Vertical") * currentSpeed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, currentSpeed);

        //Jump/gravity
        if (charController.isGrounded) {
            //Negative velocity to maintain CharacterController on the ground (prevents isGrounded flickering -> no jump)
            verticalVelocity = -2f; 
            
            if (Input.GetButtonDown("Jump")) 
                verticalVelocity = jumpForce;
        } else {
            //Applies gravity
            verticalVelocity += gravity * Time.deltaTime;
        }

        movement.y = verticalVelocity;
        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        bool wasGrounded = charController.isGrounded;
        charController.Move(movement);
        
        if (!wasGrounded && charController.isGrounded) { PlayFootstepSound(); }
        
        //Footstep sounds
        float horizontalSpeed = new Vector2(deltaX, deltaZ).magnitude;
        
        //Walking
        if (charController.isGrounded && horizontalSpeed > 2f && !isClimbing) {
            footstepTimer -= Time.deltaTime; //Time remaining for next footstep sound
            float currentInterval = isSprinting ? footstepInterval * 0.7f : footstepInterval; //Faster sprint = faster steps

            if (footstepTimer <= 0) {
                PlayFootstepSound();
                footstepTimer = currentInterval;
            }
        }
    }
    
    private void PlayFootstepSound() {
        if (audioSource != null && footstepSounds.Length > 0) {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)]; //Random sound
            audioSource.pitch = Random.Range(0.9f, 1.1f); //Varies pitch for realism
            float volume = isSprinting ? 0.6f : 0.4f; //Lower volume for crouching, higher volume for sprinting

            if (GameSettingsManager.Instance != null) volume *= GameSettingsManager.SFXVolume;
            
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    
    void UpdateFOV() {
        if (playerCamera == null) return;

        //If not aiming, don't touch FOV because  WeaponAiming manages it
        if (cachedWeaponAiming != null && cachedWeaponAiming.IsAiming()) {
            currentFOV = playerCamera.fieldOfView; //FOV transition when player stops aiming
            return;
        }
        
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * fovTransitionSpeed);
        playerCamera.fieldOfView = currentFOV;
    }
    

    //Chiamato dal LadderClimb per disabilitare il movimento normale
    public void SetClimbing(bool climbing)
    {
        isClimbing = climbing;
        if (climbing) {
            verticalVelocity = 0f; //Reset velocità verticale quando inizia a scalare
        }
    }

    //Resetta la velocità verticale (utile dopo un teletrasporto)
    public void ResetVelocity() {
        verticalVelocity = 0f;
    }
    
    public bool IsSprinting() => isSprinting;
}

