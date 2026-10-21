using UnityEngine;
using TMPro;

/// <summary>
/// Attach this script to collectible objects in the scene.
/// Player can collect them by pressing F when near.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private string collectibleName = "Secret Item";
    [SerializeField] private bool destroyOnCollect = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private bool floatObject = true;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed = 2f;
    
    private bool isPlayerNear = false;
    private bool isCollected = false;
    private Vector3 startPosition;
    private GameObject promptUI;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Visual effects
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        if (floatObject)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void Collect()
    {
        if (isCollected) return;
        isCollected = true;
        
        Debug.Log($"[Collectible] Collected: {collectibleName}");
        
        // Notify manager
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.OnCollectibleCollected(this);
        }
        else
        {
            Debug.LogWarning("[Collectible] CollectibleManager not found!");
        }
        
        // Hide prompt
        if (promptUI != null)
            Destroy(promptUI);
        
        // Destroy or disable
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isCollected) return;
        
        // Auto-collect when player touches the collectible
        Collect();
    }
}
