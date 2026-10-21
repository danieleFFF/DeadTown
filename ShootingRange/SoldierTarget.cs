using UnityEngine;
using ShootingRange;
public class SoldierTarget : MonoBehaviour
{
    [Header("Target Parts")]
    [SerializeField] private Collider headCollider;
    [SerializeField] private Collider heartCollider;
    [Header("Points Settings")]
    [SerializeField] private int headshotPoints = 10;
    [SerializeField] private int hearthitPoints = 10;
    [SerializeField] private int bodyHitPoints = 5;
    [Header("Rotation Settings")]
    [SerializeField] private float rotationDuration = 0.5f; // Durata animazione in secondi
    [SerializeField] private bool rotateOnHit = true;

    private int totalScore = 0;
    private Quaternion initialRotation;
    private Renderer[] renderers;
    private bool hasBeenHit = false;
    private ShootingRangeManager gameManager;

    private void Awake()
    {
        CacheRenderers();
    }

    private void Start()
    {
        gameManager = FindObjectOfType<ShootingRangeManager>();
        if (headCollider == null)
        {
            Transform headTransform = transform.Find("head");
            if (headTransform != null)
            {
                headCollider = headTransform.GetComponent<Collider>();
            }
        }

        if (heartCollider == null) {
            Transform heartTransform = transform.Find("heart");
            if (heartTransform != null){
                heartCollider = heartTransform.GetComponent<Collider>();
            }
        }
    }
    private void OnEnable()
    {
        initialRotation = transform.rotation;
        hasBeenHit = false;
        ShowTarget();
    }

    private void CacheRenderers(){
        renderers = GetComponentsInChildren<Renderer>(true);
    }



    private void HideTarget()
    {
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }
    }

    private void ShowTarget()
    {
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
        }
    }

    public void OnHit(Vector3 hitPoint, Collider hitCollider)
    {
        if (hasBeenHit)
            return;
        hasBeenHit = true;
        int pointsAwarded = 0;
        string hitLocation = "Body";

        if (hitCollider == headCollider)
        {
            pointsAwarded = headshotPoints;
            hitLocation = "Head";
            Debug.Log("HEADSHOT! +" + pointsAwarded + " points");
        } else if (hitCollider == heartCollider)
        {
            pointsAwarded = hearthitPoints;
            hitLocation = "Heart";
            Debug.Log("HEART HIT! +" + pointsAwarded + " points");
        } else
        {
            pointsAwarded = bodyHitPoints;
            Debug.Log("Body hit! +" + pointsAwarded + " points");
        }

        totalScore += pointsAwarded;
        Debug.Log("Total Score: " + totalScore);

        if (gameManager != null && gameManager.IsGameActive()){
            gameManager.OnTargetHit(pointsAwarded);
        }

        if (rotateOnHit)
            StartCoroutine(RotateAndHideTarget());
    }
    
    private System.Collections.IEnumerator RotateAndHideTarget()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(-90f, 0f, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationDuration;
            
            // Smooth interpolation (ease-in-out)
            t = t * t * (3f - 2f * t);
            
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        
        // Assicura rotazione finale precisa
        transform.rotation = endRotation;
        
        // Nascondi il target
        HideTarget();
    }

    public int GetScore()
    {
        return totalScore;
    }

    public void ResetTarget()
    {
        // Ferma eventuali coroutine attive
        StopAllCoroutines();
        
        totalScore = 0;
        hasBeenHit = false;
        
        if (renderers == null || renderers.Length == 0)
            CacheRenderers();
        
        transform.rotation = initialRotation;
        ShowTarget();
    }
}
