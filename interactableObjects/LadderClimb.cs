using UnityEngine;


public class LadderClimb : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 3f; 
    
    private bool playerInZone = false;
    private bool isClimbing = false;
    private CharacterController playerController;
    private FPSInput playerInput;
    private GameObject weaponCamera;

    void Update()
    {
        if (!playerInZone) return;
        if (Input.GetButton("Jump")) 
        {
            if (!isClimbing)
            {
                StartClimbing();
            }
            
            Vector3 climbMovement = Vector3.up * climbSpeed * Time.deltaTime;
            playerController.Move(climbMovement);
        }
        else // Spazio rilasciato
        {
            if (isClimbing)
            {
                StopClimbing();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            playerController = other.GetComponent<CharacterController>();
            playerInput = other.GetComponent<FPSInput>();
            
            Transform weaponCameraTransform = other.transform.Find("WeaponCamera");
            if (weaponCameraTransform != null)
            {
                weaponCamera = weaponCameraTransform.gameObject;
            }
            
            // Mostra messaggio di interazione
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.ShowMessage("Press <color=#FFCC00>SPACE</color> to climb");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isClimbing)
            {
                StopClimbing();
            }
            
            playerInZone = false;
            playerController = null;
            playerInput = null;
            weaponCamera = null;
            
            // Nascondi messaggio di interazione
            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.HideMessage();
            }
        }
    }

    void StartClimbing()
    {
        isClimbing = true;
        if (playerInput != null)
        {
            playerInput.SetClimbing(true);
        }
        if (weaponCamera != null)
        {
            weaponCamera.SetActive(false);
        }
    }

    void StopClimbing()
    {
        isClimbing = false;
        if (playerInput != null)
        {
            playerInput.SetClimbing(false);
        }
        if (weaponCamera != null)
        {
            weaponCamera.SetActive(true);
        }
    }
}
