using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private LayerMask targetLayer;
    [Header("Weapon Muzzle")]
    [Tooltip("Offset from camera for raycast origin")]
    [SerializeField] private Vector3 muzzleOffset = new Vector3(0.3f, -0.2f, 0.5f);
    [Header("Points Settings")]
    [SerializeField] private int pointsPerHit = 30, pointsPerHitOnHead = 50 ;
    [Header("Visual Feedback")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float effectLifetime = 1f;
    [Header("Bullet Tracer")]
    [SerializeField] private bool showBulletTracer = true;
    [SerializeField] private float tracerDuration = 0.1f;
    [SerializeField] private float tracerWidth = 0.02f;
    [SerializeField] private Color tracerColor = new Color(0.7f, 0.7f, 0.7f, 0.2f);
    private GameObject currentWeapon;
    private bool isHeadShot = false;
    private PointsDisplay pointsDisplay;
    
    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        currentWeapon = GetComponentInChildren<WeaponManager>().gameObject;
        
        // FORZA IL VALORE - ignora valori serializzati nell'Inspector
        pointsPerHit = 30;
        pointsPerHitOnHead = 50;

        // Find PointsDisplay on player
        pointsDisplay = GetComponentInParent<PointsDisplay>();
        if (pointsDisplay == null)
            pointsDisplay = FindObjectOfType<PointsDisplay>();
    }

    //CHIAMATO DALL'ARMA
    public void ShootWithSpread(float spreadAngleDeg)
    {
        // Calculate muzzle position (offset from camera to simulate weapon position)
        Vector3 muzzlePosition = playerCamera.transform.position + 
                                 playerCamera.transform.right * muzzleOffset.x +
                                 playerCamera.transform.up * muzzleOffset.y +
                                 playerCamera.transform.forward * muzzleOffset.z;
        
        Vector3 dir = playerCamera.transform.forward;
        float yaw = Random.Range(-spreadAngleDeg, spreadAngleDeg);
        float pitch = Random.Range(-spreadAngleDeg, spreadAngleDeg);
        dir = Quaternion.Euler(pitch, yaw, 0f) * dir;
        
        // Raycast from camera center for accurate aiming, but tracer starts from muzzle
        Ray ray = new Ray(playerCamera.transform.position, dir);
        RaycastHit hit;
        
        Vector3 endPoint;
        bool hitZombie = false;

        if (Physics.Raycast(ray, out hit, shootRange, targetLayer))
        {
            endPoint = hit.point;
            
            Debug.Log("Hit: " + hit.collider.name + " at distance: " + hit.distance);
            
            // Check for SoldierTarget (shooting range targets)
            SoldierTarget soldierTarget = hit.transform.GetComponent<SoldierTarget>();
            if (soldierTarget != null)
            {
                soldierTarget.OnHit(hit.point, hit.collider);
                Debug.Log("Soldier target hit at " + hit.collider.name);
            }
            else
            {
                // Check for ReactiveTarget (zombies/enemies)
                // Use GetComponentInParent because colliders might be on child bones (Head, Limbs)
                ReactiveTarget reactiveTarget = hit.transform.GetComponentInParent<ReactiveTarget>();
                if (reactiveTarget != null)
                {
                    // Case-insensitive check for greater robustness
                    string hitName = hit.collider.name.ToLower();
                    isHeadShot = hitName.Contains("head");
                    
                    Debug.Log($"[SHOOT DEBUG] Hit Collider: '{hit.collider.name}' | IsHeadShot: {isHeadShot}");
                    
                    reactiveTarget.ReactToHit(currentWeapon.GetComponent<WeaponManager>(), isHeadShot);
                    hitZombie = true;
                }
            }

            if (hitEffectPrefab != null){
                GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, effectLifetime);
            }

            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
        } else {
            endPoint = ray.origin + ray.direction * shootRange;
            Debug.Log("Missed!");
            Debug.DrawRay(ray.origin, ray.direction * shootRange, Color.yellow, 1f);
        }
        
        // Award points for hitting zombie
        if (hitZombie && pointsDisplay != null)
        {
            // 25 points for headshot, 15 for body
            int pointsToAward = isHeadShot ? pointsPerHitOnHead : pointsPerHit;
            pointsDisplay.AddPoints(pointsToAward);
        }
        
        // Show bullet tracer from muzzle position
        if (showBulletTracer)
        {
            CreateBulletTracer(muzzlePosition, endPoint);
        }
    }
    
    private void CreateBulletTracer(Vector3 startPoint, Vector3 endPoint)
    {
        // Create tracer object
        GameObject tracerObj = new GameObject("BulletTracer");
        LineRenderer lineRenderer = tracerObj.AddComponent<LineRenderer>();
        
        // Configure line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        
        // Line appearance
        lineRenderer.startWidth = tracerWidth;
        lineRenderer.endWidth = tracerWidth * 0.5f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = tracerColor;
        lineRenderer.endColor = new Color(tracerColor.r, tracerColor.g, tracerColor.b, 0.3f);
        
        // Start fade coroutine
        StartCoroutine(FadeAndDestroyTracer(tracerObj, lineRenderer, tracerDuration));
    }
    
    private IEnumerator FadeAndDestroyTracer(GameObject tracerObj, LineRenderer lineRenderer, float duration)
    {
        float elapsed = 0f;
        Color startColorStart = lineRenderer.startColor;
        Color startColorEnd = lineRenderer.endColor;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            
            lineRenderer.startColor = new Color(startColorStart.r, startColorStart.g, startColorStart.b, startColorStart.a * alpha);
            lineRenderer.endColor = new Color(startColorEnd.r, startColorEnd.g, startColorEnd.b, startColorEnd.a * alpha);
            
            yield return null;
        }
        
        Destroy(tracerObj);
    }
}
