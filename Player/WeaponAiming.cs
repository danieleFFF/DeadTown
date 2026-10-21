using UnityEngine;
using UnityEngine.UI;

public class WeaponAiming : MonoBehaviour
{
    [Header("Aim Settings")]
    [SerializeField] private KeyCode aimKey = KeyCode.Mouse1; // Right mouse button
    [SerializeField] private float aimSpeed = 8f;
    
    [Header("FOV Settings")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Quanto zoom applicare in mira (0.65 = 35% zoom)")]
    [Range(0.5f, 0.9f)]
    [SerializeField] private float aimFOVMultiplier = 0.65f;
    private float normalFOV = 60f;
    private float aimFOV = 52f;
    
    [Header("Weapon Position (ADS)")]
    [SerializeField] private Transform weaponHolder; // L'oggetto che contiene l'arma
    [SerializeField] private Vector3 normalPosition = Vector3.zero;
    [SerializeField] private Vector3 aimPosition = new Vector3(0f, -0.1f, 0.2f);
    
    [Header("Sensitivity Reduction")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private float normalSensitivity = 5f;
    [SerializeField] private float aimSensitivity = 3.5f;
    
    [Header("Crosshair")]
    [SerializeField] private CrosshairUI crosshair;
    [SerializeField] private bool hideCrosshairWhenAiming = true;
    
    [Header("Aim Dot (Pallino)")]
    [SerializeField] private bool showAimDot = true;
    [SerializeField] private Color aimDotColor = Color.white;
    [SerializeField] private float aimDotSize = 4f;
    private GameObject aimDotObject;
    private Image aimDotImage;
    
    private bool isAiming = false;
    private float currentFOV;
    private Vector3 currentWeaponPosition;
    
    private void Start()
    {
        // Auto-find components se non assegnati
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        if (mouseLook == null)
            mouseLook = GetComponentInParent<MouseLook>();
            
        if (crosshair == null)
            crosshair = FindFirstObjectByType<CrosshairUI>();
            
        if (weaponHolder == null)
        {
            // Cerca il WeaponManager nel player
            WeaponManager wm = FindFirstObjectByType<WeaponManager>();
            if (wm != null)
                weaponHolder = wm.transform;
        }
        
        // Leggi normalFOV dalla camera (gestito da FPSInput)
        if (playerCamera != null)
        {
            normalFOV = playerCamera.fieldOfView;
            currentFOV = normalFOV;
        }
        
        // Calcola aimFOV come percentuale del normalFOV (es. 65% = zoom del 35%)
        aimFOV = normalFOV * aimFOVMultiplier;
        
        if (weaponHolder != null)
        {
            normalPosition = weaponHolder.localPosition;
            currentWeaponPosition = normalPosition;
        }
        
        if (mouseLook != null)
        {
            normalSensitivity = mouseLook.sensitivityHor;
        }
        
        // Crea il pallino di mira
        CreateAimDot();
    }
    
    private void CreateAimDot()
    {
        if (!showAimDot) return;
        
        // Trova o crea la canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[WeaponAiming] Canvas non trovata per creare il pallino di mira!");
            return;
        }
        
        // Crea il GameObject per il pallino
        aimDotObject = new GameObject("AimDot");
        aimDotObject.transform.SetParent(canvas.transform, false);
        
        // Aggiungi RectTransform
        RectTransform rectTransform = aimDotObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(aimDotSize, aimDotSize);
        
        // Aggiungi Image component
        aimDotImage = aimDotObject.AddComponent<Image>();
        aimDotImage.color = aimDotColor;
        
        // Crea una texture circolare
        Texture2D circleTexture = CreateCircleTexture(32);
        Sprite circleSprite = Sprite.Create(
            circleTexture,
            new Rect(0, 0, circleTexture.width, circleTexture.height),
            new Vector2(0.5f, 0.5f)
        );
        aimDotImage.sprite = circleSprite;
        
        // Inizialmente nascosto
        aimDotObject.SetActive(false);
    }
    
    private Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float center = size / 2f;
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (distance <= radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private void Update()
    {
        // Disabilita tutta la logica della mira se il menu settings è aperto
        if (PauseMenuController.IsPaused)
        {
            // Se stava mirando, ferma la mira
            if (isAiming)
            {
                StopAiming();
            }
            return;
        }
        
        // Check input per mirare
        bool wantsToAim = Input.GetKey(aimKey);
        
        if (wantsToAim && !isAiming)
        {
            StartAiming();
        }
        else if (!wantsToAim && isAiming)
        {
            StopAiming();
        }
        
        // Smooth transitions
        UpdateAimTransitions();
    }
    
    private void StartAiming()
    {
        isAiming = true;
        
        // Sincronizza currentFOV con il FOV attuale della camera per una transizione fluida
        if (playerCamera != null)
        {
            currentFOV = playerCamera.fieldOfView;
            // Aggiorna anche normalFOV nel caso sia cambiato
            normalFOV = currentFOV;
            aimFOV = normalFOV * aimFOVMultiplier;
        }
        
        // Nascondi crosshair se richiesto
        if (hideCrosshairWhenAiming && crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
        }
        
        // Mostra il pallino di mira
        if (showAimDot && aimDotObject != null)
        {
            aimDotObject.SetActive(true);
        }
    }
    
    private void StopAiming()
    {
        isAiming = false;
        
        // Mostra crosshair
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(true);
        }
        
        // Nascondi il pallino di mira
        if (aimDotObject != null)
        {
            aimDotObject.SetActive(false);
        }
    }
    
    private void UpdateAimTransitions()
    {
        // Target values
        float targetFOV = isAiming ? aimFOV : normalFOV;
        Vector3 targetPosition = isAiming ? aimPosition : normalPosition;
        float targetSensitivity = isAiming ? aimSensitivity : normalSensitivity;
        
        // Smooth FOV
        if (playerCamera != null)
        {
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * aimSpeed);
            playerCamera.fieldOfView = currentFOV;
        }
        
        // Smooth weapon position
        if (weaponHolder != null)
        {
            currentWeaponPosition = Vector3.Lerp(currentWeaponPosition, targetPosition, Time.deltaTime * aimSpeed);
            weaponHolder.localPosition = currentWeaponPosition;
        }
        
        // Smooth sensitivity (solo orizzontale e verticale, mantiene sincroni)
        if (mouseLook != null)
        {
            float newSens = Mathf.Lerp(mouseLook.sensitivityHor, targetSensitivity, Time.deltaTime * aimSpeed);
            mouseLook.sensitivityHor = newSens;
            mouseLook.sensitivityVer = newSens; // Mantieni stesso valore
        }
    }
    
    // Metodo pubblico per sapere se si sta mirando (utile per altri script)
    public bool IsAiming()
    {
        return isAiming;
    }
    
    // Metodo per forzare l'uscita dalla mira (utile per reload, switch arma, etc)
    public void ForceStopAiming()
    {
        if (isAiming)
        {
            StopAiming();
        }
    }
}
